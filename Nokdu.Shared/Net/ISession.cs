using System;
using System.Net;

namespace Nokdu.Net
{
    public interface ISession : ISender, IReceiver
    {
        event EventHandler SessionClosed;
        event EventHandler DataReceived;

        EndPoint EndPoint { get; }

        bool IsAlive { get; }
    }

    public interface IServerSession : ISession, IDisposable
    {
        IServer Parent { get; }
    }
}