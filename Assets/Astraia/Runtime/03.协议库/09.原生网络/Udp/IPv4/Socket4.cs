using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeSockets.Udp
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Socket4 : IDisposable, IEquatable<Socket4>
    {
        [FieldOffset(0)] public nint Handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Socket4 Create()
        {
            UdpPal4.Initialize();
            Socket4 socket = UdpPal4.Create();
            if (socket.Handle == -1)
                UdpPal4.Cleanup();
            return socket;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            UdpPal4.Close(ref this);
            UdpPal4.Cleanup();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SocketError Bind(in SocketAddress4 address) => UdpPal4.Bind(this, ref Unsafe.AsRef(in address));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SocketError Connect(in SocketAddress4 address) => UdpPal4.Connect(this, ref Unsafe.AsRef(in address));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetOption(SocketOptionLevel level, SocketOptionName name, ref int value) => UdpPal4.SetOption(this, level, name, ref value) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetOption(SocketOptionLevel level, SocketOptionName name, ref int value) => UdpPal4.GetOption(this, level, name, ref value) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetNonBlocking(bool nonBlocking) => UdpPal4.SetNonBlocking(this, nonBlocking) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Poll(int microseconds, SelectMode mode) => UdpPal4.Poll(this, microseconds, mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Send(ReadOnlySpan<byte> buffer) => UdpPal4.Send(this, ref MemoryMarshal.GetReference(buffer), buffer.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Receive(Span<byte> buffer) => UdpPal4.Receive(this, ref MemoryMarshal.GetReference(buffer), buffer.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SendTo(ReadOnlySpan<byte> buffer, in SocketAddress4 address) => UdpPal4.SendTo(this, ref MemoryMarshal.GetReference(buffer), buffer.Length, ref Unsafe.AsRef(in address));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReceiveFrom(Span<byte> buffer, ref SocketAddress4 address) => UdpPal4.ReceiveFrom(this, ref MemoryMarshal.GetReference(buffer), buffer.Length, ref address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetAddress(ref SocketAddress4 address) => UdpPal4.GetAddress(this, ref address) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetSendBufferSize(out int sendBufferSize)
        {
            sendBufferSize = 0;
            return UdpPal4.GetOption(this, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, ref sendBufferSize) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetSendBufferSize(int sendBufferSize) => UdpPal4.SetOption(this, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, ref sendBufferSize) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetReceiveBufferSize(out int receiveBufferSize)
        {
            receiveBufferSize = 0;
            return UdpPal4.GetOption(this, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ref receiveBufferSize) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetReceiveBufferSize(int receiveBufferSize) => UdpPal4.SetOption(this, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ref receiveBufferSize) == 0;

        public bool Equals(Socket4 other) => Handle == other.Handle;
        public override bool Equals(object? obj) => obj is Socket4 socket && Equals(socket);
        public override int GetHashCode() => Handle.GetHashCode();

        public override string ToString() => $"Socket[{Handle}]";

        public static bool operator ==(Socket4 left, Socket4 right) => left.Equals(right);
        public static bool operator !=(Socket4 left, Socket4 right) => !(left == right);

        public static implicit operator bool(Socket4 socket) => socket.Handle > 0;
        public static implicit operator nint(Socket4 socket) => socket.Handle;
        public static implicit operator Socket4(nint handle) => Unsafe.As<nint, Socket4>(ref handle);
        public static implicit operator Socket4(Socket socket) => new Socket4 { Handle = (int)socket.Handle };
    }
}