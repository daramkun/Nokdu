using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Nokdu.Net
{
    public class DatagramServer : IServer
    {
        private static readonly byte[] ZeroSizeBytes = new byte[0];

        private Socket _bindSocket;
        private readonly EndPoint _bindEndPoint;

        internal Socket BindSocket => _bindSocket;

        private readonly ConcurrentDictionary<EndPoint, DatagramServerSession> _aliveSessions = new();

        public bool IsConnectionServer { get; }
        public bool IsAlive { get; private set; }

        public event EventHandler<IServerSession> SessionOpened;

        public DatagramServer(EndPoint endPoint)
        {
            _bindSocket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = 1000,
                SendTimeout = 1000,
                Blocking = false,
            };

            _bindSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 64 * 10246);
            _bindSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 64 * 1024);

            _bindSocket.Bind(_bindEndPoint = endPoint);
        }

        ~DatagramServer()
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
            _bindSocket.Close();
            _bindSocket.Dispose();
            _bindSocket = null;
        }

        public void Start()
        {
            StartReceiveFrom();
            IsAlive = true;
        }

        public void Stop()
        {
            IsAlive = false;
            foreach (var session in _aliveSessions.Values)
                session.Dispose();
            _aliveSessions.Clear();
        }

        private void StartReceiveFrom()
        {
            var remoteEndPoint = _bindEndPoint;
            _bindSocket.BeginReceiveFrom(ZeroSizeBytes, 0, 0, SocketFlags.None, ref remoteEndPoint,
                (ar) =>
                {
                    if (!IsAlive)
                        return;

                    var endRemoteEndPoint = _bindEndPoint;

                    try
                    {
                        var readLength = _bindSocket.EndReceiveFrom(ar, ref endRemoteEndPoint);

                        if (!_aliveSessions.ContainsKey(endRemoteEndPoint))
                        {
                            var newSession = new DatagramServerSession(this, endRemoteEndPoint);
                            _aliveSessions.TryAdd(endRemoteEndPoint, newSession);

                            SessionOpened?.Invoke(this, newSession);
                        }

                        var session = _aliveSessions[endRemoteEndPoint];

                        session.OnReceived();

                        StartReceiveFrom();
                    }
                    catch (SocketException ex)
                    {
                        if (!(ex.SocketErrorCode == SocketError.TimedOut ||
                              ex.SocketErrorCode == SocketError.WouldBlock))
                        {
                        }
                    }
                },
                null);
        }

        internal void RemoveSession(DatagramServerSession session)
        {
            _aliveSessions.TryRemove(session.EndPoint, out _);
        }
    }

    internal sealed class DatagramServerSession : IServerSession
    {
        private readonly WeakReference<DatagramServer> _parent;

        public event EventHandler SessionClosed;
        public event EventHandler DataReceived;

        public EndPoint EndPoint { get; }
        public bool IsAlive { get; private set; }

        public IServer Parent
        {
            get
            {
                _parent.TryGetTarget(out var target);
                return target;
            }
        }

        public DatagramServerSession(DatagramServer parent, EndPoint endPoint)
        {
            _parent = new WeakReference<DatagramServer>(parent);
            EndPoint = endPoint;
            IsAlive = true;
        }

        public void Dispose()
        {
            SessionClosed?.Invoke(this, EventArgs.Empty);
            IsAlive = false;
            (Parent as DatagramServer)?.RemoveSession(this);
        }

        public int SendTo(byte[] buffer, int offset, int length)
        {
            var written =
                (Parent as DatagramServer)?.BindSocket.SendTo(buffer, offset, length, SocketFlags.None, EndPoint);
            if (written.HasValue)
                return written.Value;
            return 0;
        }

        public int ReceiveFrom(byte[] buffer, int offset, int length)
        {
            var remoteEP = EndPoint;
            var read = (Parent as DatagramServer)?.BindSocket.ReceiveFrom(buffer, offset, length, SocketFlags.None,
                ref remoteEP);
            if (remoteEP == EndPoint && read != null)
                return read.Value;
            return 0;
        }

        internal void OnReceived()
        {
            DataReceived?.Invoke(this, EventArgs.Empty);
        }
    }
}