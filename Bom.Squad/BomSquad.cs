using Bom.Squad.Internal.MethodRedirect;
using System.Reflection;
using System.Text;

namespace Bom.Squad;

/// <summary>
/// <para>Allows you to modify the base class library <see cref="Encoding.UTF8"/> to not output byte order markers when encoding, although it will parse them correctly.</para>
/// <para>Other encodings, such as <see cref="Encoding.Unicode"/> and <c>new UTF8Encoding(true, true)</c>, will not be affected.</para>
///
/// <para>Usage:</para>
/// <para><c>BomSquad.DefuseUtf8Bom();</c></para>
/// </summary>
public static class BomSquad {

    /// <summary>
    /// A UTF-8 <see cref="Encoding"/> instance that does not output byte order markers when encoding, although it will parse them correctly.
    /// It throws exceptions when invalid bytes are detected.
    /// </summary>
    public static readonly Encoding Utf8NoBom = new UTF8Encoding(false, true);

    private static bool _armed = true;

    /// <summary>
    /// <para>Modify the base class library <see cref="Encoding.UTF8"/> to not output byte order markers when encoding, although it will parse them correctly.</para>
    /// <para>After calling this method, subsequent calls to <see cref="Encoding.UTF8"/> will not write BOMs.</para>
    /// <para>Other encodings, such as <see cref="Encoding.Unicode"/> and <c>new UTF8Encoding(true, true)</c>, will not be affected.</para>
    /// </summary>
    // ExceptionAdjustment: M:System.Type.GetProperty(System.String,System.Reflection.BindingFlags) -T:System.Reflection.AmbiguousMatchException
    // ExceptionAdjustment: M:System.Type.GetMethod(System.String,System.Reflection.BindingFlags) -T:System.Reflection.AmbiguousMatchException
    public static void DefuseUtf8Bom() {
        if (_armed) {
            MethodInfo oldMethod = typeof(Encoding).GetProperty("UTF8", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty)!.GetMethod;
            MethodInfo newMethod = typeof(BomSquad).GetMethod(nameof(Utf8), BindingFlags.Static | BindingFlags.NonPublic)!;

            oldMethod.RedirectTo(newMethod, true);
            _armed = false;
        }
    }

    private static Encoding Utf8() => Utf8NoBom;

}