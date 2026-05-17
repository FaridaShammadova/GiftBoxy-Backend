using System;
using System.Collections.Generic;
using System.Text;

namespace GiftBoxy.Application.DTOs.Chat
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
        public string SenderName { get; set; }
        public bool IsOwn { get; set; }
    }
}
