using FluentAssertions;
using PaymentsService.Application.UseCases.CreateAccount;
using Xunit;

namespace PaymentsService.Application.UnitTests.UseCases;

public class CreateAccountCommandValidatorTests
{
    private readonly CreateAccountCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_UserIdIsEmpty()
    {
        // Arrange
        var command = new CreateAccountCommand(Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.UserId));
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        var command = new CreateAccountCommand(Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}