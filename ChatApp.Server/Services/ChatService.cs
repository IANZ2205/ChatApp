using System;
using System.Collections.Generic;
using ChatApp.Common.Models;
using ChatApp.Server.Data;

namespace ChatApp.Server.Services
{
    public class ChatService
    {
        private readonly MessageRepository _messages = new MessageRepository();
        private readonly UserRepository _users = new UserRepository();
        private readonly ConversationRepository _conversations = new ConversationRepository();

        public ChatMessage SaveTextMessage(int conversationId, int senderId, string senderName, string text)
        {
            int id = _messages.Save(conversationId, senderId, "Text", text);
            return new ChatMessage
            {
                MessageId = id,
                ConversationId = conversationId,
                SenderId = senderId,
                SenderName = senderName,
                Kind = "Text",
                Content = text,
                SentAt = DateTime.UtcNow
            };
        }

        public ChatMessage SaveFileMessage(int conversationId, int senderId, string senderName,
                                            int fileId, string fileName, long fileSize)
        {
            int id = _messages.SaveWithFile(conversationId, senderId, fileId, fileName);
            return new ChatMessage
            {
                MessageId = id,
                ConversationId = conversationId,
                SenderId = senderId,
                SenderName = senderName,
                Kind = "File",
                Content = fileName,
                FileId = fileId,
                FileName = fileName,
                FileSize = fileSize,
                SentAt = DateTime.UtcNow
            };
        }

        public ChatMessage SaveSystemMessage(int conversationId, string text)
        {
            int id = _messages.Save(conversationId, null, "System", text);
            return new ChatMessage
            {
                MessageId = id,
                ConversationId = conversationId,
                SenderId = 0,
                SenderName = "(system)",
                Kind = "System",
                Content = text,
                SentAt = DateTime.UtcNow
            };
        }

        public List<ChatMessage> GetHistory(int conversationId, int beforeId, int limit)
            => _messages.GetHistory(conversationId, beforeId, limit);

        public int GetGeneralConversationId() => _users.GetGeneralConversationId();

        public List<int> GetMembers(int conversationId)
        {
            var list = new List<int>();
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT UserId FROM ConversationMembers WHERE ConversationId = $c;";
            cmd.Parameters.AddWithValue("$c", conversationId);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(r.GetInt32(0));
            return list;
        }

        public bool IsMember(int conversationId, int userId)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM ConversationMembers WHERE ConversationId = $c AND UserId = $u;";
            cmd.Parameters.AddWithValue("$c", conversationId);
            cmd.Parameters.AddWithValue("$u", userId);
            long n = (long)(cmd.ExecuteScalar() ?? 0L);
            return n > 0;
        }

        public int OpenPrivateChat(int userId1, int userId2)
            => _conversations.GetOrCreatePrivate(userId1, userId2);

        public List<ConversationInfo> GetConversationsForUser(int userId)
            => _conversations.GetConversationsForUser(userId);

        public ConversationInfo? GetConversationInfo(int convId, int forUserId)
            => _conversations.GetConversationInfo(convId, forUserId);

        public int CreateGroup(string name, int creatorId, List<int> memberIds)
            => _conversations.CreateGroup(name, creatorId, memberIds);
        public bool AddMember(int convId, int userId)
    => _conversations.AddMember(convId, userId);

        public string? GetGroupName(int convId)
            => _conversations.GetGroupName(convId);
        public void MarkRead(int conversationId, int userId, int lastMessageId)
    => _conversations.MarkRead(conversationId, userId, lastMessageId);

        public int GetLatestMessageId(int conversationId)
            => _conversations.GetLatestMessageId(conversationId);
    }
}