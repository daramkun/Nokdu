namespace Hazelnut.Nokdu.Exceptions;

public class MethodNotFoundException : Exception
{
    public string MethodName { get; }

    public MethodNotFoundException(string methodName)
        : base("Method is not found.")
    {
        MethodName = methodName;
    }

    public override string ToString() => $"Not found method: {MethodName}. {Message}";
}