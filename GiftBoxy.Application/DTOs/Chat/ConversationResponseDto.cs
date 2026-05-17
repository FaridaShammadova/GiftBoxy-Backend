using System;
using System.Collections.Generic;
using System.Text;

namespace GiftBoxy.Application.DTOs.Chat
{
    public class ConversationResponseDto
    {
        public int Id { get; set; }
        public string OtherPersonName { get; set; }
        public string? OtherPersonAvatar { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
