#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved type used by the compiler for <c>init</c> / <c>record</c> support on older TFMs.
/// </summary>
internal static class IsExternalInit
{
}
#endif
