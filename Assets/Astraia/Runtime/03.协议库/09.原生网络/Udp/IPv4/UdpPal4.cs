using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using winsock;

#pragma warning disable CS1591
#pragma warning disable SYSLIB1054
#pragma warning disable CA1401
#pragma warning disable CA2101

// ReSharper disable ALL

namespace NativeSockets.Udp
{
    public static unsafe class UdpPal4
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Initialize() => SocketPal.Initialize();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Cleanup() => SocketPal.Cleanup();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Socket4 Create() => SocketPal.Create(false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Close(ref Socket4 socket)
        {
            SocketPal.Close(socket);
            socket = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind(Socket4 socket, ref SocketAddress4 socketAddress)
        {
            if (Unsafe.IsNullRef(ref socketAddress))
                return SocketPal.Bind4(socket, (sockaddr_in*)null);

            sockaddr_in __socketAddress_native;
            __socketAddress_native.sin_family = (ushort)AddressFamily.InterNetwork;
            __socketAddress_native.sin_port = socketAddress.Port;
            Unsafe.WriteUnaligned(&__socketAddress_native.sin_addr, socketAddress.Address);
            Unsafe.InitBlockUnaligned(__socketAddress_native.sin_zero, 0, 8);

            return SocketPal.Bind4(socket, &__socketAddress_native);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect(Socket4 socket, ref SocketAddress4 socketAddress)
        {
            sockaddr_in __socketAddress_native;
            __socketAddress_native.sin_family = (ushort)AddressFamily.InterNetwork;
            __socketAddress_native.sin_port = socketAddress.Port;
            Unsafe.WriteUnaligned(&__socketAddress_native.sin_addr, socketAddress.Address);
            Unsafe.InitBlockUnaligned(__socketAddress_native.sin_zero, 0, 8);

            return SocketPal.Connect4(socket, &__socketAddress_native);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetOption(Socket4 socket, SocketOptionLevel level, SocketOptionName name, ref int value)
        {
            SocketError error;
            fixed (int* pinnedBuffer = &value)
            {
                error = SocketPal.SetOption(socket, level, name, pinnedBuffer);
            }

            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetOption(Socket4 socket, SocketOptionLevel level, SocketOptionName name, ref int value)
        {
            SocketError error;
            fixed (int* pinnedBuffer = &value)
            {
                error = SocketPal.GetOption(socket, level, name, pinnedBuffer);
            }

            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetNonBlocking(Socket4 socket, bool nonBlocking)
        {
            SocketError error = SocketPal.SetBlocking(socket, !nonBlocking);
            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Poll(Socket4 socket, int microseconds, SelectMode mode)
        {
            SocketError error = SocketPal.Poll(socket, microseconds, mode, out bool status);
            return error == SocketError.Success && status;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Send(Socket4 socket, ref byte buffer, int length)
        {
            int num;
            fixed (byte* pinnedBuffer = &buffer)
            {
                num = SocketPal.SendTo4(socket, pinnedBuffer, length, (sockaddr_in*)null);
            }

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Receive(Socket4 socket, ref byte buffer, int length)
        {
            int result;
            fixed (byte* pinnedBuffer = &buffer)
            {
                result = SocketPal.ReceiveFrom4(socket, pinnedBuffer, length, (sockaddr_in*)null);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo(Socket4 socket, ref byte buffer, int length, ref SocketAddress4 socketAddress)
        {
            sockaddr_in __socketAddress_native;
            __socketAddress_native.sin_family = (ushort)AddressFamily.InterNetwork;
            __socketAddress_native.sin_port = socketAddress.Port;
            Unsafe.WriteUnaligned(&__socketAddress_native.sin_addr, socketAddress.Address);
            Unsafe.InitBlockUnaligned(__socketAddress_native.sin_zero, 0, 8);

            int num;
            fixed (byte* pinnedBuffer = &buffer)
            {
                num = SocketPal.SendTo4(socket, pinnedBuffer, length, &__socketAddress_native);
            }

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom(Socket4 socket, ref byte buffer, int length, ref SocketAddress4 socketAddress)
        {
            sockaddr_in __socketAddress_native;
            int result;
            fixed (byte* pinnedBuffer = &buffer)
            {
                result = SocketPal.ReceiveFrom4(socket, pinnedBuffer, length, &__socketAddress_native);
            }

            if (result <= 0)
                return result;

            socketAddress.Address = Unsafe.ReadUnaligned<uint>(&__socketAddress_native.sin_addr);
            socketAddress.Port = __socketAddress_native.sin_port;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetAddress(Socket4 socket, ref SocketAddress4 socketAddress)
        {
            sockaddr_in __socketAddress_native;
            SocketError error = SocketPal.GetName4(socket, &__socketAddress_native);
            if (error == 0)
            {
                socketAddress.Address = Unsafe.ReadUnaligned<uint>(&__socketAddress_native.sin_addr);
                socketAddress.Port = __socketAddress_native.sin_port;
            }

            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP(ref SocketAddress4 socketAddress, ReadOnlySpan<char> ip)
        {
            sockaddr_in __socketAddress_native;
            SocketError error = SocketPal.SetIP4(&__socketAddress_native, ip);
            if (error == 0)
                socketAddress.Address = Unsafe.ReadUnaligned<uint>(&__socketAddress_native.sin_addr);

            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP(ref SocketAddress4 socketAddress, Span<byte> ip)
        {
            sockaddr_in __socketAddress_native;
            __socketAddress_native.sin_family = (ushort)AddressFamily.InterNetwork;
            __socketAddress_native.sin_port = socketAddress.Port;
            Unsafe.WriteUnaligned(&__socketAddress_native.sin_addr, socketAddress.Address);
            Unsafe.InitBlockUnaligned(__socketAddress_native.sin_zero, 0, 8);

            SocketError error = SocketPal.GetIP4(&__socketAddress_native, ip);

            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName(ref SocketAddress4 socketAddress, ReadOnlySpan<char> hostName)
        {
            sockaddr_in __socketAddress_native;
            SocketError error = SocketPal.SetHostName4(&__socketAddress_native, hostName);
            if (error == 0)
                socketAddress.Address = Unsafe.ReadUnaligned<uint>(&__socketAddress_native.sin_addr);

            return error;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName(ref SocketAddress4 socketAddress, Span<byte> hostName)
        {
            sockaddr_in __socketAddress_native;
            __socketAddress_native.sin_family = (ushort)AddressFamily.InterNetwork;
            __socketAddress_native.sin_port = socketAddress.Port;
            Unsafe.WriteUnaligned(&__socketAddress_native.sin_addr, socketAddress.Address);
            Unsafe.InitBlockUnaligned(__socketAddress_native.sin_zero, 0, 8);

            SocketError error = SocketPal.GetHostName4(&__socketAddress_native, hostName);

            return error;
        }
    }
}