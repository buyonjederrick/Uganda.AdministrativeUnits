using System;
using System.Text;

namespace Uganda.AdministrativeUnits.Internal;

/// <summary>
/// Culture-independent name folding used for lookups: upper-cases, strips Latin diacritics,
/// unifies apostrophes and collapses whitespace. Deliberately avoids <see cref="string.Normalize()"/>
/// so results are identical on every OS, ICU configuration and under invariant globalization.
/// </summary>
internal static class NameNormalizer
{
    // Folds U+00C0..U+00FF (Latin-1 Supplement letters) to their ASCII base letter; '\0' = leave unchanged.
    private const string Latin1Fold =
        "AAAAAAACEEEEIIIIDNOOOOO\0OUUUUY\0\0" + // U+00C0..U+00DF (upper-case block)
        "AAAAAAACEEEEIIIIDNOOOOO\0OUUUUY\0Y";   // U+00E0..U+00FF (lower-case block, folded to upper)

    public static string Normalize(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (IsAlreadyNormalized(value))
        {
            return value;
        }

        var sb = new StringBuilder(value.Length);
        bool pendingSpace = false;

        foreach (char raw in value)
        {
            char ch = Fold(raw);
            if (char.IsWhiteSpace(ch))
            {
                pendingSpace = sb.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                sb.Append(' ');
                pendingSpace = false;
            }

            sb.Append(ch);
        }

        return sb.ToString();
    }

    private static char Fold(char ch)
    {
        if (ch is '\u2018' or '\u2019' or '\u02BC' or '`' or '\u00B4')
        {
            return '\'';
        }

        if (ch is >= '\u00C0' and <= '\u00FF')
        {
            char folded = Latin1Fold[ch - '\u00C0'];
            return folded == '\0' ? ch : folded;
        }

        return char.ToUpperInvariant(ch);
    }

    /// <summary>Fast path: true when the text is already upper-case ASCII with single inner spaces.</summary>
    private static bool IsAlreadyNormalized(string value)
    {
        if (value.Length == 0)
        {
            return true;
        }

        char previous = ' ';
        foreach (char ch in value)
        {
            bool plain = ch is (>= 'A' and <= 'Z') or (>= '0' and <= '9') or (>= '!' and <= '/' and not '\'' and not '`')
                || ch is ':' or ';' or '=' or '?' or '@' or '[' or ']' or '_' or '\'';
            bool ok = plain || (ch == ' ' && previous != ' ');
            if (!ok)
            {
                return false;
            }

            previous = ch;
        }

        return previous != ' ' && value[0] != ' ';
    }
}
