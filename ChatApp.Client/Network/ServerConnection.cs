using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using ChatApp.Common.Protocol;

namespace ChatApp.Client.Network
{
    public class ServerConnection
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private Thread? _listenThread;
        private volatile bool _running;

        public event Action<string>? Log;
        public event Action<Envelope>? MessageReceived;
        public event Action? Disconnected;

        public bool IsConnected => _running && _tcpClient?.Connected == true;

        public void Connect(string host, int port)
        {
            if (_running) return;

            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(host, port);
                _stream = _tcpClient.GetStream();
                _reader = new StreamReader(_stream, new UTF8Encoding(false));
                _writer = new StreamWriter(_stream, new UTF8Encoding(false)) { AutoFlush = true };
                _running = true;

                _listenThread = new Thread(ListenLoop) { IsBackground = true };
                _listenThread.Start();

                Log?.Invoke($"Connected to {host}:{port}");
            }
            catch (Exception ex)
            {
                Log?.Invoke($"Connection failed: {ex.Message}");
                Disconnect();
            }
        }

        public void Send(Envelope envelope)
        {
            if (!IsConnected) { Log?.Invoke("Send skipped: not connected."); return; }
            try
            {
                string json = JsonSerializer.Serialize(envelope, Envelope.Options);
                _writer!.WriteLine(json);
                System.Diagnostics.Debug.WriteLine($"[Send] {envelope.Type} ({json.Length} chars)");
            }
            catch (Exception ex)
            {
                Log?.Invoke($"Send error: {ex.Message}");
            }
        }

        private void ListenLoop()
        {
            try
            {
                string? line;
                while (_running && (line = _reader!.ReadLine()) != null)
                {
                    Envelope? envelope = null;
                    try
                    {
                        envelope = JsonSerializer.Deserialize<Envelope>(line, Envelope.Options);
                    }
                    catch (JsonException)
                    {
                        continue;
                    }

                    if (envelope != null)
                    {
                        // Never let a handler exception kill the connection
                        try { MessageReceived?.Invoke(envelope); }
                        catch (Exception ex) { Log?.Invoke($"Handler error: {ex.Message}"); }
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.Invoke($"Listen error: {ex.Message}");
            }
            finally
            {
                Disconnect();
                Disconnected?.Invoke();
            }
        }

        public void Disconnect()
        {
            if (!_running) return;
            _running = false;
            try { _reader?.Close(); } catch { }
            try { _writer?.Close(); } catch { }
            try { _stream?.Close(); } catch { }
            try { _tcpClient?.Close(); } catch { }
            Log?.Invoke("Disconnected from server.");
        }
    }
}