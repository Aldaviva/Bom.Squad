using System.Reflection;
#if NET6_0
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UseUtf8StringLiteral
#endif

namespace Bom.Squad;

internal static class Workarounds {

    public static void ApplyWorkarounds() {
        try {
            FixUtf8Json();
        } catch (NotSupportedException) {
            // method was removed during trimming
        }
    }

    /// <summary>
    /// <para>Utf8Json needs to read the UTF-8 BOM once when it first loads.</para>
    /// <para>To prevent it from crashing with an <see cref="IndexOutOfRangeException"/> inside <c>JsonSerializer.DeserializeAsync</c> and related methods, construct a throwaway <c>JsonReader</c> instance before disabling the BOM.</para>
    /// <para>https://www.nuget.org/packages/ZCS.Utf8Json</para>
    /// </summary>
    /// <exception cref="NotSupportedException">if this method was removed during trimming because it uses reflection, and Utf8Json doesn't work in AOT programs anyway</exception>
#if NET6_0
    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2057",
        Justification = "Entire FixUtf8Json() method is replaced with 'throw new NotSupportedException()' by the AOT linker, configured using 'ILLink.Substitutions.xml'.")]
#endif
    private static void FixUtf8Json() {
        const string originalAssemblyName       = "Utf8Json";
        const string namespaceQualifiedTypeName = "Utf8Json.JsonReader";
        try {
            (Type.GetType($"{namespaceQualifiedTypeName}, {originalAssemblyName}") ??
                    Type.GetType($"{namespaceQualifiedTypeName}, {Assembly.GetEntryAssembly()?.GetName().Name}"))?
                .GetConstructor([typeof(byte[])])?
                .Invoke([new[] { (byte) '1', (byte) '1', (byte) '1' }]);
        } catch (Exception e) when (e is not OutOfMemoryException) { }
    }

}