using Hazelnut.Nokdu.Exceptions;

namespace Hazelnut.Nokdu.TestProgram;

public class TestActor : NokduActor
{
    protected override void OnInitialize(object? arguments)
    {
        Console.WriteLine("OnInitialize()");
        Console.WriteLine("Actor Name: {0}", Thread.CurrentThread.Name);
        Console.WriteLine("Actor Id: {0}", ActorId);
    }

    protected override void OnDestroy()
    {
        Console.WriteLine("OnDestroy()");
    }

    protected override bool OnCatchException(Exception ex)
    {
        Console.WriteLine("OnCatchException()");
        Console.WriteLine(ex);

        if (ex is MethodNotFoundException or MethodParameterNotMatchedException)
            return true;
        
        return base.OnCatchException(ex);
    }

    private void TestA()
    {
        Console.WriteLine("TestA()");
    }

    private void TestB(int value)
    {
        Console.WriteLine("TestB({0})", value);
    }

    private void TestC()
    {
        Console.WriteLine("TestC()");
    }
}