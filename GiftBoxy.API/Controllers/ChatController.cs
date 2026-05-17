using GiftBoxy.Application.DTOs.Chat;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftBoxy.API.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        // Bütün söhbətləri gör
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var conversations = await _context.Conversations
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages)
                .Where(c => c.BuyerId == userId || c.SellerId == userId)
                .OrderByDescending(c => c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.SentAt)
                    .FirstOrDefault())
                .ToListAsync();

            var result = conversations.Select(c =>
            {
                var isBuyer = c.BuyerId == userId;
                var otherPerson = isBuyer ? c.Seller : c.Buyer;
                var lastMessage = c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefault();

                return new ConversationResponseDto
                {
                    Id = c.Id,
                    OtherPersonName = otherPerson?.UserName ?? "",
                    OtherPersonAvatar = otherPerson?.Avatar,
                    LastMessage = lastMessage?.Text,
                    LastMessageAt = lastMessage?.SentAt,
                    UnreadCount = c.Messages
                        .Count(m => !m.IsRead && m.SenderId != userId)
                };
            }).ToList();

            return Ok(result);
        }

        // Söhbətin mesajlarını gör
        [HttpGet("conversations/{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var conversation = await _context.Conversations
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    (c.BuyerId == userId || c.SellerId == userId));

            if (conversation == null)
                return NotFound();

            // Oxunmamış mesajları oxunmuş et
            var unreadMessages = conversation.Messages
                .Where(m => !m.IsRead && m.SenderId != userId)
                .ToList();

            foreach (var message in unreadMessages)
                message.IsRead = true;

            await _context.SaveChangesAsync();

            var messages = conversation.Messages
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageResponseDto
                {
                    Id = m.Id,
                    Text = m.Text,
                    IsRead = m.IsRead,
                    SentAt = m.SentAt,
                    SenderName = m.Sender?.UserName ?? "",
                    IsOwn = m.SenderId == userId
                }).ToList();

            return Ok(messages);
        }

        // Yeni söhbət başlat (yalnız Buyer)
        [Authorize(Roles = "Buyer")]
        [HttpPost("conversations")]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Seller mövcuddurmu?
            var seller = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.SellerId);

            if (seller == null)
                return NotFound("Seller not found");

            // Artıq söhbət varmı?
            var existing = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.BuyerId == userId &&
                    c.SellerId == dto.SellerId);

            if (existing != null)
                return Ok(new { conversationId = existing.Id });

            var conversation = new Conversation
            {
                BuyerId = userId,
                SellerId = dto.SellerId
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            return Ok(new { conversationId = conversation.Id });
        }

        // Mesaj göndər
        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id == dto.ConversationId &&
                    (c.BuyerId == userId || c.SellerId == userId));

            if (conversation == null)
                return NotFound("Conversation not found");

            if (string.IsNullOrWhiteSpace(dto.Text))
                return BadRequest("Message cannot be empty");

            var message = new Message
            {
                Text = dto.Text,
                ConversationId = dto.ConversationId,
                SenderId = userId,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = message.Id,
                text = message.Text,
                sentAt = message.SentAt
            });
        }

        // Oxunmamış mesaj sayı
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var count = await _context.Messages
                .Include(m => m.Conversation)
                .CountAsync(m =>
                    !m.IsRead &&
                    m.SenderId != userId &&
                    (m.Conversation.BuyerId == userId ||
                     m.Conversation.SellerId == userId));

            return Ok(new { unreadCount = count });
        }
    }
}
