using System;

namespace ChatApp.Common.Models
{
    public class ChatMessage
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = "";
        public string Kind { get; set; } = "Text"; // "Text", "File", or "System"
        public string Content { get; set; } = "";
        public int? FileId { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public DateTime SentAt { get; set; }
    }
}