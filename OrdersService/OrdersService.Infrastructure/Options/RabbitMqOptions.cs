using System.ComponentModel.DataAnnotations;

namespace OrdersService.Infrastructure.Options;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    [Required]
    public string Host { get; init; } = null!;

    [Required]
    public string User { get; init; } = null!;

    [Required]
    public string Password { get; init; } = null!;
}