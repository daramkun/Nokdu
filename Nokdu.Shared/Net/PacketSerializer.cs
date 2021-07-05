using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace Nokdu.Net
{
    public class PacketSerializer<T> where T : Enum
    {
        private readonly Dictionary<T, Type> _packetTypes;
        private readonly Dictionary<Type, int> _validSizes;

        public PacketSerializer(IEnumerable<KeyValuePair<T, Type>> types)
        {
            _packetTypes = new Dictionary<T, Type>();
            _validSizes = new Dictionary<Type, int>();

            foreach (var (key, value) in types)
            {
                if (!value.IsValueType)
                    throw new ArgumentException();
                _packetTypes.Add(key, value);

                _validSizes.Add(value, Marshal.SizeOf(value));
            }
        }

        public unsafe T ReadPacketType([NotNull] Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var buffer = ByteBufferPool.GetByteBuffer(8);
            try
            {
                if (stream.Read(buffer, 0, 8) == 0)
                    throw new IOException();

                fixed (byte* bytesPtr = buffer)
                {
                    var bytesIntPtr = new IntPtr(bytesPtr);
                    return (T) (object) Marshal.ReadInt64(bytesIntPtr);
                }
            }
            finally
            {
                ByteBufferPool.PutByteBuffer(buffer);
            }
        }

        public unsafe T ReadPacketType([NotNull] ISession session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            var buffer = ByteBufferPool.GetByteBuffer(8);
            try
            {
                if (session.ReceiveFrom(buffer, 0, 8) == 0)
                    throw new IOException();

                fixed (byte* bytesPtr = buffer)
                {
                    var bytesIntPtr = new IntPtr(bytesPtr);
                    return (T) (object) Marshal.ReadInt64(bytesIntPtr);
                }
            }
            finally
            {
                ByteBufferPool.PutByteBuffer(buffer);
            }
        }

        public unsafe bool ReadPacket<TPacket>([NotNull] Stream stream, T type, out TPacket packet)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (_packetTypes[type] != typeof(TPacket))
                throw new ArgumentException();

            int packetSize;
            var sizeBuffer = ByteBufferPool.GetByteBuffer(4);
            if (stream.Read(sizeBuffer, 0, 4) != 4)
                throw new IOException();
            try
            {
                fixed (byte* sizeBufferPtr = sizeBuffer)
                {
                    var sizeBufferIntPtr = new IntPtr(sizeBufferPtr);
                    packetSize = Marshal.ReadInt32(sizeBufferIntPtr);
                }
            }
            finally
            {
                ByteBufferPool.PutByteBuffer(sizeBuffer);
            }

            if (packetSize != _validSizes[_packetTypes[type]])
            {
                packet = default;
                return false;
            }

            var buffer = ByteBufferPool.GetByteBuffer(packetSize);
            if (stream.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new IOException();
            try
            {
                fixed (byte* bufferPtr = buffer)
                {
                    var bufferIntPtr = new IntPtr(bufferPtr);
                    packet = Marshal.PtrToStructure<TPacket>(bufferIntPtr);
                }
            }
            finally
            {
                ByteBufferPool.PutByteBuffer(buffer);
            }

            return true;
        }

        public unsafe bool ReadPacket<TPacket>([NotNull] ISession session, T type,
            out TPacket packet)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            if (_packetTypes[type] != typeof(TPacket))
                throw new ArgumentException();

            int packetSize;
            var sizeBuffer = ByteBufferPool.GetByteBuffer(4);
            if (session.ReceiveFrom(sizeBuffer, 0, 4) != 4)
                throw new IOException();
            try
            {
                fixed (byte* sizeBufferPtr = sizeBuffer)
                {
                    var sizeBufferIntPtr = new IntPtr(sizeBufferPtr);
                    packetSize = Marshal.ReadInt32(sizeBufferIntPtr);
                }
            }
            finally
            {
                ByteBufferPool.PutByteBuffer(sizeBuffer);
            }

            if (packetSize != _validSizes[_packetTypes[type]])
            {
                packet = default;
                return false;
            }

            var buffer = ByteBufferPool.GetByteBuffer(packetSize);
            if (session.ReceiveFrom(buffer, 0, buffer.Length) != buffer.Length)
                throw new IOException();
            try
            {
                fixed (byte* bufferPtr = buffer)
                {
                    var bufferIntPtr = new IntPtr(bufferPtr);
                    packet = Marshal.PtrToStructure<TPacket>(bufferIntPtr);
                }
            }
            finally
            {
                ByteBufferPool.PutByteBuffer(buffer);
            }

            return true;
        }

        public unsafe void WritePacket<TPacket>([NotNull] Stream stream, T type, TPacket packet)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (_packetTypes[type] != packet.GetType())
                throw new ArgumentException();

            var buffer = ByteBufferPool.GetByteBuffer(8 + 4 + _validSizes[packet.GetType()]);
            try
            {
                fixed (byte* bufferPtr = buffer)
                {
                    var bufferIntPtr = new IntPtr(bufferPtr);
                    Marshal.WriteInt64(bufferIntPtr, (long) (object) type);
                    Marshal.WriteInt32(bufferIntPtr + 8, _validSizes[packet.GetType()]);
                    Marshal.StructureToPtr(packet, bufferIntPtr + 8 + 4, false);
                }

                stream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                ByteBufferPool.PutByteBuffer(buffer);
            }
        }

        public unsafe void WritePacket<TPacket>([NotNull] ISession session, T type,
            TPacket packet)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            if (_packetTypes[type] != packet.GetType())
                throw new ArgumentException();

            var buffer = ByteBufferPool.GetByteBuffer(8 + 4 + _validSizes[packet.GetType()]);
            try
            {
                fixed (byte* bufferPtr = buffer)
                {
                    var bufferIntPtr = new IntPtr(bufferPtr);
                    Marshal.WriteInt64(bufferIntPtr, (long) (object) type);
                    Marshal.WriteInt32(bufferIntPtr + 8, _validSizes[packet.GetType()]);
                    Marshal.StructureToPtr(packet, bufferIntPtr + 8 + 4, false);
                }

                session.SendTo(buffer, 0, buffer.Length);
            }
            finally
            {
                ByteBufferPool.PutByteBuffer(buffer);
            }
        }
    }
}