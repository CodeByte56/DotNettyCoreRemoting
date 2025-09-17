using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNettyCoreRemoting
{
    public static class IByteBufferByteEx
    {
        public static byte[] ByteBufferTobyte(this IByteBuffer buffer)
        {
            int length = buffer.ReadableBytes;
            byte[] data = new byte[length];
            buffer.ReadBytes(data); // 从当前 readerIndex 开始读取
            return data;
        }
    }
}
