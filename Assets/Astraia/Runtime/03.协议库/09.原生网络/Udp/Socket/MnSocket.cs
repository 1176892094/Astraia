using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8632

// ReSharper disable ALL

namespace NativeSockets.Udp
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct MnSocket : IDisposable, IEquatable<MnSocket>
    {
        [FieldOffset(0)] public int Handle;
        public bool IsIPv4 => !IsIPv6;
        [FieldOffset(4)] public bool IsIPv6;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MnSocket Create(bool ipv6)
        {
            MnUdpPal.Initialize();
            MnSocket socket = MnUdpPal.Create(ipv6);
            if (socket.Handle == -1)
                MnUdpPal.Cleanup();
            return socket;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            MnUdpPal.Close(ref this);
            MnUdpPal.Cleanup();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SocketError SetDualMode(bool dualMode) => MnUdpPal.SetDualMode(this, dualMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetDualMode(out bool dualMode)
        {
            int optionValue = 0;
            bool error = GetOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, ref optionValue);
            dualMode = optionValue == 0;
            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SocketError Bind(in MnSocketAddress address) => MnUdpPal.Bind(this, ref Unsafe.AsRef(in address));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SocketError Connect(in MnSocketAddress address) => MnUdpPal.Connect(this, ref Unsafe.AsRef(in address));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetOption(SocketOptionLevel level, SocketOptionName name, ref int value) => MnUdpPal.SetOption(this, level, name, ref value) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetOption(SocketOptionLevel level, SocketOptionName name, ref int value) => MnUdpPal.GetOption(this, level, name, ref value) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetNonBlocking(bool nonBlocking) => MnUdpPal.SetNonBlocking(this, nonBlocking) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Poll(int microseconds, SelectMode mode) => MnUdpPal.Poll(this, microseconds, mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Send(ReadOnlySpan<byte> buffer) => MnUdpPal.Send(this, ref MemoryMarshal.GetReference(buffer), buffer.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Receive(Span<byte> buffer) => MnUdpPal.Receive(this, ref MemoryMarshal.GetReference(buffer), buffer.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SendTo(ReadOnlySpan<byte> buffer, in MnSocketAddress address) => MnUdpPal.SendTo(this, ref MemoryMarshal.GetReference(buffer), buffer.Length, ref Unsafe.AsRef(in address));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReceiveFrom(Span<byte> buffer, ref MnSocketAddress address) => MnUdpPal.ReceiveFrom(this, ref MemoryMarshal.GetReference(buffer), buffer.Length, ref address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetAddress(ref MnSocketAddress address) => MnUdpPal.GetAddress(this, ref address) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetSendBufferSize(out int sendBufferSize)
        {
            sendBufferSize = 0;
            return MnUdpPal.GetOption(this, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, ref sendBufferSize) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetSendBufferSize(int sendBufferSize) => MnUdpPal.SetOption(this, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, ref sendBufferSize) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetReceiveBufferSize(out int receiveBufferSize)
        {
            receiveBufferSize = 0;
            return MnUdpPal.GetOption(this, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ref receiveBufferSize) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetReceiveBufferSize(int receiveBufferSize) => MnUdpPal.SetOption(this, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ref receiveBufferSize) == 0;

        public bool Equals(MnSocket other) => Handle == other.Handle;
        public override bool Equals(object? obj) => obj is MnSocket socket && Equals(socket);
        public override int GetHashCode() => Handle.GetHashCode();

        public override string ToString() => $"Socket[{Handle}]";

        public static bool operator ==(MnSocket left, MnSocket right) => left.Equals(right);
        public static bool operator !=(MnSocket left, MnSocket right) => !(left == right);

        public static implicit operator bool(MnSocket socket) => socket.Handle > 0;
        public static implicit operator nint(MnSocket socket) => socket.Handle;
        public static implicit operator MnSocket(Socket socket) => new MnSocket { Handle = (int)socket.Handle, IsIPv6 = socket.AddressFamily == AddressFamily.InterNetworkV6 };
    }
}