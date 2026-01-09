using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SGP_Freelancing.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                _logger.LogInformation($"User {userId} connected to chat");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                _logger.LogInformation($"User {userId} disconnected from chat");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var senderName = Context.User?.Identity?.Name ?? "Unknown";

            if (string.IsNullOrEmpty(senderId))
                return;

            // Send to receiver
            await Clients.Group(receiverId).SendAsync("ReceiveMessage", senderId, senderName, message, DateTime.UtcNow);
            
            // Send confirmation to sender
            await Clients.Caller.SendAsync("MessageSent", receiverId, message, DateTime.UtcNow);
        }

        public async Task NotifyTyping(string receiverId)
        {
            var senderId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(senderId))
            {
                await Clients.Group(receiverId).SendAsync("UserTyping", senderId);
            }
        }

        public async Task NotifyOnline()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.Others.SendAsync("UserOnline", userId);
            }
        }
    }
}
