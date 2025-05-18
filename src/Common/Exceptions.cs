using System;

namespace Common;

/// <summary>
///     Thrown when an exception related to the monolith occurs.
/// </summary>
public class MonolithException : Exception
{
    public MonolithException(string message)
        : base(message)
    {
    }

    public MonolithException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
///     Thrown when an exception occurs during the startup of the monolith.
/// </summary>
public class MonolithStartupException : Exception
{
    public MonolithStartupException(string message)
        : base(message)
    {
    }

    public MonolithStartupException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}