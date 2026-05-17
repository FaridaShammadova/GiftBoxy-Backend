using System;
using System.Collections.Generic;
using System.Text;

namespace GiftBoxy.Application.DTOs.Chat
{
    public class SendMessageDto
    {
        public string Text { get; set; }
        public int ConversationId { get; set; }
    }
}
