using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace APIGateway.Services;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();

    public void AddSocket(Guid userId, WebSocket socket)
    {
        _sockets.TryAdd(userId, socket);
    }

    public async Task RemoveSocketAsync(Guid userId)
    {
        if (_sockets.TryRemove(userId, out var socket))
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by server", CancellationToken.None);
            }
        }
    }

    public WebSocket? GetSocketByUserId(Guid userId)
    {
        return _sockets.TryGetValue(userId, out var socket) ? socket : null;
    }
}