using System.Reflection;

namespace Bom.Squad;

internal static class Workarounds {

    public static void ApplyWorkarounds() {
        FixUtf8Json();
    }

    /// <summary>
    /// <para>Utf8Json needs to read the UTF-8 BOM once when it first loads.</para>
    /// <para>To prevent it from crashing with an <see cref="IndexOutOfRangeException"/> inside <c>JsonSerializer.DeserializeAsync</c> and related methods, construct a throwaway <c>JsonReader</c> instance before disabling the BOM.</para>
    /// <para>https://www.nuget.org/packages/ZCS.Utf8Json</para>
    /// </summary>
    private static void FixUtf8Json() {
        const string originalAssemblyName       = "Utf8Json";
        const string namespaceQualifiedTypeName = "Utf8Json.JsonReader";
        try {
            (Type.GetType($"{namespaceQualifiedTypeName}, {originalAssemblyName}")
                    ?? Type.GetType($"{namespaceQualifiedTypeName}, {Assembly.GetEntryAssembly()?.GetName().Name}"))? // ILRepack changes assembly name
                .GetConstructor(new[] { typeof(byte[]) })?
                .Invoke(new object[] { new[] { (byte) '1', (byte) '1', (byte) '1' } });
        } catch (Exception e) when (e is not OutOfMemoryException) { }
    }

}