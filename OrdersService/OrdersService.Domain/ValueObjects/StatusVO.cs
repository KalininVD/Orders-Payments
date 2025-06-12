namespace OrdersService.Domain.ValueObjects;

public class StatusVO
{
    public string Value { get; }

    private StatusVO(string value)
    {
        Value = value;
    }

    public static StatusVO New => new("New");

    public static StatusVO Finished => new("Finished");

    public static StatusVO Cancelled => new("Cancelled");

    public override string ToString() => Value;
}