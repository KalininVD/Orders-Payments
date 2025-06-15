using MediatR;

namespace OrdersService.Application.UseCases.CreateOrder;

public record CreateOrderCommand(Guid UserId, decimal Amount, string Description) : IRequest<Guid>;