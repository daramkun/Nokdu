// See https://aka.ms/new-console-template for more information

using Hazelnut.Nokdu.TestProgram;

Console.WriteLine("new TestActor()");
var testActor = new TestActor();

ulong messageId = 0;

while (testActor.IsAlive)
{
    var readKey = Console.ReadKey();
    
    if (readKey.Key == ConsoleKey.A)
        testActor.SendMessage("TestA");
    else if (readKey.Key == ConsoleKey.B)
        testActor.SendMessage("TestB", 10);
    else if (readKey.Key == ConsoleKey.C)
        messageId = testActor.SendMessageAfter("TestC", TimeSpan.FromSeconds(10));
    else if (readKey.Key == ConsoleKey.D)
    {
        testActor.CancelMessage(messageId);
        Console.WriteLine("Message Canceled");
    }
    else if (readKey.Key == ConsoleKey.Y)
        testActor.SendMessage("NoExistMessage");
    else if (readKey.Key == ConsoleKey.Q)
        testActor.Dispose();
    
    Thread.Sleep(0);
}