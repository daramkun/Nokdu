using System;
using System.Net;
using System.Threading.Tasks;

namespace Nokdu.Net
{
    public interface IClient : IDisposable, ISession
    {
        bool IsConnectionClient { get; }

        void Start();
        Task StartAsync();
        void Stop();
    }
}