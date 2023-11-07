namespace Bom.Squad;

internal readonly struct PlatformInfo {

    public static bool Is64Bit { get; } = IntPtr.Size == 8;

    public static bool IsNetCore { get; } =
#if NETCOREAPP
        true;
#else
        false;
#endif

}