// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-08-03 02:08:22
// # Recently: 2025-08-03 02:08:22
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using Event = Astraia.KcpClient.Event;

namespace Astraia
{
    public static class Writer<T>
    {
        public static Action<MemoryWriter, T> writer;
    }

    public static class Reader<T>
    {
        public static Func<MemoryReader, T> reader;
    }
}

namespace Astraia
{
    public class MemoryWriter : IDisposable
    {
        public byte[] buffer = new byte[1500];
        public int position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write<T>(T value) where T : unmanaged
        {
            Resize(position + sizeof(T));
            fixed (byte* ptr = &buffer[position])
            {
                *(T*)ptr = value;
            }

            position += sizeof(T);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNullable<T>(T? value) where T : unmanaged
        {
            if (!value.HasValue)
            {
                Write((byte)0);
                return;
            }

            Write((byte)1);
            Write(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke<T>(T value)
        {
            Writer<T>.writer?.Invoke(this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryWriter Pop()
        {
            var writer = HeapManager.Dequeue<MemoryWriter>();
            writer.Reset();
            return writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(MemoryWriter writer)
        {
            HeapManager.Enqueue(writer);
        }

        public override string ToString()
        {
            return BitConverter.ToString(buffer, 0, position);
        }

        void IDisposable.Dispose()
        {
            HeapManager.Enqueue(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int length)
        {
            if (buffer.Length < length)
            {
                Array.Resize(ref buffer, Math.Max(length, buffer.Length * 2));
            }
        }

        public void WriteBytes(byte[] segment, int offset, int count)
        {
            Resize(position + count);
            Buffer.BlockCopy(segment, offset, buffer, position, count);
            position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ArraySegment<byte>(MemoryWriter writer)
        {
            return new ArraySegment<byte>(writer.buffer, 0, writer.position);
        }
    }
}

namespace Astraia
{
    public class MemoryReader : IDisposable
    {
        public ArraySegment<byte> buffer;
        public int position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T Read<T>() where T : unmanaged
        {
            T value;
            fixed (byte* ptr = &buffer.Array[buffer.Offset + position])
            {
                value = *(T*)ptr;
            }

            position += sizeof(T);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ReadNullable<T>() where T : unmanaged
        {
            return Read<byte>() != 0 ? Read<T>() : default(T?);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Invoke<T>()
        {
            return Reader<T>.reader != null ? Reader<T>.reader.Invoke(this) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(ArraySegment<byte> segment)
        {
            buffer = segment;
            position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryReader Pop(ArraySegment<byte> segment)
        {
            var reader = HeapManager.Dequeue<MemoryReader>();
            reader.Reset(segment);
            return reader;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(MemoryReader reader)
        {
            HeapManager.Enqueue(reader);
        }

        public override string ToString()
        {
            return BitConverter.ToString(buffer.Array, buffer.Offset, buffer.Count);
        }

        void IDisposable.Dispose()
        {
            HeapManager.Enqueue(this);
        }

        public byte[] ReadBytes(byte[] bytes, int count)
        {
            if (buffer.Count - position < count)
            {
                throw new OverflowException("读取器剩余容量不够!");
            }

            Buffer.BlockCopy(buffer.Array, buffer.Offset + position, bytes, 0, count);
            position += count;
            return bytes;
        }

        public ArraySegment<byte> ReadArraySegment(int count)
        {
            if (buffer.Count - position < count)
            {
                throw new OverflowException("读取器剩余容量不够!");
            }

            var segment = new ArraySegment<byte>(buffer.Array, buffer.Offset + position, count);
            position += count;
            return segment;
        }
    }
}

namespace Astraia
{
    internal static class Compress
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Invoke(ulong value)
        {
            if (value == 0)
            {
                return 1;
            }

            var count = 0;
            while (value > 0)
            {
                count++;
                value >>= 7;
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeUInt64(MemoryWriter writer, ulong value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong DecodeUInt64(MemoryReader reader)
        {
            var shift = 0;
            var value = 0UL;
            while (true)
            {
                var bit = reader.Read<byte>();
                value |= (ulong)(bit & 0x7F) << shift;
                if ((bit & 0x80) == 0)
                {
                    break;
                }

                shift += 7;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ZigZagEncode(long n)
        {
            return (ulong)((n << 1) ^ (n >> 63));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ZigZagDecode(ulong n)
        {
            return (long)((n >> 1) ^ (ulong)-(long)(n & 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeUInt32(MemoryWriter writer, uint value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DecodeUInt32(MemoryReader reader)
        {
            var shift = 0;
            var value = 0U;
            while (true)
            {
                var bit = reader.Read<byte>();
                value |= (uint)(bit & 0x7F) << shift;
                if ((bit & 0x80) == 0)
                {
                    break;
                }

                shift += 7;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ZigZagEncode(int n)
        {
            return (uint)((n << 1) ^ (n >> 31));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ZigZagDecode(uint n)
        {
            return (int)((n >> 1) ^ -(int)(n & 1));
        }
    }
}

namespace Astraia
{
    internal class WriterQueue
    {
        private readonly Queue<MemoryWriter> writers = new Queue<MemoryWriter>();
        private readonly uint maxLength;
        private MemoryWriter writer;

        public WriterQueue(uint maxLength)
        {
            this.maxLength = maxLength;
        }

        public void AddMessage(ArraySegment<byte> segment)
        {
            var length = Compress.Invoke((ulong)segment.Count);
            if (writer != null && writer.position + length + segment.Count > maxLength)
            {
                writers.Enqueue(writer);
                writer = null;
            }

            writer ??= MemoryWriter.Pop();
            Compress.EncodeUInt64(writer, (ulong)segment.Count);
            writer.WriteBytes(segment.Array, segment.Offset, segment.Count);
        }

        public bool GetPacket(MemoryWriter result)
        {
            if (writers.Count > 0)
            {
                var target = writers.Dequeue();
                if (result.position != 0)
                {
                    throw new ArgumentException("拷贝目标不是空的！");
                }

                ArraySegment<byte> segment = target;
                result.WriteBytes(segment.Array, segment.Offset, segment.Count);
                MemoryWriter.Push(target);
                return true;
            }

            if (writer != null)
            {
                if (result.position != 0)
                {
                    throw new ArgumentException("拷贝目标不是空的！");
                }

                ArraySegment<byte> segment = writer;
                result.WriteBytes(segment.Array, segment.Offset, segment.Count);
                MemoryWriter.Push(writer);
                writer = null;
                return true;
            }

            return false;
        }
    }
}

namespace Astraia
{
    internal class ReaderQueue
    {
        private readonly Queue<MemoryWriter> writers = new Queue<MemoryWriter>();
        private readonly MemoryReader reader = new MemoryReader();
        public int Count => writers.Count;

        public bool AddPacket(ArraySegment<byte> segment)
        {
            if (segment.Count < sizeof(ushort))
            {
                return false;
            }

            var writer = MemoryWriter.Pop();
            writer.WriteBytes(segment.Array, segment.Offset, segment.Count);
            if (writers.Count == 0)
            {
                reader.Reset(writer);
            }

            writers.Enqueue(writer);
            return true;
        }

        public bool GetMessage(out ArraySegment<byte> segment)
        {
            segment = default;
            if (writers.Count == 0)
            {
                return false;
            }

            if (reader.buffer.Count == 0)
            {
                return false;
            }

            if (reader.buffer.Count - reader.position == 0)
            {
                var writer = writers.Dequeue();
                MemoryWriter.Push(writer);
                if (writers.Count > 0)
                {
                    reader.Reset(writers.Peek());
                }
                else
                {
                    return false;
                }
            }

            if (reader.buffer.Count - reader.position == 0)
            {
                return false;
            }

            var length = (int)Compress.DecodeUInt64(reader);

            if (reader.buffer.Count - reader.position < length)
            {
                return false;
            }

            segment = reader.ReadArraySegment(length);
            return true;
        }
    }
}

namespace Astraia
{
    internal static class Channel
    {
        public const byte Reliable = 1 << 0;
        public const byte Unreliable = 1 << 1;
    }

    internal static class Common
    {
        public static void Encode(byte[] p, int offset, uint value)
        {
            p[0 + offset] = (byte)(value >> 0);
            p[1 + offset] = (byte)(value >> 8);
            p[2 + offset] = (byte)(value >> 16);
            p[3 + offset] = (byte)(value >> 24);
        }

        public static uint Decode(byte[] p, int offset)
        {
            uint result = 0;
            result |= p[0 + offset];
            result |= (uint)(p[1 + offset] << 8);
            result |= (uint)(p[2 + offset] << 16);
            result |= (uint)(p[3 + offset] << 24);
            return result;
        }

        public static void Blocked(Socket socket, int buffer = 1024 * 1024 * 7)
        {
            socket.Blocking = false;
            var sendBuffer = socket.SendBufferSize;
            var receiveBuffer = socket.ReceiveBufferSize;
            try
            {
                socket.SendBufferSize = buffer;
                socket.ReceiveBufferSize = buffer;
            }
            catch (SocketException)
            {
                Log.Info("发送缓冲: {0} => {1} : {2:F}", buffer, sendBuffer, sendBuffer / buffer);
                Log.Info("接收缓冲: {0} => {1} : {2:F}", buffer, receiveBuffer, receiveBuffer / buffer);
            }
        }
    }

    internal enum Error : byte
    {
        解析失败 = 1,
        连接超时 = 2,
        网络拥塞 = 3,
        无效接收 = 4,
        无效发送 = 5,
        连接关闭 = 6,
        未知异常 = 7
    }

    internal enum Lobby : byte
    {
        身份验证成功 = 1,
        请求进入大厅 = 2,
        进入大厅成功 = 3,
        请求创建房间 = 4,
        创建房间成功 = 5,
        请求加入房间 = 6,
        加入房间成功 = 7,
        请求离开房间 = 8,
        离开房间成功 = 9,
        请求移除玩家 = 10,
        断开玩家连接 = 11,
        更新房间数据 = 12,
        同步网络数据 = 13,
    }

    internal enum State : byte
    {
        Connect = 0,
        Connected = 1,
        Disconnect = 2
    }

    internal enum Opcode : byte
    {
        Connect = 1,
        Ping = 2,
        Data = 3,
        Disconnect = 4
    }

    internal readonly struct Setting
    {
        public readonly uint MaxUnit;
        public readonly uint Timeout;
        public readonly uint Interval;
        public readonly uint DeadLink;
        public readonly uint FastResend;
        public readonly uint SendWindow;
        public readonly uint ReceiveWindow;
        public readonly bool NoDelay;
        public readonly bool DualMode;
        public readonly bool Congestion;

        public Setting(
            uint MaxUnit = Kcp.IKCP_MTU_DEF,
            uint Timeout = 10000,
            uint Interval = 10,
            uint DeadLink = Kcp.IKCP_DEADLINK,
            uint FastResend = 0,
            uint SendWindow = Kcp.IKCP_WND_SND,
            uint ReceiveWindow = Kcp.IKCP_WND_RCV,
            bool NoDelay = true,
            bool DualMode = true,
            bool Congestion = false)
        {
            this.MaxUnit = MaxUnit;
            this.Timeout = Timeout;
            this.Interval = Interval;
            this.DeadLink = DeadLink;
            this.FastResend = FastResend;
            this.SendWindow = SendWindow;
            this.ReceiveWindow = ReceiveWindow;
            this.NoDelay = NoDelay;
            this.DualMode = DualMode;
            this.Congestion = Congestion;
        }
    }
}

namespace Astraia
{
    internal sealed unsafe class Protocol : IDisposable
    {
        private byte[] buffer;
        private IKCPCB* kcp;
        private Action<byte[], int> output;
        public uint State => kcp->state;
        public uint Count => kcp->nrcv_buf + kcp->nrcv_que + kcp->nsnd_buf + kcp->nsnd_que;
        public uint Death => kcp->dead_link;

        public Protocol(uint conv, Action<byte[], int> output)
        {
            this.output = output;
            kcp = Kcp.ikcp_create(conv, ref buffer);
        }

        public int Input(byte[] buffer, int offset, int length)
        {
            fixed (byte* ptr = &buffer[offset])
            {
                return Kcp.ikcp_input(kcp, ptr, length);
            }
        }

        public int Receive(byte[] buffer, int length)
        {
            fixed (byte* ptr = buffer)
            {
                return Kcp.ikcp_recv(kcp, ptr, length);
            }
        }

        public int Send(byte[] buffer, int offset, int length)
        {
            fixed (byte* ptr = &buffer[offset])
            {
                return Kcp.ikcp_send(kcp, ptr, length);
            }
        }

        public void Flush()
        {
            fixed (byte* ptr = buffer)
            {
                Kcp.ikcp_flush(kcp, ptr, buffer, output);
            }
        }

        public void Update(uint current)
        {
            fixed (byte* ptr = buffer)
            {
                Kcp.ikcp_update(kcp, current, ptr, buffer, output);
            }
        }

        public int PeekSize()
        {
            return Kcp.ikcp_peeksize(kcp);
        }

        public void Clear()
        {
            Kcp.iqueue_del_init(&kcp->snd_queue);
        }

        public void SetData(uint mtu, uint deadLink)
        {
            kcp->dead_link = deadLink;
            Kcp.ikcp_setmtu(kcp, (int)mtu, ref buffer);
        }

        public void SetDelay(bool noDelay, uint interval, uint resend, bool nc)
        {
            Kcp.ikcp_nodelay(kcp, noDelay ? 1 : 0, (int)interval, (int)resend, nc ? 1 : 0);
        }

        public void SetWindow(uint send, uint receive)
        {
            Kcp.ikcp_wndsize(kcp, (int)send, (int)receive);
        }

        private int dispose;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref dispose, 1, 0) != 0)
            {
                return;
            }

            Kcp.ikcp_release(kcp);
            kcp = null;
            output = null;
            buffer = null;
            GC.SuppressFinalize(this);
        }

        ~Protocol() => Dispose();
    }
}

namespace Astraia
{
    internal sealed class KcpPeer
    {
        private const int PING_INTERVAL = 1000;
        private const int METADATA_SIZE = sizeof(byte) + sizeof(int);
        private readonly byte[] rawSendBuffer;
        private readonly byte[] kcpSendBuffer;
        private readonly byte[] kcpDataBuffer;
        private readonly uint kcpLength;
        private readonly uint udpLength;
        private readonly string userName;
        private readonly Stopwatch watch = new Stopwatch();
        private readonly Event onEvent;
        private uint overTime;
        private uint pingTime;
        private uint prevTime;
        private uint userData;
        private Protocol kcp;
        private State state;
        private uint Time => (uint)watch.ElapsedMilliseconds;

        public KcpPeer(Setting setting, Event onEvent, string userName, uint userData = 0)
        {
            Rebuild(setting);
            this.onEvent = onEvent;
            this.userName = userName;
            this.userData = userData;
            udpLength = UdpLength(setting.MaxUnit);
            kcpLength = KcpLength(setting.MaxUnit, setting.ReceiveWindow);
            kcpDataBuffer = new byte[1 + kcpLength];
            kcpSendBuffer = new byte[1 + kcpLength];
            rawSendBuffer = new byte[setting.MaxUnit];
        }

        public void Rebuild(Setting setting)
        {
            userData = 0;
            pingTime = 0;
            prevTime = 0;
            overTime = setting.Timeout;
            kcp = new Protocol(0, SendReliable);
            kcp.SetData(setting.MaxUnit - METADATA_SIZE, setting.DeadLink);
            kcp.SetDelay(setting.NoDelay, setting.Interval, setting.FastResend, !setting.Congestion);
            kcp.SetWindow(setting.SendWindow, setting.ReceiveWindow);
            state = State.Connect;
            watch.Restart();
        }

        private void SendReliable(byte[] bytes, int count)
        {
            rawSendBuffer[0] = Channel.Reliable;
            Common.Encode(rawSendBuffer, 1, userData);
            Buffer.BlockCopy(bytes, 0, rawSendBuffer, 1 + 4, count);
            onEvent.Send(new ArraySegment<byte>(rawSendBuffer, 0, count + 1 + 4));
        }

        public static uint KcpLength(uint count, uint window)
        {
            return (count - Kcp.IKCP_OVERHEAD - METADATA_SIZE) * (Math.Min(window, 255) - 1) - 1;
        }

        public static uint UdpLength(uint count)
        {
            return count - METADATA_SIZE;
        }

        public void Handshake()
        {
            SendReliable(Opcode.Connect, new ArraySegment<byte>(BitConverter.GetBytes(userData)));
        }

        private bool TryReceive(out Opcode message, out ArraySegment<byte> segment)
        {
            segment = default;
            message = Opcode.Disconnect;
            var count = kcp.PeekSize();
            if (count <= 0)
            {
                return false;
            }

            if (count > kcpDataBuffer.Length)
            {
                onEvent.Error(Error.无效接收, "{0}接收网络消息过大。消息大小: {1} < {2}。".Format(userName, kcpDataBuffer.Length, count));
                Disconnect();
                return false;
            }

            if (kcp.Receive(kcpDataBuffer, count) < 0)
            {
                onEvent.Error(Error.无效接收, "{0}接收网络消息失败。".Format(userName));
                Disconnect();
                return false;
            }

            message = (Opcode)kcpDataBuffer[0];
            segment = new ArraySegment<byte>(kcpDataBuffer, 1, count - 1);
            prevTime = Time;
            return true;
        }

        public void Input(ArraySegment<byte> segment)
        {
            if (segment.Count <= 1 + 4)
            {
                return;
            }

            var channel = segment.Array[segment.Offset];
            var newData = Common.Decode(segment.Array, segment.Offset + 1);
            if (state == State.Connected && newData != userData)
            {
                Log.Warn("{0}数据校验失败。旧: {1} 新: {2}", userName, userData, newData);
                return;
            }

            var message = new ArraySegment<byte>(segment.Array, segment.Offset + 1 + 4, segment.Count - 1 - 4);
            if (channel == Channel.Reliable)
            {
                if (kcp.Input(message.Array, message.Offset, message.Count) != 0)
                {
                    Log.Warn("{0}发送可靠消息失败。消息大小: {1}", userName, message.Count - 1);
                }
            }
            else if (channel == Channel.Unreliable)
            {
                if (state == State.Connected)
                {
                    onEvent.Receive(message, Channel.Unreliable);
                    prevTime = Time;
                }
            }
        }

        private void SendReliable(Opcode message, ArraySegment<byte> segment = default)
        {
            if (segment.Count + 1 > kcpSendBuffer.Length)
            {
                onEvent.Error(Error.无效发送, "{0}发送网络消息过大。消息大小: {1} < {2}".Format(userName, segment.Count, kcpLength));
                return;
            }

            kcpSendBuffer[0] = (byte)message;
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array, segment.Offset, kcpSendBuffer, 1, segment.Count);
            }

            if (kcp.Send(kcpSendBuffer, 0, segment.Count + 1) < 0)
            {
                onEvent.Error(Error.无效发送, "{0}发送网络消息失败。消息大小: {1}。".Format(userName, segment.Count));
            }
        }

        private void SendUnreliable(ArraySegment<byte> segment)
        {
            if (segment.Count > udpLength)
            {
                Log.Error("{0}发送不可靠消息失败。消息大小: {1}", userName, segment.Count);
                return;
            }

            rawSendBuffer[0] = Channel.Unreliable;
            Common.Encode(rawSendBuffer, 1, userData);
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array, segment.Offset, rawSendBuffer, 1 + 4, segment.Count);
            }

            onEvent.Send(new ArraySegment<byte>(rawSendBuffer, 0, segment.Count + 1 + 4));
        }

        public void SendData(ArraySegment<byte> segment, int channel)
        {
            if (segment.Count == 0)
            {
                onEvent.Error(Error.无效发送, "{0}尝试发送空消息。".Format(userName));
                Disconnect();
                return;
            }

            switch (channel)
            {
                case Channel.Reliable:
                    SendReliable(Opcode.Data, segment);
                    break;
                case Channel.Unreliable:
                    SendUnreliable(segment);
                    break;
            }
        }

        public void Disconnect()
        {
            if (state == State.Disconnect) return;
            try
            {
                SendReliable(Opcode.Disconnect);
                kcp.Flush();
            }
            finally
            {
                state = State.Disconnect;
                onEvent.Disconnect();
            }
        }

        private void BeforeReceive(uint sinceTime)
        {
            if (sinceTime >= prevTime + overTime)
            {
                onEvent.Error(Error.连接超时, "{0}在{1}秒内没有收到任何消息后的连接超时！".Format(userName, overTime / 1000));
                Disconnect();
            }

            if (kcp.State == unchecked((uint)-1))
            {
                onEvent.Error(Error.连接超时, "{0}网络消息被重传了{1}次而没有得到确认！".Format(userName, kcp.Death));
                Disconnect();
            }

            if (sinceTime >= pingTime + PING_INTERVAL)
            {
                SendReliable(Opcode.Ping);
                pingTime = sinceTime;
            }

            if (kcp.Count >= 10000)
            {
                onEvent.Error(Error.网络拥塞, "{0}断开连接，因为它处理数据的速度不够快！".Format(userName));
                kcp.Clear();
                Disconnect();
            }
        }

        private void UpdateConnect()
        {
            BeforeReceive(Time);
            if (TryReceive(out var message, out var segment))
            {
                switch (message)
                {
                    case Opcode.Connect when segment.Count != 4:
                        onEvent.Error(Error.无效接收, "{0}接收无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        return;
                    case Opcode.Connect:
                        state = State.Connected;
                        userData = Common.Decode(segment.Array, segment.Offset);
                        onEvent.Connect();
                        break;
                    case Opcode.Data:
                        onEvent.Error(Error.无效接收, "{0}接收无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        break;
                    case Opcode.Disconnect:
                        Disconnect();
                        break;
                }
            }
        }

        private void UpdateConnected()
        {
            BeforeReceive(Time);
            while (TryReceive(out var message, out var segment))
            {
                switch (message)
                {
                    case Opcode.Connect:
                        onEvent.Error(Error.无效接收, "{0}接收无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        break;
                    case Opcode.Data when segment.Count == 0:
                        onEvent.Error(Error.无效接收, "{0}收到无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        break;
                    case Opcode.Data:
                        onEvent.Receive(segment, Channel.Reliable);
                        break;
                    case Opcode.Disconnect:
                        Disconnect();
                        break;
                }
            }
        }

        public void EarlyUpdate()
        {
            try
            {
                switch (state)
                {
                    case State.Connect:
                        UpdateConnect();
                        break;
                    case State.Connected:
                        UpdateConnected();
                        break;
                }
            }
            catch (SocketException e)
            {
                onEvent.Error(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                onEvent.Error(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (Exception e)
            {
                onEvent.Error(Error.未知异常, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
        }

        public void AfterUpdate()
        {
            try
            {
                if (state != State.Disconnect)
                {
                    kcp.Update(Time);
                }
            }
            catch (SocketException e)
            {
                onEvent.Error(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                onEvent.Error(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (Exception e)
            {
                onEvent.Error(Error.未知异常, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
        }
    }
}

namespace Astraia
{
    internal sealed class KcpServer
    {
        public class Event
        {
            public Action<int> Connect;
            public Action<int> Disconnect;
            public Action<int, Error, string> Error;
            public Action<int, ArraySegment<byte>> Send;
            public Action<int, ArraySegment<byte>, int> Receive;
        }

        private readonly Dictionary<int, KcpClient> clients = new Dictionary<int, KcpClient>();
        private readonly HashSet<int> removes = new HashSet<int>();
        private readonly byte[] buffer;
        private readonly Event onEvent;
        private readonly Setting setting;
        private Socket socket;
        private EndPoint endPoint;

        public KcpServer(Setting setting, Event onEvent)
        {
            this.setting = setting;
            this.onEvent = onEvent;
            buffer = new byte[setting.MaxUnit];
            endPoint = setting.DualMode ? new IPEndPoint(IPAddress.IPv6Any, 0) : new IPEndPoint(IPAddress.Any, 0);
        }

        public void Connect(ushort port)
        {
            if (socket != null)
            {
                Log.Warn("服务器已经连接!");
                return;
            }

            if (setting.DualMode)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                try
                {
                    socket.DualMode = true;
                }
                catch (NotSupportedException e)
                {
                    Log.Warn("服务器不支持双连接模式!\n{0}", e);
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    const uint IOC_IN = 0x80000000U;
                    const uint IOC_VENDOR = 0x18000000U;
                    const int SIO_UDP_RESET = unchecked((int)(IOC_IN | IOC_VENDOR | 12));
                    socket.IOControl(SIO_UDP_RESET, new byte[] { 0x00 }, null);
                }

                socket.Bind(new IPEndPoint(IPAddress.IPv6Any, port));
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
            }

            Common.Blocked(socket);
        }

        public void Send(int id, ArraySegment<byte> segment, int channel)
        {
            if (clients.TryGetValue(id, out var client))
            {
                client.kcpPeer.SendData(segment, channel);
            }
        }

        private bool TryReceive(out int id, out ArraySegment<byte> segment)
        {
            id = 0;
            segment = default;
            try
            {
                if (socket != null && socket.Poll(0, SelectMode.SelectRead))
                {
                    var count = socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint);
                    segment = new ArraySegment<byte>(buffer, 0, count);
                    id = endPoint.GetHashCode();
                    return true;
                }

                return false;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Log.Info("服务器发送消息失败!\n{0}", e);
                }

                return false;
            }
        }

        public void Disconnect(int id)
        {
            if (clients.TryGetValue(id, out var client))
            {
                client.kcpPeer.Disconnect();
            }
        }

        private KcpClient Register(int id)
        {
            var newEvent = new Astraia.KcpClient.Event();
            var client = new KcpClient(new KcpPeer(setting, newEvent, "服务器"), endPoint);
            newEvent.Connect = OnConnect;
            newEvent.Disconnect = OnDisconnect;
            newEvent.Error = OnError;
            newEvent.Receive = OnReceive;
            newEvent.Send = OnSend;
            return client;

            void OnConnect()
            {
                Log.Info("客户端 {0} 连接到服务器。", id);
                clients.Add(id, client);
                client.kcpPeer.Handshake();
                onEvent.Connect.Invoke(id);
            }

            void OnDisconnect()
            {
                Log.Info("客户端 {0} 从服务器断开。", id);
                removes.Add(id);
                onEvent.Disconnect.Invoke(id);
            }

            void OnError(Error error, string reason)
            {
                onEvent.Error?.Invoke(id, error, reason);
            }

            void OnReceive(ArraySegment<byte> message, int channel)
            {
                onEvent.Receive.Invoke(id, message, channel);
            }

            void OnSend(ArraySegment<byte> segment)
            {
                try
                {
                    if (clients.TryGetValue(id, out var result))
                    {
                        if (socket.Poll(0, SelectMode.SelectWrite))
                        {
                            socket.SendTo(segment.Array, segment.Offset, segment.Count, SocketFlags.None, result.endPoint);
                        }
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                    {
                        Log.Error("服务器接收消息失败!\n{0}", e);
                    }
                }
            }
        }

        public void EarlyUpdate()
        {
            while (TryReceive(out var id, out var segment))
            {
                if (!clients.TryGetValue(id, out var client))
                {
                    client = Register(id);
                    client.kcpPeer.Input(segment);
                    client.kcpPeer.EarlyUpdate();
                }
                else
                {
                    client.kcpPeer.Input(segment);
                }
            }

            foreach (var client in clients.Values)
            {
                client.kcpPeer.EarlyUpdate();
            }

            foreach (var client in removes)
            {
                clients.Remove(client);
            }

            removes.Clear();
        }

        public void AfterUpdate()
        {
            foreach (var client in clients.Values)
            {
                client.kcpPeer.AfterUpdate();
            }
        }

        public void StopServer()
        {
            clients.Clear();
            socket?.Close();
            socket = null;
        }

        private class KcpClient
        {
            public readonly KcpPeer kcpPeer;
            public readonly EndPoint endPoint;

            public KcpClient(KcpPeer kcpPeer, EndPoint endPoint)
            {
                this.kcpPeer = kcpPeer;
                this.endPoint = endPoint;
            }
        }
    }
}

namespace Astraia
{
    internal sealed class KcpClient
    {
        public class Event
        {
            public Action Connect;
            public Action Disconnect;
            public Action<Error, string> Error;
            public Action<ArraySegment<byte>> Send;
            public Action<ArraySegment<byte>, int> Receive;
        }

        private readonly byte[] buffer;
        private readonly Event onEvent;
        private readonly Setting setting;
        private State state;
        private Socket socket;
        private KcpPeer kcpPeer;
        private EndPoint endPoint;

        public KcpClient(Setting setting, Event onEvent)
        {
            this.setting = setting;
            this.onEvent = onEvent;
            buffer = new byte[setting.MaxUnit];
            state = State.Disconnect;
        }

        public void Connect(string address, ushort port)
        {
            try
            {
                if (state != State.Disconnect)
                {
                    Log.Warn("客户端已经连接!");
                    return;
                }

                var addresses = Dns.GetHostAddresses(address);
                if (addresses.Length >= 1)
                {
                    Register(setting);
                    state = State.Connect;
                    endPoint = new IPEndPoint(addresses[0], port);
                    socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    Common.Blocked(socket);
                    socket.Connect(endPoint);
                    Log.Info("客户端连接到: {0} : {1}", addresses[0], port);
                    kcpPeer.Handshake();
                }
            }
            catch (SocketException e)
            {
                onEvent.Error(Error.解析失败, "无法解析主机地址: {0}\n{1}".Format(address, e));
                onEvent.Disconnect();
            }
        }

        public void Send(ArraySegment<byte> segment, int channel)
        {
            if (state != State.Disconnect)
            {
                kcpPeer.SendData(segment, channel);
            }
        }

        private bool TryReceive(out ArraySegment<byte> segment)
        {
            segment = default;
            try
            {
                if (socket != null && socket.Poll(0, SelectMode.SelectRead))
                {
                    var count = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    segment = new ArraySegment<byte>(buffer, 0, count);
                    return true;
                }

                return false;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Log.Info("客户端接收消息失败!\n{0}", e);
                    kcpPeer.Disconnect();
                }

                return false;
            }
        }

        public void Disconnect()
        {
            if (state != State.Disconnect)
            {
                kcpPeer.Disconnect();
            }
        }

        private void Register(Setting setting)
        {
            if (kcpPeer == null)
            {
                var newEvent = new Event();
                kcpPeer = new KcpPeer(setting, newEvent, "客户端");
                newEvent.Connect = OnConnect;
                newEvent.Disconnect = OnDisconnect;
                newEvent.Error = OnError;
                newEvent.Receive = OnReceive;
                newEvent.Send = OnSend;
            }
            else
            {
                kcpPeer.Rebuild(setting);
            }
        }

        private void OnConnect()
        {
            Log.Info("客户端连接成功。");
            state = State.Connected;
            onEvent.Connect.Invoke();
        }

        private void OnDisconnect()
        {
            Log.Info("客户端断开连接。");
            state = State.Disconnect;
            socket.Close();
            socket = null;
            endPoint = null;
            onEvent.Disconnect.Invoke();
        }

        private void OnError(Error error, string message)
        {
            onEvent.Error?.Invoke(error, message);
        }

        private void OnReceive(ArraySegment<byte> segment, int channel)
        {
            onEvent.Receive.Invoke(segment, channel);
        }

        private void OnSend(ArraySegment<byte> segment)
        {
            try
            {
                if (socket != null)
                {
                    if (socket.Poll(0, SelectMode.SelectWrite))
                    {
                        socket.Send(segment.Array, segment.Offset, segment.Count, SocketFlags.None);
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Log.Info("客户端发送消息失败!\n{0}", e);
                }
            }
        }

        public void EarlyUpdate()
        {
            if (state != State.Disconnect)
            {
                while (TryReceive(out var segment))
                {
                    kcpPeer.Input(segment);
                }

                kcpPeer.EarlyUpdate();
            }
        }

        public void AfterUpdate()
        {
            if (state != State.Disconnect)
            {
                kcpPeer.AfterUpdate();
            }
        }
    }
}