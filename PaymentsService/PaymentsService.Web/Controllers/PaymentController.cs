using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentsService.Application.UseCases.CreateAccount;
using PaymentsService.Application.UseCases.GetAccountById;
using PaymentsService.Application.UseCases.DepositFunds;
using PaymentsService.Application.UseCases.GetAccountByUserId;

namespace PaymentsService.Web.Controllers;

[ApiController]
[Route("api/accounts")]
public class PaymentController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var accountId = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetAccountById), new { id = accountId }, accountId);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var query = new GetAccountByIdQuery(id);

        var account = await _mediator.Send(query);

        return account is not null ? Ok(account) : NotFound();
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetAccountByUserId(Guid userId)
    {
        var query = new GetAccountByUserIdQuery(userId);

        var account = await _mediator.Send(query);

        return account is not null ? Ok(account) : NotFound();
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> DepositFunds([FromBody] DepositFundsCommand command)
    {
        await _mediator.Send(command);

        return Ok(new { message = $"Successfully deposited {command.Amount} for user {command.UserId}" });
    }
}