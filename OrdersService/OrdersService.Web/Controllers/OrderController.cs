using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrdersService.Application.UseCases.CreateOrder;
using OrdersService.Application.UseCases.GetOrderById;
using OrdersService.Application.UseCases.CancelOrder;
using OrdersService.Application.UseCases.GetUserOrders;

namespace OrdersService.Web.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var orderId = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var query = new GetOrderByIdQuery(id);

        var order = await _mediator.Send(query);

        return order is not null ? Ok(order) : NotFound();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var command = new CancelOrderCommand(id);

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetUserOrders([FromQuery] Guid userId)
    {
        var query = new GetUserOrdersQuery(userId);

        var orders = await _mediator.Send(query);

        return Ok(orders);
    }
}