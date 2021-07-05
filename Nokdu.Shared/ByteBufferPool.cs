using System.Collections.Concurrent;

namespace Nokdu
{
    static class ByteBufferPool
    {
        public const int MaximumReceiveBufferSize = 65536; //< 64KB

        private static readonly ConcurrentDictionary<int, ConcurrentQueue<byte[]>> byteBufferPool =
            new();

        public static byte[] GetByteBuffer(int bufferSize = MaximumReceiveBufferSize)
        {
            if (!byteBufferPool.TryGetValue(bufferSize, out var poolQueue))
                if (!byteBufferPool.TryAdd(bufferSize, poolQueue = new ConcurrentQueue<byte[]>()))
                    if (!byteBufferPool.TryGetValue(bufferSize, out poolQueue))
                        return null;

            while (poolQueue.TryDequeue(out var result))
                return result;

            return new byte [bufferSize];
        }

        public static void PutByteBuffer(byte[] buf)
        {
            var bufferSize = buf.Length;
            if (!byteBufferPool.TryGetValue(bufferSize, out var poolQueue))
                if (!byteBufferPool.TryAdd(bufferSize, poolQueue = new ConcurrentQueue<byte[]>()))
                    if (!byteBufferPool.TryGetValue(bufferSize, out poolQueue))
                        return;
            poolQueue.Enqueue(buf);
        }
    }
}