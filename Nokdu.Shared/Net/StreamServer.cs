using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Nokdu.Net
{
    public class StreamServer : IServer
    {
        private Socket _listenSocket;
        private readonly X509Certificate _certificate;

        private readonly ConcurrentDictionary<EndPoint, IServerSession> _sessions;

        public StreamServer(int backlog, EndPoint endPoint, X509Certificate cert)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(backlog);

            _certificate = cert;

            _sessions = new ConcurrentDictionary<EndPoint, IServerSession>();
        }

        ~StreamServer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (var session in _sessions.Values)
                session.Dispose();
            _sessions.Clear();

            if (disposing)
            {
                _listenSocket.Close();
                _listenSocket.Dispose();
            }

            _sessions.Clear();
            _listenSocket = null;
        }

        public bool IsConnectionServer => true;
        public bool IsAlive { get; private set; }

        public event EventHandler<IServerSession> SessionOpened;

        public void Start()
        {
            IsAlive = true;
            BeginAccept();
        }

        private void BeginAccept()
        {
            _listenSocket.BeginAccept((ar) =>
            {
                var acceptedSocket = _listenSocket.EndAccept(ar);
                var endPoint = acceptedSocket.RemoteEndPoint;

                var session = new StreamServerSession(this, acceptedSocket, _certificate);
                _sessions.TryAdd(endPoint, session);

                SessionOpened?.Invoke(this, session);

                BeginAccept();
            }, null);
        }

        public void Stop()
        {
            IsAlive = false;
            _listenSocket.Close();
        }

        internal void Disposed(StreamServerSession session)
        {
            _sessions.TryRemove(session.EndPoint, out _);
        }
    }

    internal sealed class StreamServerSession : IServerSession
    {
        private readonly WeakReference<IServer> _baseServer;

        private Socket _baseSocket;
        private Stream _baseStream;

        public IServer Parent => _baseServer.TryGetTarget(out var parent) ? parent : null;
        public EndPoint EndPoint { get; }

        public bool IsAlive
        {
            get
            {
                if (_baseSocket == null || _baseStream == null)
                    return false;

                return !(_baseSocket.Poll(int.MaxValue, SelectMode.SelectRead) && _baseSocket.Available == 0);
            }
        }

        public StreamServerSession(IServer server, Socket acceptedSocket, X509Certificate cert)
        {
            _baseServer = new WeakReference<IServer>(server);
            EndPoint = acceptedSocket.RemoteEndPoint;

            _baseSocket = acceptedSocket;
            _baseStream = new NetworkStream(_baseSocket, FileAccess.ReadWrite, true);

            if (cert != null)
            {
                var sslStream = new SslStream(_baseStream);
                sslStream.AuthenticateAsServer(cert);

                _baseStream = sslStream;
            }

            BeginReceive();
        }

        ~StreamServerSession()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                SessionClosed?.Invoke(this, EventArgs.Empty);

                _baseStream?.Dispose();
                _baseSocket?.Dispose();

                (Parent as StreamServer)?.Disposed(this);
            }

            _baseStream = null;
            _baseSocket = null;
        }

        public int SendTo(byte[] buffer, int offset, int length)
        {
            try
            {
                _baseStream.Write(buffer, offset, length);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode is SocketError.Disconnecting or SocketError.Shutdown or SocketError.HostDown or
                    SocketError.HostUnreachable or SocketError.NetworkDown or SocketError.NetworkUnreachable)
                    Dispose();

                return 0;
            }

            return length;
        }

        public int ReceiveFrom(byte[] buffer, int offset, int length)
        {
            try
            {
                return _baseStream.Read(buffer, offset, length);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode is SocketError.Disconnecting or SocketError.Shutdown or SocketError.HostDown or
                    SocketError.HostUnreachable or SocketError.NetworkDown or SocketError.NetworkUnreachable)
                    Dispose();

                return 0;
            }
        }

        public event EventHandler SessionClosed;
        public event EventHandler DataReceived;

        private void BeginReceive()
        {
            ThreadPool.QueueUserWorkItem(DetectReceiveableDelegate, this);
        }

        private static readonly WaitCallback DetectReceiveableDelegate = DetectReceiveable;

        private static void DetectReceiveable(object state)
        {
            var session = state as StreamServerSession;

            if (session?._baseSocket != null && session._baseSocket.Poll(-1, SelectMode.SelectRead))
                session.DataReceived?.Invoke(session, EventArgs.Empty);

            if (session?._baseSocket == null)
                return;

            session.BeginReceive();
        }
    }
}