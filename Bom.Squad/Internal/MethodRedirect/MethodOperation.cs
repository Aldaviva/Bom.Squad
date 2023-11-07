namespace Bom.Squad.Internal.MethodRedirect;

/// <summary>
/// Base method operation result
/// Source: https://github.com/spinico/MethodRedirect/blob/1e2a681c22dc3781cfd36a8f683c3b808456f429/Sources/MethodOperation.cs
/// </summary>
internal abstract class MethodOperation: IDisposable {

    public abstract void Restore();

    public void Dispose() {
        Restore();
    }

}

/// <summary>
/// Result of a method redirection (Origin => *)
/// </summary>
internal class MethodRedirection: MethodOperation {

    public MethodToken Origin { get; }

    public MethodRedirection(IntPtr address) {
        Origin = new MethodToken(address);
    }

    public override void Restore() {
        Origin.Restore();
    }

    public override string ToString() {
        return Origin.ToString();
    }

}