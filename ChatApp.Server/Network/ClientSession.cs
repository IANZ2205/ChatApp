using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ChatApp.Common;
using ChatApp.Common.Protocol;
using ChatApp.Server.Services;

namespace ChatApp.Server.Network
{
    public class ClientSession
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly object _sendLock = new object();
        private readonly Action<string> _log;
        private readonly CommandRouter _router;

        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string RemoteAddress { get; private set; } = "";
        public DateTime ConnectedAt { get; } = DateTime.UtcNow;
        public event Action<ClientSession>? Disconnected;

        public ClientSession(TcpClient tcpClient, CommandRouter router, Action<string> log)
        {
            _tcpClient = tcpClient;
            _router = router;
            RemoteAddress = tcpClient.Client.RemoteEndPoint?.ToString() ?? "unknown";
            _stream = tcpClient.GetStream();
            _stream.WriteTimeout = 5000;
            _reader = new StreamReader(_stream, new UTF8Encoding(false));
            _writer = new StreamWriter(_stream, new UTF8Encoding(false)) { AutoFlush = true };
            _log = log;
        }

        public void Send(Envelope envelope)
        {
            try
            {
                string json = JsonSerializer.Serialize(envelope, Envelope.Options);
                lock (_sendLock) { _writer.WriteLine(json); }
                Stats.BytesSent += Encoding.UTF8.GetByteCount(json) + 1;
            }
            catch (Exception ex)
            {
                _log($"Send error to {Username ?? "unknown"}: {ex.Message}");
            }
        }

        public void ForceClose()
        {
            try { _tcpClient?.Close(); } catch { }
        }

        public void Run()
        {
            string remote = RemoteAddress;
            _log($"New connection from {remote}");
            Stats.ConnectionsTotal++;

            try
            {
                string? line;
                while ((line = _reader.ReadLine()) != null)
                {
                    Stats.BytesReceived += Encoding.UTF8.GetByteCount(line) + 1;

                    if (line.Length > Constants.MaxLineLength)
                    {
                        _log($"Line too long from {remote} ({line.Length} chars), dropping.");
                        break;
                    }

                    Envelope? env = null;
                    try { env = JsonSerializer.Deserialize<Envelope>(line, Envelope.Options); }
                    catch (JsonException) { }

                    if (env != null) _router.Handle(this, env);
                }
            }
            catch (Exception ex)
            {
                _log($"Session error ({remote}): {ex.Message}");
            }
            finally
            {
                try { _reader?.Close(); } catch { }
                try { _writer?.Close(); } catch { }
                try { _stream?.Close(); } catch { }
                try { _tcpClient?.Close(); } catch { }
                _log($"Disconnected: {remote}");
                Disconnected?.Invoke(this);
            }
        }
    }
}