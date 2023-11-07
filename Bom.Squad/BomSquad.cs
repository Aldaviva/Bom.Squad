using System.Runtime.InteropServices;
using System.Text;

namespace Bom.Squad;

/// <summary>
/// <para>Allows you to modify the base class library <see cref="Encoding.UTF8"/> to not output byte order markers when encoding, although it will still parse them correctly.</para>
/// <para>Other encodings, such as <see cref="Encoding.Unicode"/> and <c>new UTF8Encoding(true, true)</c>, will not be affected.</para>
///
/// <para>Usage:</para>
/// <para><c>BomSquad.DefuseUtf8Bom();</c></para>
/// </summary>
public static class BomSquad {

    /// <summary>
    /// <para>Modify the base class library <see cref="Encoding.UTF8"/> to not output byte order markers when encoding, although it will still parse them correctly.</para>
    /// <para>After calling this method, the <see cref="Encoding.UTF8"/> instance will behave as if was constructed with the <c>encoderShouldEmitUTF8Identifier</c> constructor parameter set to false, so it will not write BOMs.</para>
    /// <para>Other encodings, such as <see cref="Encoding.Unicode"/> and <c>new UTF8Encoding(true, true)</c>, will not be affected.</para>
    /// </summary>
    /// <exception cref="AccessViolationException">if dereferencing one of the pointers fails</exception>
    public static void DefuseUtf8Bom() {
        Encoding utf8 = Encoding.UTF8;
        if (utf8.GetPreamble().LongLength > 0) {
            GCHandle gcHandle = GCHandle.Alloc(utf8, GCHandleType.WeakTrackResurrection);
            IntPtr   pointer1 = GCHandle.ToIntPtr(gcHandle);
            IntPtr   pointer2 = Marshal.ReadIntPtr(pointer1);

            int emitUtf8IdentifierFieldOffset = PlatformInfo.IsNetCore switch {
                true when PlatformInfo.Is64Bit  => 37,
                true                            => 21,
                false when PlatformInfo.Is64Bit => 38,
                false                           => 22
            };

            Marshal.WriteByte(pointer2, emitUtf8IdentifierFieldOffset, 0); // set private readonly bool UTF8Encoding._emitUTF8Identifier to false

            gcHandle.Free();
        }
    }

}