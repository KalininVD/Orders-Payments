using System.Net.WebSockets;
using System.Text;
using APIGateway.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Notifications;

namespace APIGateway.Endpoints;

public static class WebSocketEndpoints
{
    public static void MapWebSocketEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/ws", async (HttpContext context, ConnectionManager manager, ILogger<Program> logger) =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            if (!Guid.TryParse(context.Request.Query["userId"], out var userId))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("userId query parameter is required and must be a valid Guid.");
                return;
            }

            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            manager.AddSocket(userId, webSocket);
            logger.LogInformation("WebSocket connection established for User: {UserId}", userId);

            try
            {
                var buffer = new byte[1024 * 4];
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!receiveResult.CloseStatus.HasValue)
                {
                    receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
            }
            catch (WebSocketException ex)
            {
                logger.LogWarning(ex, "WebSocket error for User: {UserId}", userId);
            }
            finally
            {
                await manager.RemoveSocketAsync(userId);
                logger.LogInformation("WebSocket connection closed for User: {UserId}", userId);
            }
        });

        app.MapPost("/internal-api/notify",
            async ([FromBody] NotificationRequest request, ConnectionManager manager, ILogger<Program> logger) =>
        {
            var socket = manager.GetSocketByUserId(request.UserId);
            if (socket is not null && socket.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(request.Message);
                await socket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                logger.LogInformation("Sent notification to User: {UserId}", request.UserId);
                return Results.Ok();
            }

            logger.LogWarning("Socket not found or was closed for User: {UserId}", request.UserId);
            return Results.NotFound(new { Message = $"Connection for user {request.UserId} not found." });
        });
    }
}