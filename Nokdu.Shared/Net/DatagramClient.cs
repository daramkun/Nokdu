using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Nokdu.Net
{
    public class DatagramClient : IClient
    {
        private static readonly byte[] ZeroSizeBytes = new byte[0];

        private Socket _clientSocket;

        public event EventHandler SessionClosed;
        public event EventHandler DataReceived;

        public EndPoint EndPoint { get; }
        public bool IsAlive { get; private set; }
        public bool IsConnectionClient { get; } = false;

        public DatagramClient(EndPoint endPoint)
        {
            _clientSocket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Dispose()
        {
            IsAlive = false;
            _clientSocket.Dispose();
            _clientSocket = null;
        }

        public int SendTo(byte[] buffer, int offset, int length)
        {
            return _clientSocket.SendTo(buffer, offset, length, SocketFlags.None, EndPoint);
        }

        public int ReceiveFrom(byte[] buffer, int offset, int length)
        {
            var remoteEP = EndPoint;
            var read = _clientSocket.ReceiveFrom(buffer, offset, length, SocketFlags.None, ref remoteEP);
            return read;
        }

        public void Start()
        {
            IsAlive = true;
            StartReceiveFrom();
        }

        public async Task StartAsync()
        {
            await Task.Run(Start);
        }

        public void Stop()
        {
            IsAlive = false;
        }

        private void StartReceiveFrom()
        {
            var remoteEndPoint = EndPoint;
            _clientSocket.BeginReceiveFrom(ZeroSizeBytes, 0, 0, SocketFlags.None, ref remoteEndPoint,
                (ar) =>
                {
                    if (!IsAlive)
                        return;

                    try
                    {
                        var readLength = _clientSocket.EndReceiveFrom(ar, ref remoteEndPoint);
                        if (readLength == 0)
                        {
                            IsAlive = false;
                            return;
                        }

                        DataReceived?.Invoke(this, EventArgs.Empty);

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
    }
}