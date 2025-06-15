namespace OrdersService.Application.UseCases.GetOrderById;

public record OrderResponse(Guid Id, Guid UserId, decimal Amount, string Description, string Status);