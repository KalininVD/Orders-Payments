namespace PaymentsService.Application.UseCases.GetAccountById;

public record AccountResponse(Guid Id, Guid UserId, decimal Balance);