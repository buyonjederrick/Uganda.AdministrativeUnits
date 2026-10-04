using System;

namespace Uganda.AdministrativeUnits.Application.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message)
        : base(message)
    {
    }

    public UnauthorizedException(string[] errors)
        : base("Multiple errors occurred. See error details.")
    {
        Errors = errors;
    }

    public string[] Errors { get; set; } = [];
}
