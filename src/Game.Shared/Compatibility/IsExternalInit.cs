// Guards on the absence of a modern .NET moniker rather than a specific TFM symbol, because
// Unity's own compiler pass (used when this folder is consumed as a Unity local package)
// defines neither NETSTANDARD2_1 nor NET5_0_OR_GREATER - this condition is true for both the
// netstandard2.1 dotnet build and the Unity build, and false for net10.0 where the BCL already
// provides this type.
#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Polyfill so that C# 9+ `init`-only record properties compile under netstandard2.1 and
    /// under Unity (max API compatibility level netstandard2.1), both of which predate this
    /// compiler-recognized marker type. Never used at runtime; the compiler only checks for
    /// its existence.
    /// </summary>
    internal static class IsExternalInit
    {
    }
}
#endif
