using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
#if !NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using unixsock;
using winsock;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace NativeSockets
{
    public static unsafe class SocketPal
    {
        public static readonly ushort ADDRESS_FAMILY_INTER_NETWORK_V6;
        private static readonly delegate* managed<SocketError> _GetLastSocketError;
        private static readonly delegate* managed<SocketError> _Initialize;
        private static readonly delegate* managed<SocketError> _Cleanup;
        private static readonly delegate* managed<bool, nint> _Create;
        private static readonly delegate* managed<nint, SocketError> _Close;
        private static readonly delegate* managed<nint, bool, SocketError> _SetDualMode6;
        private static readonly delegate* managed<nint, sockaddr_in*, SocketError> _Bind4;
        private static readonly delegate* managed<nint, sockaddr_in6*, SocketError> _Bind6;
        private static readonly delegate* managed<nint, sockaddr_in*, SocketError> _Connect4;
        private static readonly delegate* managed<nint, sockaddr_in6*, SocketError> _Connect6;
        private static readonly delegate* managed<nint, SocketOptionLevel, SocketOptionName, int*, int, SocketError> _SetOption;
        private static readonly delegate* managed<nint, SocketOptionLevel, SocketOptionName, int*, int*, SocketError> _GetOption;
        private static readonly delegate* managed<nint, bool, SocketError> _SetBlocking;
        private static readonly delegate* managed<nint, int, SelectMode, out bool, SocketError> _Poll;
        private static readonly delegate* managed<nint, void*, int, sockaddr_in*, int> _SendTo4;
        private static readonly delegate* managed<nint, void*, int, sockaddr_in6*, int> _SendTo6;
        private static readonly delegate* managed<nint, void*, int, sockaddr_in*, int> _ReceiveFrom4;
        private static readonly delegate* managed<nint, void*, int, sockaddr_in6*, int> _ReceiveFrom6;
        private static readonly delegate* managed<nint, sockaddr_in*, SocketError> _GetName4;
        private static readonly delegate* managed<nint, sockaddr_in6*, SocketError> _GetName6;
        private static readonly delegate* managed<sockaddr_in*, ReadOnlySpan<char>, SocketError> _SetIP4;
        private static readonly delegate* managed<sockaddr_in6*, ReadOnlySpan<char>, SocketError> _SetIP6;
        private static readonly delegate* managed<sockaddr_in*, Span<byte>, SocketError> _GetIP4;
        private static readonly delegate* managed<sockaddr_in6*, Span<byte>, SocketError> _GetIP6;
        private static readonly delegate* managed<sockaddr_in*, ReadOnlySpan<char>, SocketError> _SetHostName4;
        private static readonly delegate* managed<sockaddr_in6*, ReadOnlySpan<char>, SocketError> _SetHostName6;
        private static readonly delegate* managed<sockaddr_in*, Span<byte>, SocketError> _GetHostName4;
        private static readonly delegate* managed<sockaddr_in6*, Span<byte>, SocketError> _GetHostName6;

        static SocketPal()
        {
            bool isWindows =
#if NET5_0_OR_GREATER
                OperatingSystem.IsWindows();
#else
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
            if (isWindows)
            {
                ADDRESS_FAMILY_INTER_NETWORK_V6 = WinSock.ADDRESS_FAMILY_INTER_NETWORK_V6;
                _GetLastSocketError = &WinSock.GetLastSocketError;
                _Initialize = &WinSock.Initialize;
                _Cleanup = &WinSock.Cleanup;
                _Create = &WinSock.Create;
                _Close = &WinSock.Close;
                _SetDualMode6 = &WinSock.SetDualMode6;
                _Bind4 = &WinSock.Bind4;
                _Bind6 = &WinSock.Bind6;
                _Connect4 = &WinSock.Connect4;
                _Connect6 = &WinSock.Connect6;
                _SetOption = &WinSock.SetOption;
                _GetOption = &WinSock.GetOption;
                _SetBlocking = &WinSock.SetBlocking;
                _Poll = &WinSock.Poll;
                _SendTo4 = &WinSock.SendTo4;
                _SendTo6 = &WinSock.SendTo6;
                _ReceiveFrom4 = &WinSock.ReceiveFrom4;
                _ReceiveFrom6 = &WinSock.ReceiveFrom6;
                _GetName4 = &WinSock.GetName4;
                _GetName6 = &WinSock.GetName6;
                _SetIP4 = &WinSock.SetIP4;
                _SetIP6 = &WinSock.SetIP6;
                _GetIP4 = &WinSock.GetIP4;
                _GetIP6 = &WinSock.GetIP6;
                _SetHostName4 = &WinSock.SetHostName4;
                _SetHostName6 = &WinSock.SetHostName6;
                _GetHostName4 = &WinSock.GetHostName4;
                _GetHostName6 = &WinSock.GetHostName6;
            }
            else
            {
                ADDRESS_FAMILY_INTER_NETWORK_V6 = UnixSock.ADDRESS_FAMILY_INTER_NETWORK_V6;
                _GetLastSocketError = &UnixSock.GetLastSocketError;
                _Initialize = &UnixSock.Initialize;
                _Cleanup = &UnixSock.Cleanup;
                _Create = &UnixSock.Create;
                _Close = &UnixSock.Close;
                _SetDualMode6 = &UnixSock.SetDualMode6;
                _Bind4 = &UnixSock.Bind4;
                _Bind6 = &UnixSock.Bind6;
                _Connect4 = &UnixSock.Connect4;
                _Connect6 = &UnixSock.Connect6;
                _SetOption = &UnixSock.SetOption;
                _GetOption = &UnixSock.GetOption;
                _SetBlocking = &UnixSock.SetBlocking;
                _Poll = &UnixSock.Poll;
                _SendTo4 = &UnixSock.SendTo4;
                _SendTo6 = &UnixSock.SendTo6;
                _ReceiveFrom4 = &UnixSock.ReceiveFrom4;
                _ReceiveFrom6 = &UnixSock.ReceiveFrom6;
                _GetName4 = &UnixSock.GetName4;
                _GetName6 = &UnixSock.GetName6;
                _SetIP4 = &UnixSock.SetIP4;
                _SetIP6 = &UnixSock.SetIP6;
                _GetIP4 = &UnixSock.GetIP4;
                _GetIP6 = &UnixSock.GetIP6;
                _SetHostName4 = &UnixSock.SetHostName4;
                _SetHostName6 = &UnixSock.SetHostName6;
                _GetHostName4 = &UnixSock.GetHostName4;
                _GetHostName6 = &UnixSock.GetHostName6;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetLastSocketError() => _GetLastSocketError();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Initialize() => _Initialize();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Cleanup() => _Cleanup();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Create(bool ipv6) => _Create(ipv6);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Close(nint socket) => _Close(socket);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetDualMode6(nint socket, bool dualMode) => _SetDualMode6(socket, dualMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind4(nint socket, sockaddr_in* socketAddress) => _Bind4(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind6(nint socket, sockaddr_in6* socketAddress) => _Bind6(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect4(nint socket, sockaddr_in* socketAddress) => _Connect4(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect6(nint socket, sockaddr_in6* socketAddress) => _Connect6(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetOption(nint socket, SocketOptionLevel level, SocketOptionName name, int* value, int length = sizeof(int)) => _SetOption(socket, level, name, value, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetOption(nint socket, SocketOptionLevel level, SocketOptionName name, int* value, int* length = null) => _GetOption(socket, level, name, value, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetBlocking(nint socket, bool blocking) => _SetBlocking(socket, blocking);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Poll(nint socket, int microseconds, SelectMode mode, out bool status) => _Poll(socket, microseconds, mode, out status);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo4(nint socket, void* buffer, int length, sockaddr_in* socketAddress) => _SendTo4(socket, buffer, length, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo6(nint socket, void* buffer, int length, sockaddr_in6* socketAddress) => _SendTo6(socket, buffer, length, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom4(nint socket, void* buffer, int length, sockaddr_in* socketAddress) => _ReceiveFrom4(socket, buffer, length, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom6(nint socket, void* buffer, int length, sockaddr_in6* socketAddress) => _ReceiveFrom6(socket, buffer, length, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName4(nint socket, sockaddr_in* socketAddress) => _GetName4(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName6(nint socket, sockaddr_in6* socketAddress) => _GetName6(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP4(sockaddr_in* pAddrBuf, ReadOnlySpan<char> ip) => _SetIP4(pAddrBuf, ip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP6(sockaddr_in6* pAddrBuf, ReadOnlySpan<char> ip) => _SetIP6(pAddrBuf, ip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP4(sockaddr_in* pAddrBuf, Span<byte> ip) => _GetIP4(pAddrBuf, ip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP6(sockaddr_in6* pAddrBuf, Span<byte> ip) => _GetIP6(pAddrBuf, ip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName4(sockaddr_in* pAddrBuf, ReadOnlySpan<char> hostName) => _SetHostName4(pAddrBuf, hostName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName6(sockaddr_in6* pAddrBuf, ReadOnlySpan<char> hostName) => _SetHostName6(pAddrBuf, hostName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName4(sockaddr_in* socketAddress, Span<byte> hostName) => _GetHostName4(socketAddress, hostName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName6(sockaddr_in6* socketAddress, Span<byte> hostName) => _GetHostName6(socketAddress, hostName);
    }
}