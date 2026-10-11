namespace ChatApp.Common.Models
{
    public class ConversationInfo
    {
        public int ConversationId { get; set; }
        public string Type { get; set; } = "Group"; // "Group" or "Private"
        public string Name { get; set; } = "";
        public string? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        public int MemberCount { get; set; }
    }
}