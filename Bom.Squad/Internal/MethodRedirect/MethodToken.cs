using System.Runtime.InteropServices;

namespace Bom.Squad.Internal.MethodRedirect;

/// <summary>
/// Source: https://github.com/spinico/MethodRedirect/blob/1e2a681c22dc3781cfd36a8f683c3b808456f429/Sources/MethodToken.cs
/// </summary>
internal readonly struct MethodToken: IDisposable {

    public IntPtr Address { get; }
    public IntPtr Value { get; }

    public MethodToken(IntPtr address) {
        // On token creation, preserve the address and the current value at this address
        Address = address;
        Value   = Marshal.ReadIntPtr(address);
    }

    public void Restore() {
        // Restore the value at the address            
        Marshal.Copy(new[] { Value }, 0, Address, 1);
    }

    public override string ToString() {
        IntPtr met = Address;
        IntPtr tar = Marshal.ReadIntPtr(Address);
        IntPtr ori = Value;

        return "Method address = " + met.ToString("x").PadLeft(8, '0') + "\n" +
            "Target address = " + tar.ToString("x").PadLeft(8, '0') + "\n" +
            "Origin address = " + ori.ToString("x").PadLeft(8, '0');
    }

    public void Dispose() {
        Restore();
    }

}