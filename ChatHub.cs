using Microsoft.AspNetCore.SignalR;

public sealed class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        user = string.IsNullOrWhiteSpace(user) ? "Anonymous" : user.Trim();
        message = message?.Trim() ?? string.Empty;

        if (message.Length == 0)
            return;

        await Clients.All.SendAsync("ReceiveMessage", user, message, DateTimeOffset.UtcNow);
    }
}

