using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591
#pragma warning disable SYSLIB1054
#pragma warning disable CA1401
#pragma warning disable CA2101

// ReSharper disable ALL

namespace NativeSockets.Udp
{
    public static unsafe class MnUdpPal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Initialize() => SocketPal.Initialize();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cleanup() => SocketPal.Cleanup();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MnSocket Create(bool ipv6)
        {
            MnSocket socket;
            socket.Handle = (int)SocketPal.Create(ipv6);
            socket.IsIPv6 = ipv6;
            return socket;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Close(ref MnSocket socket)
        {
            SocketPal.Close(socket);
            socket.Handle = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetDualMode(MnSocket socket, bool dualMode) => SocketPal.SetDualMode6(socket, dualMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind(MnSocket socket, ref MnSocketAddress socketAddress)
        {
            if (socket.IsIPv6)
                return UdpPal6.Bind(new Socket6 { Handle = socket }, ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref socketAddress));

            if (Unsafe.IsNullRef(ref socketAddress))
                return UdpPal4.Bind(new Socket4 { Handle = socket }, ref Unsafe.NullRef<SocketAddress4>());

            if (socketAddress.IsIPv6)
                return SocketError.AddressFamilyNotSupported;

            return UdpPal4.Bind(new Socket4 { Handle = socket }, ref Unsafe.As<uint, SocketAddress4>(ref socketAddress.Address));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect(MnSocket socket, ref MnSocketAddress socketAddress)
        {
            if (socket.IsIPv6)
                return UdpPal6.Connect(new Socket6 { Handle = socket }, ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref socketAddress));

            if (socketAddress.IsIPv6)
                return SocketError.AddressFamilyNotSupported;

            return UdpPal4.Connect(new Socket4 { Handle = socket }, ref Unsafe.As<uint, SocketAddress4>(ref socketAddress.Address));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetOption(MnSocket socket, SocketOptionLevel level, SocketOptionName name, ref int value)
        {
            SocketError error;
            fixed (int* pinnedBuffer = &value)
            {
                error = SocketPal.SetOption(socket, level, name, pinnedBuffer);
            }

            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetOption(MnSocket socket, SocketOptionLevel level, SocketOptionName name, ref int value)
        {
            SocketError error;
            fixed (int* pinnedBuffer = &value)
            {
                error = SocketPal.GetOption(socket, level, name, pinnedBuffer);
            }

            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetNonBlocking(MnSocket socket, bool nonBlocking)
        {
            SocketError error = SocketPal.SetBlocking(socket, !nonBlocking);
            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Poll(MnSocket socket, int microseconds, SelectMode mode)
        {
            SocketError error = SocketPal.Poll(socket, microseconds, mode, out bool status);
            return error == SocketError.Success && status;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Send(MnSocket socket, ref byte buffer, int length)
        {
            if (socket.IsIPv6)
                return UdpPal6.Send(new Socket6 { Handle = socket }, ref buffer, length);

            return UdpPal4.Send(new Socket4 { Handle = socket }, ref buffer, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Receive(MnSocket socket, ref byte buffer, int length)
        {
            if (socket.IsIPv6)
                return UdpPal6.Receive(new Socket6 { Handle = socket }, ref buffer, length);

            return UdpPal4.Receive(new Socket4 { Handle = socket }, ref buffer, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo(MnSocket socket, ref byte buffer, int length, ref MnSocketAddress socketAddress)
        {
            if (socket.IsIPv6)
                return UdpPal6.SendTo(new Socket6 { Handle = socket }, ref buffer, length, ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref socketAddress));

            if (socketAddress.IsIPv6)
                return -1;

            return UdpPal4.SendTo(new Socket4 { Handle = socket }, ref buffer, length, ref Unsafe.As<uint, SocketAddress4>(ref socketAddress.Address));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom(MnSocket socket, ref byte buffer, int length, ref MnSocketAddress socketAddress)
        {
            if (socket.IsIPv6)
                return UdpPal6.ReceiveFrom(new Socket6 { Handle = socket }, ref buffer, length, ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref socketAddress));

            ref byte reference = ref Unsafe.As<MnSocketAddress, byte>(ref socketAddress);
            Unsafe.InitBlockUnaligned(ref reference, 0, 8);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref reference, (nint)8), -0x10000);

            return UdpPal4.ReceiveFrom(new Socket4 { Handle = socket }, ref buffer, length, ref Unsafe.As<byte, SocketAddress4>(ref Unsafe.AddByteOffset(ref reference, (nint)12)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetAddress(MnSocket socket, ref MnSocketAddress socketAddress)
        {
            return UdpPal6.GetAddress(new Socket6 { Handle = socket }, ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref socketAddress));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP(ref MnSocketAddress socketAddress, ReadOnlySpan<char> ip)
        {
            return UdpPal6.SetIP(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref socketAddress), ip);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP(ref MnSocketAddress socketAddress, Span<byte> ip, bool ipv6)
        {
            return UdpPal6.GetIP(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref socketAddress), ip);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName(ref MnSocketAddress socketAddress, ReadOnlySpan<char> hostName)
        {
            return UdpPal6.SetHostName(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref socketAddress), hostName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName(ref MnSocketAddress socketAddress, Span<byte> hostName)
        {
            return UdpPal6.GetHostName(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref socketAddress), hostName);
        }
    }
}