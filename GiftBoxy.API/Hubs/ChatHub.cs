using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace GiftBoxy.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        // User qoşulduqda öz personal group-una qoşulur
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            await base.OnConnectedAsync();
        }

        // Söhbətə qoşul
        public async Task JoinConversation(int conversationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            // Bu user bu söhbətə aiddir?
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    (c.BuyerId == userId || c.SellerId == userId));

            if (conversation == null)
                return;

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"conversation_{conversationId}");
        }

        // Söhbətdən çıx
        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"conversation_{conversationId}");
        }

        // Mesaj göndər
        public async Task SendMessage(int conversationId, string text)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(text))
                return;

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    (c.BuyerId == userId || c.SellerId == userId));

            if (conversation == null)
                return;

            var message = new Message
            {
                Text = text,
                ConversationId = conversationId,
                SenderId = userId!,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var sender = await _context.Users.FindAsync(userId);

            var payload = new
            {
                id = message.Id,
                text = message.Text,
                sentAt = message.SentAt,
                senderId = userId,
                senderName = sender?.Name ?? "",
                conversationId,
                isOwn = false // alıcı tərəfindən false görünəcək
            };

            // Söhbətdəki hər ikisinə göndər
            await Clients
                .Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessage", payload);

            // Əgər qarşı tərəf söhbətdə deyilsə notification göndər
            var receiverId = conversation.BuyerId == userId
                ? conversation.SellerId
                : conversation.BuyerId;

            await Clients
                .Group($"user_{receiverId}")
                .SendAsync("NewMessageNotification", new
                {
                    conversationId,
                    senderName = sender?.Name ?? "",
                    preview = text.Length > 50 ? text[..50] + "..." : text
                });
        }

        // Mesajları oxunmuş et
        public async Task MarkAsRead(int conversationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var unreadMessages = await _context.Messages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderId != userId &&
                    !m.IsRead)
                .ToListAsync();

            if (!unreadMessages.Any())
                return;

            foreach (var message in unreadMessages)
                message.IsRead = true;

            await _context.SaveChangesAsync();

            // Göndərənə "oxundu" siqnalı ver
            var senderId = unreadMessages.First().SenderId;

            await Clients
                .Group($"user_{senderId}")
                .SendAsync("MessagesRead", new { conversationId });
        }

        // Yazır... göstəricisi
        public async Task Typing(int conversationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);

            await Clients
                .OthersInGroup($"conversation_{conversationId}")
                .SendAsync("UserTyping", new
                {
                    conversationId,
                    userName = user?.Name ?? ""
                });
        }
    }
}
