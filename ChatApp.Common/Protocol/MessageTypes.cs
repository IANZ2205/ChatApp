namespace ChatApp.Common.Protocol
{
    public static class MessageTypes
    {
        public const string Register = "REGISTER";
        public const string Login = "LOGIN";
        public const string Logout = "LOGOUT";
        public const string SendMessage = "SEND_MESSAGE";
        public const string GetHistory = "GET_HISTORY";
        public const string MarkRead = "MARK_READ";
        public const string GetOnlineList = "GET_ONLINE_LIST";
        public const string GetConversations = "GET_CONVERSATIONS";
        public const string OpenPrivateChat = "OPEN_PRIVATE_CHAT";
        public const string CreateGroup = "CREATE_GROUP";
        public const string AddMemberToGroup = "ADD_MEMBER_TO_GROUP";
        public const string FileUploadStart = "FILE_UPLOAD_START";
        public const string FileUploadChunk = "FILE_UPLOAD_CHUNK";
        public const string FileUploadEnd = "FILE_UPLOAD_END";
        public const string FileDownloadRequest = "FILE_DOWNLOAD_REQUEST";
        public const string FriendSendRequest = "FRIEND_SEND_REQUEST";
        public const string FriendResponse = "FRIEND_RESPONSE";
        public const string FriendGetList = "FRIEND_GET_LIST";
        public const string GetPendingRequests = "GET_PENDING_REQUESTS";
        public const string GetProfile = "GET_PROFILE";
        public const string UpdateProfile = "UPDATE_PROFILE";
        public const string Ping = "PING";

        public const string RegisterResult = "REGISTER_RESULT";
        public const string LoginResult = "LOGIN_RESULT";
        public const string LogoutResult = "LOGOUT_RESULT";
        public const string SyncState = "SYNC_STATE";
        public const string OnlineList = "ONLINE_LIST";
        public const string MessageNew = "MESSAGE_NEW";
        public const string HistoryResult = "HISTORY_RESULT";
        public const string PresenceUpdate = "PRESENCE_UPDATE";
        public const string SystemNotice = "SYSTEM_NOTICE";
        public const string ConversationList = "CONVERSATION_LIST";
        public const string OpenPrivateChatResult = "OPEN_PRIVATE_CHAT_RESULT";
        public const string ConversationAdded = "CONVERSATION_ADDED";
        public const string ConversationUpdated = "CONVERSATION_UPDATED";
        public const string FileUploadResult = "FILE_UPLOAD_RESULT";
        public const string FileDownloadChunk = "FILE_DOWNLOAD_CHUNK";
        public const string FileDownloadEnd = "FILE_DOWNLOAD_END";
        public const string FriendListResult = "FRIEND_LIST_RESULT";
        public const string FriendRequestIncoming = "FRIEND_REQUEST_INCOMING";
        public const string FriendRequestResult = "FRIEND_REQUEST_RESULT";
        public const string PendingRequestsResult = "PENDING_REQUESTS_RESULT";
        public const string ProfileData = "PROFILE_DATA";
        public const string Kicked = "KICKED";
        public const string Error = "ERROR";
        public const string Pong = "PONG";
    }
}