using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ChatApp.Common.Protocol;
using ChatApp.Server.Services;

namespace ChatApp.Server.Network
{
    public class ChatServer
    {
        private readonly Action<string> _log;
        private TcpListener? _listener;
        private Thread? _acceptThread;
        private volatile bool _running;
        private CommandRouter? _router;

        private readonly ConcurrentDictionary<string, ClientSession> _sessions = new();
        private readonly ConcurrentDictionary<int, ClientSession> _sessionsByUserId = new();

        public event Action? StateChanged;
        public bool IsRunning => _running;

        public ChatServer(Action<string> log) { _log = log; }

        public void Start(int port)
        {
            if (_running) return;
            _router ??= new CommandRouter(_log, this);

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _running = true;

            _acceptThread = new Thread(AcceptLoop) { IsBackground = true };
            _acceptThread.Start();

            _log($"Server started on port {port}");
            StateChanged?.Invoke();
        }

        public void Stop()
        {
            if (!_running) return;
            _running = false;
            try { _listener?.Stop(); } catch { }
            _sessions.Clear();
            _sessionsByUserId.Clear();
            _log("Server stopped.");
            StateChanged?.Invoke();
        }

        public void RegisterUser(int userId, ClientSession session)
        {
            _sessionsByUserId[userId] = session;
            var notice = Envelope.Create(MessageTypes.PresenceUpdate,
                new PresenceUpdate { UserId = userId, Username = session.Username ?? "", IsOnline = true });
            Broadcast(notice);
        }

        public void UnregisterUser(int userId, string username)
        {
            if (_sessionsByUserId.TryRemove(userId, out _))
            {
                // Record last-seen time
                try { new Data.UserRepository().UpdateLastSeen(userId); }
                catch { }

                var notice = Envelope.Create(MessageTypes.PresenceUpdate,
                    new PresenceUpdate { UserId = userId, Username = username, IsOnline = false });
                Broadcast(notice);
            }
        }

        public void Broadcast(Envelope envelope)
        {
            foreach (var s in _sessionsByUserId.Values) s.Send(envelope);
        }

        public void SendToUser(int userId, Envelope envelope)
        {
            if (_sessionsByUserId.TryGetValue(userId, out var s)) s.Send(envelope);
        }

        public List<UserInfoPayload> GetOnlineUsers()
        {
            var list = new List<UserInfoPayload>();
            foreach (var kv in _sessionsByUserId)
            {
                list.Add(new UserInfoPayload
                {
                    UserId = kv.Key,
                    Username = kv.Value.Username ?? "",
                    Address = kv.Value.RemoteAddress
                });
            }
            return list;
        }

        // NEW: Kick a user by username
        public bool KickUser(string username, string reason)
        {
            foreach (var kv in _sessionsByUserId)
            {
                if (string.Equals(kv.Value.Username, username, StringComparison.OrdinalIgnoreCase))
                {
                    kv.Value.Send(Envelope.Create(MessageTypes.Kicked,
                        new KickNotice { Reason = reason }));

                    // Give the socket a moment to flush, then close it
                    var session = kv.Value;
                    new Thread(() =>
                    {
                        try { Thread.Sleep(200); } catch { }
                        try { session.ForceClose(); } catch { }
                    })
                    { IsBackground = true }.Start();

                    _log($"KICKED user: {username}");
                    return true;
                }
            }
            return false;
        }

        private void AcceptLoop()
        {
            while (_running)
            {
                try
                {
                    TcpClient tcp = _listener!.AcceptTcpClient();
                    string remote = tcp.Client.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();

                    var session = new ClientSession(tcp, _router!, _log);
                    session.Disconnected += s =>
                    {
                        if (s.UserId.HasValue)
                            UnregisterUser(s.UserId.Value, s.Username ?? "");
                        _sessions.TryRemove(remote, out _);
                    };

                    _sessions.TryAdd(remote, session);
                    new Thread(session.Run) { IsBackground = true }.Start();
                }
                catch (SocketException) { }
                catch (Exception ex) { _log($"Accept error: {ex.Message}"); }
            }
        }
        public bool IsUserOnline(int userId) => _sessionsByUserId.ContainsKey(userId);

        public ClientSession? GetSessionForUser(int userId)
            => _sessionsByUserId.TryGetValue(userId, out var s) ? s : null;
    }
}