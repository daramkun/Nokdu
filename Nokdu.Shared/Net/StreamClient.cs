using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Nokdu.Net
{
    public class StreamClient : IClient
    {
        private Socket _baseSocket;
        private Stream _baseStream;

        public event EventHandler SessionClosed;
        public event EventHandler DataReceived;
        public EndPoint EndPoint { get; }

        public bool IsConnectionClient => true;
        public bool IsSslStream { get; }

        public bool IsAlive
        {
            get
            {
                if (_baseSocket == null || _baseStream == null)
                    return false;

                return !(_baseSocket.Poll(int.MaxValue, SelectMode.SelectRead) && _baseSocket.Available == 0);
            }
        }

        public event EventHandler SessionStarted;

        public StreamClient(EndPoint endPoint, bool ssl = false)
        {
            EndPoint = endPoint;

            IsSslStream = ssl;

            _baseSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        ~StreamClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            SessionClosed?.Invoke(this, EventArgs.Empty);

            _baseStream?.Dispose();
            _baseStream = null;

            _baseSocket?.Dispose();
            _baseSocket = null;
        }

        public int SendTo(byte[] buffer, int offset, int length)
        {
            _baseStream.Write(buffer, offset, length);
            return length;
        }

        public int ReceiveFrom(byte[] buffer, int offset, int length)
        {
            return _baseStream.Read(buffer, offset, length);
        }

        public void Start()
        {
            _baseSocket.BeginConnect(EndPoint, ar =>
            {
                _baseSocket.EndConnect(ar);

                _baseStream = new NetworkStream(_baseSocket, FileAccess.ReadWrite, true);

                if (IsSslStream)
                {
                    _baseStream = new SslStream(_baseStream);
                    (_baseStream as SslStream).AuthenticateAsClient(new SslClientAuthenticationOptions()
                    {
                        EncryptionPolicy = EncryptionPolicy.RequireEncryption,
                        EnabledSslProtocols = SslProtocols.Tls13,
                    });
                }

                SessionStarted?.Invoke(this, EventArgs.Empty);

                BeginReceive();
            }, null);
        }

        public Task StartAsync()
        {
            return _baseSocket.ConnectAsync(EndPoint)
                .ContinueWith((task, _) =>
                {
                    SessionStarted?.Invoke(this, EventArgs.Empty);

                    BeginReceive();
                }, null);
        }

        public void Stop()
        {
            _baseSocket.Close();
        }

        private void BeginReceive()
        {
            ThreadPool.QueueUserWorkItem(DetectReceiveableDelegate, this);
        }

        private static readonly WaitCallback DetectReceiveableDelegate = DetectReceiveable;

        private static void DetectReceiveable(object state)
        {
            var client = state as StreamClient;

            if (client._baseSocket != null && client._baseSocket.Poll(-1, SelectMode.SelectRead))
                client.DataReceived?.Invoke(client, EventArgs.Empty);

            if (client._baseSocket == null)
                return;

            client.BeginReceive();
        }
    }
}