using System;
using System.Collections.Generic;
using System.Text;

namespace GiftBoxy.Application.DTOs.QA
{
    public class QuestionResponseDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string? AnswerText { get; set; }
        public DateTime AskedAt { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public string BuyerName { get; set; }
        public bool IsAnswered => AnswerText != null;
    }
}
