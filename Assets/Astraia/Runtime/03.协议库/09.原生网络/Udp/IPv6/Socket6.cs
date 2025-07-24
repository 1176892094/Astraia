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
    public struct Socket6 : IDisposable, IEquatable<Socket6>
    {
        [FieldOffset(0)] public nint Handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Socket6 Create()
        {
            UdpPal6.Initialize();
            Socket6 socket = UdpPal6.Create();
            if (socket.Handle == -1)
                UdpPal6.Cleanup();
            return socket;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            UdpPal6.Close(ref this);
            UdpPal6.Cleanup();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SocketError SetDualMode(bool dualMode) => UdpPal6.SetDualMode(this, dualMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetDualMode(out bool dualMode)
        {
            int optionValue = 0;
            bool error = GetOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, ref optionValue);
            dualMode = optionValue == 0;
            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SocketError Bind(in SocketAddress6 address) => UdpPal6.Bind(this, ref Unsafe.AsRef(in address));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SocketError Connect(in SocketAddress6 address) => UdpPal6.Connect(this, ref Unsafe.AsRef(in address));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetOption(SocketOptionLevel level, SocketOptionName name, ref int value) => UdpPal6.SetOption(this, level, name, ref value) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetOption(SocketOptionLevel level, SocketOptionName name, ref int value) => UdpPal6.GetOption(this, level, name, ref value) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetNonBlocking(bool nonBlocking) => UdpPal6.SetNonBlocking(this, nonBlocking) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Poll(int microseconds, SelectMode mode) => UdpPal6.Poll(this, microseconds, mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Send(ReadOnlySpan<byte> buffer) => UdpPal6.Send(this, ref MemoryMarshal.GetReference(buffer), buffer.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Receive(Span<byte> buffer) => UdpPal6.Receive(this, ref MemoryMarshal.GetReference(buffer), buffer.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SendTo(ReadOnlySpan<byte> buffer, in SocketAddress6 address) => UdpPal6.SendTo(this, ref MemoryMarshal.GetReference(buffer), buffer.Length, ref Unsafe.AsRef(in address));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReceiveFrom(Span<byte> buffer, ref SocketAddress6 address) => UdpPal6.ReceiveFrom(this, ref MemoryMarshal.GetReference(buffer), buffer.Length, ref address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetAddress(ref SocketAddress6 address) => UdpPal6.GetAddress(this, ref address) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetSendBufferSize(out int sendBufferSize)
        {
            sendBufferSize = 0;
            return UdpPal6.GetOption(this, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, ref sendBufferSize) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetSendBufferSize(int sendBufferSize) => UdpPal6.SetOption(this, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, ref sendBufferSize) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetReceiveBufferSize(out int receiveBufferSize)
        {
            receiveBufferSize = 0;
            return UdpPal6.GetOption(this, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ref receiveBufferSize) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetReceiveBufferSize(int receiveBufferSize) => UdpPal6.SetOption(this, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ref receiveBufferSize) == 0;

        public bool Equals(Socket6 other) => Handle == other.Handle;
        public override bool Equals(object? obj) => obj is Socket6 socket && Equals(socket);
        public override int GetHashCode() => Handle.GetHashCode();

        public override string ToString() => $"Socket[{Handle}]";

        public static bool operator ==(Socket6 left, Socket6 right) => left.Equals(right);
        public static bool operator !=(Socket6 left, Socket6 right) => !(left == right);

        public static implicit operator bool(Socket6 socket) => socket.Handle > 0;
        public static implicit operator nint(Socket6 socket) => socket.Handle;
        public static implicit operator Socket6(nint handle) => Unsafe.As<nint, Socket6>(ref handle);
        public static implicit operator Socket6(Socket socket) => new Socket6 { Handle = (int)socket.Handle };
    }
}