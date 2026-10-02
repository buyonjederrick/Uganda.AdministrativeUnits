using System;

namespace Uganda.AdministrativeUnits.Internal;

internal static class Guard
{
    public static void NotNull(object? value, string paramName)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    public static void Positive(int value, string paramName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
        }
    }
}
