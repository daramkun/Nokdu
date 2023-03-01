namespace Hazelnut.Nokdu.Exceptions;

public class MethodParameterNotMatchedException : Exception
{
    public string MethodName { get; }

    public MethodParameterNotMatchedException(string methodName)
        : base("Matched Overloaded method is not found.")
    {
        MethodName = methodName;
    }

    public override string ToString() => $"Not found matched method: {MethodName}. {Message}";
}