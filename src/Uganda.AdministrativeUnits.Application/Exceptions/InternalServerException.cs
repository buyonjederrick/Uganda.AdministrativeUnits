using System;

namespace Uganda.AdministrativeUnits.Application.Exceptions;

public class InternalServerException : Exception
{
    public InternalServerException(string message)
        : base(message)
    {
    }

    public InternalServerException(string[] errors)
        : base("Multiple errors occurred. See error details.")
    {
        Errors = errors;
    }

    public string[] Errors { get; set; } = [];
}
