namespace OrdersService.Application.UseCases.GetUserOrders;

public record OrdersResponse(Guid Id, Guid UserId, decimal Amount, string Description, string Status);