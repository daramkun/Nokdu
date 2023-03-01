namespace Hazelnut.Nokdu;

public class PreImplementedDispose : IDisposable
{
    ~PreImplementedDispose() => Dispose(false);
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing) { }
}