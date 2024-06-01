using System.Reflection;
using System.Text;
using System.Threading;

namespace Bom.Squad;

/// <summary>
/// <para>Allows you to modify the base class library's shared, process-wide <see cref="Encoding.UTF8"/> instance to not output byte order marks when encoding, although it will still parse them correctly.</para>
/// <para>Other encodings, such as <see cref="Encoding.Unicode"/> and <c>new UTF8Encoding(true, true)</c>, will not be affected. Other processes are not affected. Does not modify any assemblies on disk.</para>
/// <para> </para>
/// <para>Usage:</para>
/// <para><c>BomSquad.DefuseUtf8Bom();</c></para>
/// </summary>
public static class BomSquad {

    private const BindingFlags EmitUtf8IdentifierFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

    private static readonly Encoding Utf8 = Encoding.UTF8;

    private static readonly Lazy<FieldInfo> EmitUtf8IdentifierField = new(() => {
            Type utf8EncodingClass = typeof(UTF8Encoding);
            return utf8EncodingClass.GetField("_emitUTF8Identifier", EmitUtf8IdentifierFieldFlags) ?? // .NET Core
                utf8EncodingClass.GetField("emitUTF8Identifier", EmitUtf8IdentifierFieldFlags)!;      // .NET Framework
        },
        LazyThreadSafetyMode.PublicationOnly);

    private static bool EmitUtf8Identifier {
        get => (bool) EmitUtf8IdentifierField.Value.GetValue(Utf8)!;
        set => EmitUtf8IdentifierField.Value.SetValue(Utf8, value);
    }

    /// <summary>
    /// <inheritdoc cref="BomSquad" />
    /// <para> </para>
    /// <para>After calling this method, the <see cref="Encoding.UTF8"/> instance will behave as if was constructed with the <c>encoderShouldEmitUTF8Identifier</c> constructor parameter set to <c>false</c>, so it will not write BOMs.</para>
    /// </summary>
    public static void DefuseUtf8Bom() {
        if (EmitUtf8Identifier) {
            Workarounds.ApplyWorkarounds();
            EmitUtf8Identifier = false;
        }
    }

    internal static void RearmUtf8Bom() {
        if (!EmitUtf8Identifier) {
            EmitUtf8Identifier = true;
        }
    }

}