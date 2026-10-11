using System;

namespace ChatApp.Common.Models
{
    public class UserInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Bio { get; set; } = "";
        public string? PhotoPath { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}