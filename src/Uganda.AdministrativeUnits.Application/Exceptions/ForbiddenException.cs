using System;

namespace Uganda.AdministrativeUnits.Application.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message)
        : base(message)
    {
    }

    public ForbiddenException(string[] errors)
        : base("Multiple errors occurred. See error details.")
    {
        Errors = errors;
    }

    public string[] Errors { get; set; } = [];
}
