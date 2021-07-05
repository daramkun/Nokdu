using System;

namespace Nokdu.Net
{
    public interface IServer : IDisposable
    {
        bool IsConnectionServer { get; }
        bool IsAlive { get; }

        event EventHandler<IServerSession> SessionOpened;

        void Start();
        void Stop();
    }
}