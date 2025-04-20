namespace Common;

public record Configuration
{
    public int PORT { get; init; } = 5000;
    public string DB_CONNECTION_STRING { get; init; }
}
