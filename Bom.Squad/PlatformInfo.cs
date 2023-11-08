namespace Bom.Squad;

internal readonly struct PlatformInfo {

    public static bool IsProcess64Bit { get; } = IntPtr.Size == 8;

    public static Runtime? ProcessRuntime { get; } = typeof(object).Assembly.GetName().Name switch {
        "mscorlib"               => Runtime.NetFramework,
        "System.Private.CoreLib" => Runtime.NetCore,
        _                        => null
    };

    public enum Runtime {

        NetFramework,
        NetCore

    }

}