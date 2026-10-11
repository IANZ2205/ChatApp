using System.Collections.Generic;
using ChatApp.Common.Models;

namespace ChatApp.Common.Protocol
{
    public class RegisterRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FullName { get; set; } = "";
    }

    public class LoginRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? Message { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
    }

    public class ErrorPayload
    {
        public string Code { get; set; } = "";
        public string Message { get; set; } = "";
    }

    public class SendMessageRequest
    {
        public int ConversationId { get; set; }
        public string Text { get; set; } = "";
    }

    public class GetHistoryRequest
    {
        public int ConversationId { get; set; }
        public int BeforeMessageId { get; set; }
        public int Limit { get; set; } = 50;
    }

    public class MarkReadRequest
    {
        public int ConversationId { get; set; }
        public int LastMessageId { get; set; }
    }

    public class HistoryResult
    {
        public int ConversationId { get; set; }
        public List<ChatMessage> Messages { get; set; } = new();
    }

    public class UserInfoPayload
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string? Address { get; set; }
    }

    public class OnlineListResult
    {
        public List<UserInfoPayload> Users { get; set; } = new();
    }

    public class PresenceUpdate
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public bool IsOnline { get; set; }
    }

    public class SystemNotice
    {
        public string Text { get; set; } = "";
    }

    public class KickNotice
    {
        public string Reason { get; set; } = "";
    }

    public class GetConversationsRequest { }

    public class ConversationListResult
    {
        public List<ConversationInfo> Conversations { get; set; } = new();
    }

    public class OpenPrivateChatRequest
    {
        public int TargetUserId { get; set; }
    }

    public class FileUploadStartRequest
    {
        public int ConversationId { get; set; }
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
    }

    public class FileUploadStartResult
    {
        public bool Success { get; set; }
        public string UploadId { get; set; } = "";
        public string? Message { get; set; }
    }

    public class FileUploadChunkRequest
    {
        public string UploadId { get; set; } = "";
        public int Seq { get; set; }
        public string DataB64 { get; set; } = "";
    }

    public class FileUploadEndRequest
    {
        public string UploadId { get; set; } = "";
    }

    public class FileDownloadRequest
    {
        public int FileId { get; set; }
    }

    public class FileDownloadChunk
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = "";
        public int Seq { get; set; }
        public int Total { get; set; }
        public string DataB64 { get; set; } = "";
    }

    public class FileDownloadEnd
    {
        public int FileId { get; set; }
    }

    public class FriendSendRequestReq
    {
        public string TargetUsername { get; set; } = "";
    }

    public class FriendResponseReq
    {
        public int FriendshipId { get; set; }
        public bool Accept { get; set; }
    }

    public class FriendListRequest { }

    public class FriendInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public bool IsOnline { get; set; }
    }

    public class FriendListResult
    {
        public List<FriendInfo> Friends { get; set; } = new();
    }

    public class FriendRequestIncoming
    {
        public int FriendshipId { get; set; }
        public int RequesterId { get; set; }
        public string RequesterUsername { get; set; } = "";
    }

    public class FriendRequestResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string PeerUsername { get; set; } = "";
    }

    public class CreateGroupRequest
    {
        public string Name { get; set; } = "";
        public List<int> MemberUserIds { get; set; } = new();
    }

    public class AddMemberToGroupRequest
    {
        public int ConversationId { get; set; }
        public int TargetUserId { get; set; }
    }

    public class PendingRequestInfo
    {
        public int FriendshipId { get; set; }
        public int RequesterId { get; set; }
        public string RequesterUsername { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }

    public class PendingRequestsResult
    {
        public List<PendingRequestInfo> Requests { get; set; } = new();
    }

    public class GetProfileRequest
    {
        public int UserId { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = "";
        public string Bio { get; set; } = "";
        public string? PhotoBase64 { get; set; }
    }

    public class ProfileData
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Bio { get; set; } = "";
        public string? PhotoBase64 { get; set; }
        public string? LastSeen { get; set; }
        public bool Success { get; set; } = true;
    }
}