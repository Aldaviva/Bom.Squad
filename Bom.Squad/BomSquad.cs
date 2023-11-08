using System.Runtime.InteropServices;
using System.Text;

namespace Bom.Squad;

/// <summary>
/// <para>Allows you to modify the base class library <see cref="Encoding.UTF8"/> to not output byte order marks when encoding, although it will still parse them correctly.</para>
/// <para>Other encodings, such as <see cref="Encoding.Unicode"/> and <c>new UTF8Encoding(true, true)</c>, will not be affected.</para>
///
/// <para>Usage:</para>
/// <para><c>BomSquad.DefuseUtf8Bom();</c></para>
/// </summary>
public static class BomSquad {

    private const int MinScanOffset = 5; // if we write to offset 4, it causes a segfault on .NET 7 MacOS (ARM64 or x86_64)
    private const int MaxScanOffset = 76;

    private static readonly Encoding Utf8 = Encoding.UTF8;

    private static bool IsBomDefused => Utf8.GetPreamble().Length == 0;

    private static int? _emitUtf8IdentifierFieldOffset;

    /// <summary>
    /// <para>Modify the base class library <see cref="Encoding.UTF8"/> to not output byte order marks when encoding, although it will still parse them correctly.</para>
    /// <para>After calling this method, the <see cref="Encoding.UTF8"/> instance will behave as if was constructed with the <c>encoderShouldEmitUTF8Identifier</c> constructor parameter set to false, so it will not write BOMs.</para>
    /// <para>Other encodings, such as <see cref="Encoding.Unicode"/> and <c>new UTF8Encoding(true, true)</c>, will not be affected.</para>
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">If this library does not support the current operating system, runtime, and CPU architecture, either explicitly or automatically.</exception>
    /// <exception cref="AccessViolationException">If this process' memory cannot be read or written.</exception>
    public static void DefuseUtf8Bom() {
        if (!IsBomDefused) {
            Workarounds.ApplyWorkarounds();

            GCHandle gcHandle = GCHandle.Alloc(Utf8, GCHandleType.WeakTrackResurrection);
            try {
                IntPtr gcHandlePointer = GCHandle.ToIntPtr(gcHandle);
                IntPtr utf8Pointer     = Marshal.ReadIntPtr(gcHandlePointer);

                bool is64Bit = PlatformInfo.IsProcess64Bit;
                int? emitUtf8IdentifierFieldOffset = _emitUtf8IdentifierFieldOffset ?? PlatformInfo.ProcessRuntime switch {
                    PlatformInfo.Runtime.NetCore when is64Bit      => 37,
                    PlatformInfo.Runtime.NetCore                   => 21,
                    PlatformInfo.Runtime.NetFramework when is64Bit => 38,
                    PlatformInfo.Runtime.NetFramework              => 22,
                    _                                              => null
                };

                if (emitUtf8IdentifierFieldOffset != null) {
                    // set private readonly bool UTF8Encoding._emitUTF8Identifier to false
                    Marshal.WriteByte(utf8Pointer, emitUtf8IdentifierFieldOffset.Value, 0);
                } else if ((emitUtf8IdentifierFieldOffset = ScanForBomOffset(utf8Pointer)) == null) {
                    throw new PlatformNotSupportedException(
                        "Bom.Squad does not yet have the ability to disable Encoding.UTF8 BOM on this operating system, .NET runtime, and CPU architecture combination. Please file an issue at https://github.com/Aldaviva/Bom.Squad/issues/new with this information.");
                }

                _emitUtf8IdentifierFieldOffset = emitUtf8IdentifierFieldOffset;
            } finally {
                gcHandle.Free();
            }
        }
    }

    /// <exception cref="AccessViolationException">If this process' memory cannot be read or written.</exception>
    internal static void RearmUtf8Bom() {
        if (IsBomDefused && _emitUtf8IdentifierFieldOffset != null) {
            GCHandle gcHandle = GCHandle.Alloc(Utf8, GCHandleType.WeakTrackResurrection);
            try {
                Marshal.WriteByte(Marshal.ReadIntPtr(GCHandle.ToIntPtr(gcHandle)), _emitUtf8IdentifierFieldOffset.Value, 1);
            } finally {
                gcHandle.Free();
            }
        }
    }

    internal static int? ScanForBomOffset(IntPtr utf8Pointer2) {
        for (int offset = MinScanOffset; offset <= MaxScanOffset; offset++) {
            try {
                byte oldValue = Marshal.ReadByte(utf8Pointer2, offset);
                if (oldValue == 1) {
                    Marshal.WriteByte(utf8Pointer2, offset, 0);
                    if (IsBomDefused) {
                        return offset;
                    } else {
                        Marshal.WriteByte(utf8Pointer2, offset, oldValue);
                    }
                }
            } catch {
                //continue to next offset
            }
        }

        return null;
    }

}