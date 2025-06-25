using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace APIGateway.Services;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<WebSocket>> _userSockets = new();

    public void AddSocket(Guid userId, WebSocket socket)
    {
        var sockets = _userSockets.GetOrAdd(userId, _ => []);

        sockets.Add(socket);
    }

    public void RemoveSocket(Guid userId, WebSocket socket)
    {
        if (_userSockets.TryGetValue(userId, out var sockets))
        {
            var updatedSockets = new ConcurrentBag<WebSocket>(sockets.Where(s => s != socket));

            if (updatedSockets.IsEmpty)
            {
                _userSockets.TryRemove(userId, out _);
            }
            else
            {
                _userSockets.TryUpdate(userId, updatedSockets, sockets);
            }
        }
    }

    public async Task SendMessageToUserAsync(Guid userId, string message)
    {
        if (_userSockets.TryGetValue(userId, out var sockets))
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);

            var sendTasks = sockets.Select(socket =>
            {
                if (socket.State == WebSocketState.Open)
                {
                    return socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                return Task.CompletedTask;
            }).ToList();

            await Task.WhenAll(sendTasks);
        }
    }

    public async Task CloseAndRemoveSocketAsync(Guid userId, WebSocket socket, ILogger logger)
    {
        try
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
            }
        }
        catch (WebSocketException ex)
        {
            logger.LogWarning(ex, "WebSocket already closed for User: {UserId}", userId);
        }
        finally
        {
            RemoveSocket(userId, socket);
            logger.LogInformation("WebSocket connection removed for User: {UserId}", userId);
        }
    }
}