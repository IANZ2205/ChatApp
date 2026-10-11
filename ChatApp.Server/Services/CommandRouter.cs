using System;
using System.IO;
using System.Linq;
using ChatApp.Common;
using ChatApp.Common.Protocol;
using ChatApp.Server.Data;
using ChatApp.Server.Network;

namespace ChatApp.Server.Services
{
    public class CommandRouter
    {
        private readonly AuthService _auth = new AuthService();
        private readonly ChatService _chat = new ChatService();
        private readonly UserRepository _users = new UserRepository();
        private readonly FileService _files = new FileService();
        private readonly FriendService _friends = new FriendService();
        private readonly Action<string> _log;
        private readonly ChatServer _server;

        public CommandRouter(Action<string> log, ChatServer server)
        {
            _log = log;
            _server = server;
        }

        public void Handle(ClientSession session, Envelope envelope)
        {
            try
            {
                switch (envelope.Type)
                {
                    case MessageTypes.Register: HandleRegister(session, envelope); break;
                    case MessageTypes.Login: HandleLogin(session, envelope); break;
                    case MessageTypes.Logout: HandleLogout(session, envelope); break;
                    case MessageTypes.SendMessage: HandleSendMessage(session, envelope); break;
                    case MessageTypes.GetHistory: HandleGetHistory(session, envelope); break;
                    case MessageTypes.MarkRead: HandleMarkRead(session, envelope); break;
                    case MessageTypes.GetOnlineList: HandleGetOnlineList(session, envelope); break;
                    case MessageTypes.GetConversations: HandleGetConversations(session, envelope); break;
                    case MessageTypes.OpenPrivateChat: HandleOpenPrivateChat(session, envelope); break;
                    case MessageTypes.CreateGroup: HandleCreateGroup(session, envelope); break;
                    case MessageTypes.AddMemberToGroup: HandleAddMemberToGroup(session, envelope); break;
                    case MessageTypes.FileUploadStart: HandleFileUploadStart(session, envelope); break;
                    case MessageTypes.FileUploadChunk: HandleFileUploadChunk(session, envelope); break;
                    case MessageTypes.FileUploadEnd: HandleFileUploadEnd(session, envelope); break;
                    case MessageTypes.FileDownloadRequest: HandleFileDownload(session, envelope); break;
                    case MessageTypes.FriendSendRequest: HandleFriendSendRequest(session, envelope); break;
                    case MessageTypes.FriendResponse: HandleFriendResponse(session, envelope); break;
                    case MessageTypes.FriendGetList: HandleFriendGetList(session, envelope); break;
                    case MessageTypes.GetPendingRequests: HandleGetPendingRequests(session, envelope); break;
                    case MessageTypes.GetProfile: HandleGetProfile(session, envelope); break;
                    case MessageTypes.UpdateProfile: HandleUpdateProfile(session, envelope); break;
                    case MessageTypes.Ping:
                        session.Send(Envelope.Create(MessageTypes.Pong, new { }, envelope.RequestId));
                        break;
                    default:
                        SendError(session, envelope, Constants.ErrInvalid, $"Unknown type: {envelope.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _log($"Router error: {ex.Message}");
                SendError(session, envelope, Constants.ErrServerError, "Server error.");
            }
        }

        private void HandleRegister(ClientSession session, Envelope envelope)
        {
            var req = envelope.ReadData<RegisterRequest>() ?? new RegisterRequest();
            var result = _auth.Register(req);
            session.Send(Envelope.Create(MessageTypes.RegisterResult, result, envelope.RequestId));
            _log($"REGISTER {req.Username}: {(result.Success ? "OK" : result.ErrorCode)}");
        }

        private void HandleLogin(ClientSession session, Envelope envelope)
        {
            var req = envelope.ReadData<LoginRequest>() ?? new LoginRequest();
            var result = _auth.Login(req);
            if (result.Success)
            {
                session.UserId = result.UserId;
                session.Username = result.Username;
                _server.RegisterUser(result.UserId, session);
                _log($"USER ONLINE: {result.Username} (id={result.UserId})");
                SendPendingRequests(session);
                SendFriendList(session);
            }
            Stats.LoginsTotal++;
            session.Send(Envelope.Create(MessageTypes.LoginResult, result, envelope.RequestId));
            _log($"LOGIN {req.Username}: {(result.Success ? "OK" : result.ErrorCode)}");
        }
        private void HandleLogout(ClientSession session, Envelope envelope)
        {
            if (session.UserId.HasValue)
            {
                _log($"LOGOUT {session.Username}");
                _server.UnregisterUser(session.UserId.Value, session.Username ?? "");
                session.UserId = null;
                session.Username = null;
            }
            // Confirm to the client — do NOT close the socket here.
            // The client will disconnect itself, and the session loop will exit cleanly.
            session.Send(Envelope.Create(MessageTypes.LogoutResult, new { }, envelope.RequestId));
        }

        private void HandleSendMessage(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) { SendError(session, envelope, Constants.ErrAuthRequired, "Log in first."); return; }
            var req = envelope.ReadData<SendMessageRequest>();
            if (req == null) return;
            if (!_chat.IsMember(req.ConversationId, session.UserId.Value))
            { SendError(session, envelope, Constants.ErrNotFound, "Not a member."); return; }

            string text = (req.Text ?? "").Trim();
            if (text.Length == 0) return;
            if (text.Length > Constants.MaxMessageLength) { SendError(session, envelope, Constants.ErrInvalid, "Too long."); return; }

            var msg = _chat.SaveTextMessage(req.ConversationId, session.UserId.Value, session.Username ?? "", text);
            
            BroadcastMessage(req.ConversationId, msg);
        }

        private void HandleGetHistory(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) { SendError(session, envelope, Constants.ErrAuthRequired, "Log in first."); return; }
            var req = envelope.ReadData<GetHistoryRequest>() ?? new GetHistoryRequest();
            if (!_chat.IsMember(req.ConversationId, session.UserId.Value))
            { SendError(session, envelope, Constants.ErrNotFound, "Not a member."); return; }
            int before = req.BeforeMessageId <= 0 ? int.MaxValue : req.BeforeMessageId;
            var msgs = _chat.GetHistory(req.ConversationId, before, req.Limit);
            session.Send(Envelope.Create(MessageTypes.HistoryResult,
                new HistoryResult { ConversationId = req.ConversationId, Messages = msgs }, envelope.RequestId));
        }

        private void HandleMarkRead(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) return;
            var req = envelope.ReadData<MarkReadRequest>();
            if (req == null) return;
            if (!_chat.IsMember(req.ConversationId, session.UserId.Value)) return;
            int lastId = req.LastMessageId;
            if (lastId <= 0) lastId = _chat.GetLatestMessageId(req.ConversationId);
            _chat.MarkRead(req.ConversationId, session.UserId.Value, lastId);
        }

        private void HandleGetOnlineList(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) return;
            session.Send(Envelope.Create(MessageTypes.OnlineList,
                new OnlineListResult { Users = _server.GetOnlineUsers() }, envelope.RequestId));
        }

        private void HandleGetConversations(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) return;
            var list = _chat.GetConversationsForUser(session.UserId.Value);
            session.Send(Envelope.Create(MessageTypes.ConversationList,
                new ConversationListResult { Conversations = list }, envelope.RequestId));
        }

        private void HandleOpenPrivateChat(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) { SendError(session, envelope, Constants.ErrAuthRequired, "Log in first."); return; }
            var req = envelope.ReadData<OpenPrivateChatRequest>();
            if (req == null) return;
            if (req.TargetUserId == session.UserId.Value) { SendError(session, envelope, Constants.ErrInvalid, "Cannot chat with yourself."); return; }
            var target = _users.GetById(req.TargetUserId);
            if (target == null) { SendError(session, envelope, Constants.ErrNotFound, "User not found."); return; }

            int convId = _chat.OpenPrivateChat(session.UserId.Value, req.TargetUserId);
            var infoMe = _chat.GetConversationInfo(convId, session.UserId.Value);
            if (infoMe != null) session.Send(Envelope.Create(MessageTypes.OpenPrivateChatResult, infoMe, envelope.RequestId));
            var infoThem = _chat.GetConversationInfo(convId, req.TargetUserId);
            if (infoThem != null) _server.SendToUser(req.TargetUserId, Envelope.Create(MessageTypes.ConversationAdded, infoThem));
        }

        private void HandleCreateGroup(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) { SendError(session, envelope, Constants.ErrAuthRequired, "Log in first."); return; }
            var req = envelope.ReadData<CreateGroupRequest>();
            if (req == null) return;

            string name = (req.Name ?? "").Trim();
            if (name.Length == 0) { SendError(session, envelope, Constants.ErrInvalid, "Group name required."); return; }
            if (name.Length > 50) name = name.Substring(0, 50);

            var memberIds = (req.MemberUserIds ?? new System.Collections.Generic.List<int>())
                .Where(id => id != session.UserId.Value).Distinct().ToList();

            int convId = _chat.CreateGroup(name, session.UserId.Value, memberIds);
            _log($"GROUP created conv={convId} name='{name}'");
            _chat.SaveSystemMessage(convId, $"{session.Username} created the group '{name}'");

            var infoMe = _chat.GetConversationInfo(convId, session.UserId.Value);
            if (infoMe != null)
                session.Send(Envelope.Create(MessageTypes.ConversationAdded, infoMe, envelope.RequestId));

            foreach (int uid in memberIds)
            {
                var infoThem = _chat.GetConversationInfo(convId, uid);
                if (infoThem != null)
                    _server.SendToUser(uid, Envelope.Create(MessageTypes.ConversationAdded, infoThem));
            }
        }

        private void HandleAddMemberToGroup(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) { SendError(session, envelope, Constants.ErrAuthRequired, "Log in first."); return; }
            var req = envelope.ReadData<AddMemberToGroupRequest>();
            if (req == null) return;
            if (!_chat.IsMember(req.ConversationId, session.UserId.Value))
            { SendError(session, envelope, Constants.ErrNotFound, "Not a member."); return; }
            string? groupName = _chat.GetGroupName(req.ConversationId);
            if (string.IsNullOrEmpty(groupName)) { SendError(session, envelope, Constants.ErrInvalid, "Not a group."); return; }
            if (_chat.IsMember(req.ConversationId, req.TargetUserId))
            { SendError(session, envelope, Constants.ErrInvalid, "Already a member."); return; }

            var target = _users.GetById(req.TargetUserId);
            if (target == null) { SendError(session, envelope, Constants.ErrNotFound, "User not found."); return; }
            if (!_chat.AddMember(req.ConversationId, req.TargetUserId))
            { SendError(session, envelope, Constants.ErrServerError, "Could not add member."); return; }

            _chat.SaveSystemMessage(req.ConversationId, $"{target.Username} joined the group");
            _log($"GROUP ADD: {target.Username} to conv={req.ConversationId}");

            foreach (int uid in _chat.GetMembers(req.ConversationId))
            {
                var info = _chat.GetConversationInfo(req.ConversationId, uid);
                if (info != null)
                    _server.SendToUser(uid, Envelope.Create(MessageTypes.ConversationUpdated, info));
            }
        }

        private void HandleFileUploadStart(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) { SendError(session, envelope, Constants.ErrAuthRequired, "Log in first."); return; }
            var req = envelope.ReadData<FileUploadStartRequest>();
            if (req == null) return;
            if (!_chat.IsMember(req.ConversationId, session.UserId.Value))
            { SendError(session, envelope, Constants.ErrNotFound, "Not a member."); return; }
            var (ok, uploadId, error) = _files.Start(session.UserId.Value, req.ConversationId, req.FileName, req.FileSize);
            if (ok) _log($"FILE UPLOAD START {req.FileName}");
            session.Send(Envelope.Create(MessageTypes.FileUploadResult,
                new FileUploadStartResult { Success = ok, UploadId = uploadId, Message = error }, envelope.RequestId));
        }

        private void HandleFileUploadChunk(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) return;
            var req = envelope.ReadData<FileUploadChunkRequest>();
            if (req == null) return;
            try
            {
                byte[] data = Convert.FromBase64String(req.DataB64);
                _files.AppendChunk(req.UploadId, req.Seq, data);
            }
            catch (Exception ex) { _log($"Chunk error: {ex.Message}"); }
        }

        private void HandleFileUploadEnd(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) return;
            var req = envelope.ReadData<FileUploadEndRequest>();
            if (req == null) return;
            int fileId = _files.Finish(req.UploadId, session.UserId.Value, out int convId);
            if (fileId == 0) { _log("File upload failed"); return; }
            var info = _files.GetFile(fileId);
            if (info == null) return;
            var msg = _chat.SaveFileMessage(convId, session.UserId.Value, session.Username ?? "",
                                             fileId, info.Value.name, info.Value.size);
            Stats.FilesTotal++;
            _log($"FILE saved id={fileId} msg={msg.MessageId}");
            BroadcastMessage(convId, msg);
        }

        private void HandleFileDownload(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) return;
            var req = envelope.ReadData<FileDownloadRequest>();
            if (req == null) return;
            var info = _files.GetFile(req.FileId);
            if (info == null) { SendError(session, envelope, Constants.ErrNotFound, "File not found."); return; }
            try
            {
                byte[] all = System.IO.File.ReadAllBytes(info.Value.path);
                int total = (int)Math.Ceiling((double)all.Length / Constants.FileChunkSize);
                if (total == 0) total = 1;
                for (int i = 0; i < total; i++)
                {
                    int offset = i * Constants.FileChunkSize;
                    int len = Math.Min(Constants.FileChunkSize, all.Length - offset);
                    session.Send(Envelope.Create(MessageTypes.FileDownloadChunk, new FileDownloadChunk
                    {
                        FileId = req.FileId,
                        FileName = info.Value.name,
                        Seq = i,
                        Total = total,
                        DataB64 = Convert.ToBase64String(all, offset, len)
                    }));
                }
                session.Send(Envelope.Create(MessageTypes.FileDownloadEnd, new FileDownloadEnd { FileId = req.FileId }));
            }
            catch (Exception ex)
            {
                _log($"Download error: {ex.Message}");
                SendError(session, envelope, Constants.ErrServerError, "Could not read file.");
            }
        }

        private void HandleFriendSendRequest(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) { SendError(session, envelope, Constants.ErrAuthRequired, "Log in first."); return; }
            var req = envelope.ReadData<FriendSendRequestReq>();
            if (req == null) return;

            var (result, friendshipId, targetId) = _friends.Send(session.UserId.Value, req.TargetUsername ?? "");
            switch (result)
            {
                case FriendRepository.AddResult.NotFound:
                    session.Send(Envelope.Create(MessageTypes.FriendRequestResult,
                        new FriendRequestResult { Success = false, Message = "User not found." }, envelope.RequestId));
                    return;
                case FriendRepository.AddResult.AlreadyFriends:
                    session.Send(Envelope.Create(MessageTypes.FriendRequestResult,
                        new FriendRequestResult { Success = false, Message = "Already friends." }, envelope.RequestId));
                    return;
                case FriendRepository.AddResult.AlreadyPending:
                    session.Send(Envelope.Create(MessageTypes.FriendRequestResult,
                        new FriendRequestResult { Success = false, Message = "Request already pending." }, envelope.RequestId));
                    return;
            }

            session.Send(Envelope.Create(MessageTypes.FriendRequestResult,
                new FriendRequestResult { Success = true, PeerUsername = req.TargetUsername }, envelope.RequestId));

            _server.SendToUser(targetId, Envelope.Create(MessageTypes.FriendRequestIncoming, new FriendRequestIncoming
            {
                FriendshipId = friendshipId,
                RequesterId = session.UserId.Value,
                RequesterUsername = session.Username ?? ""
            }));

            var targetSession = _server.GetSessionForUser(targetId);
            if (targetSession != null) SendPendingRequests(targetSession);
        }

        private void HandleFriendResponse(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) { SendError(session, envelope, Constants.ErrAuthRequired, "Log in first."); return; }
            var req = envelope.ReadData<FriendResponseReq>();
            if (req == null) return;

            var (ok, otherId) = _friends.Respond(req.FriendshipId, session.UserId.Value, req.Accept);
            if (!ok) { SendError(session, envelope, Constants.ErrNotFound, "Request not found."); return; }

            NotifyFriendListChanged(session.UserId.Value);
            NotifyFriendListChanged(otherId);
            SendPendingRequests(session);
        }

        private void HandleFriendGetList(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) return;
            SendFriendList(session);
        }

        private void HandleGetPendingRequests(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) return;
            SendPendingRequests(session, envelope.RequestId);
        }

        // ---------- PROFILE ----------
        private void HandleGetProfile(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) { SendError(session, envelope, Constants.ErrAuthRequired, "Log in first."); return; }
            var req = envelope.ReadData<GetProfileRequest>();
            if (req == null) return;

            int targetId = req.UserId > 0 ? req.UserId : session.UserId.Value;
            var user = _users.GetProfile(targetId);
            if (user == null)
            {
                _log($"PROFILE read failed: user {targetId} not found");
                session.Send(Envelope.Create(MessageTypes.ProfileData,
                    new ProfileData { Success = false, FullName = "User not found." }, envelope.RequestId));
                return;
            }

            var (bio, photoPath) = _users.GetBioAndPhoto(targetId);
            string? photoB64 = null;
            if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
            {
                try { photoB64 = Convert.ToBase64String(File.ReadAllBytes(photoPath)); }
                catch (Exception ex) { _log($"PROFILE photo read error: {ex.Message}"); }
            }

            _log($"PROFILE read for {user.Username}: name='{user.FullName}' bio-len={bio.Length} photo={(photoB64 != null ? "yes" : "no")}");

            session.Send(Envelope.Create(MessageTypes.ProfileData, new ProfileData
            {
                Success = true,
                UserId = targetId,
                Username = user.Username,
                FullName = user.FullName,
                Bio = bio,
                PhotoBase64 = photoB64
            }, envelope.RequestId));
        }

        private void HandleUpdateProfile(ClientSession session, Envelope envelope)
        {
            if (!session.UserId.HasValue) { SendError(session, envelope, Constants.ErrAuthRequired, "Log in first."); return; }
            var req = envelope.ReadData<UpdateProfileRequest>();
            if (req == null) return;

            string fullName = (req.FullName ?? "").Trim();
            string bio = (req.Bio ?? "").Trim();
            if (fullName.Length > 60) fullName = fullName.Substring(0, 60);
            if (bio.Length > 300) bio = bio.Substring(0, 300);

            _log($"PROFILE update received: name='{fullName}' bio-len={bio.Length} photo={(!string.IsNullOrEmpty(req.PhotoBase64) ? "yes" : "no")}");

            string? photoPath = null;
            if (!string.IsNullOrEmpty(req.PhotoBase64))
            {
                try
                {
                    byte[] bytes = Convert.FromBase64String(req.PhotoBase64);
                    if (bytes.Length > 2 * 1024 * 1024)
                    { SendError(session, envelope, Constants.ErrInvalid, "Photo too large."); return; }
                    string photosDir = Path.Combine(Database.DataFolder, "Photos");
                    Directory.CreateDirectory(photosDir);
                    string fileName = $"u{session.UserId.Value}_{Guid.NewGuid():N}.png";
                    photoPath = Path.Combine(photosDir, fileName);
                    File.WriteAllBytes(photoPath, bytes);
                    _log($"PROFILE photo saved to {fileName} ({bytes.Length} bytes)");
                }
                catch (Exception ex)
                {
                    _log($"Photo save error: {ex.Message}");
                    SendError(session, envelope, Constants.ErrServerError, "Could not save photo.");
                    return;
                }
            }

            _users.UpdateProfile(session.UserId.Value, fullName, bio, photoPath);
            _log($"PROFILE stored for user id={session.UserId.Value}");

            // Read back and send the fresh data
            HandleGetProfile(session, Envelope.Create(MessageTypes.GetProfile,
                new GetProfileRequest { UserId = session.UserId.Value }, envelope.RequestId));
        }

        private void SendFriendList(ClientSession session)
        {
            if (!session.UserId.HasValue) return;
            var friends = _friends.GetFriends(session.UserId.Value);
            foreach (var f in friends) f.IsOnline = _server.IsUserOnline(f.UserId);
            session.Send(Envelope.Create(MessageTypes.FriendListResult,
                new FriendListResult { Friends = friends }));
        }

        private void SendPendingRequests(ClientSession session, string? requestId = null)
        {
            if (!session.UserId.HasValue) return;
            var list = _friends.GetPendingIncoming(session.UserId.Value);
            session.Send(Envelope.Create(MessageTypes.PendingRequestsResult,
                new PendingRequestsResult { Requests = list }, requestId));
        }

        private void NotifyFriendListChanged(int userId)
        {
            var session = _server.GetSessionForUser(userId);
            if (session != null) SendFriendList(session);
        }

        private void BroadcastMessage(int conversationId, ChatApp.Common.Models.ChatMessage msg)
        {
            var outgoing = Envelope.Create(MessageTypes.MessageNew, msg);
            foreach (int uid in _chat.GetMembers(conversationId)) _server.SendToUser(uid, outgoing);
        }

        private void SendError(ClientSession session, Envelope envelope, string code, string message)
        {
            session.Send(Envelope.Create(MessageTypes.Error,
                new ErrorPayload { Code = code, Message = message }, envelope.RequestId));
        }
    }
}