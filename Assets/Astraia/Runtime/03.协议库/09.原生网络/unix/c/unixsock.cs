using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using winsock;

#pragma warning disable CS1591
#pragma warning disable CS8981
#pragma warning disable SYSLIB1054

// ReSharper disable ALL

namespace unixsock
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class UnixSock
    {
        public const ushort ADDRESS_FAMILY_INTER_NETWORK_V6 = 10;

        static UnixSock()
        {
            bool isUnix;
            try
            {
                _ = UnixPal.getpid();
                isUnix = true;
                goto label;
            }
            catch
            {
                //
            }

            try
            {
                _ = iOSPal.getpid();
                isUnix = false;
            }
            catch
            {
                _Initialized = 0;
                return;
            }

            label:
            if (isUnix)
            {
                _bind = &UnixPal.bind;
                _getsockname = &UnixPal.getsockname;
                _socket = &UnixPal.socket;
                _fcntl = &UnixPal.fcntl;
                _setsockopt = &UnixPal.setsockopt;
                _getsockopt = &UnixPal.getsockopt;
                _connect = &UnixPal.connect;
                _close = &UnixPal.close;
                _sendto = &UnixPal.sendto;
                _recvfrom = &UnixPal.recvfrom;
                _select = &UnixPal.select;
                _inet_pton = &UnixPal.inet_pton;
                _getaddrinfo = &UnixPal.getaddrinfo;
                _freeaddrinfo = &UnixPal.freeaddrinfo;
                _inet_ntop = &UnixPal.inet_ntop;
                _getnameinfo = &UnixPal.getnameinfo;
            }
            else
            {
                _bind = &iOSPal.bind;
                _getsockname = &iOSPal.getsockname;
                _socket = &iOSPal.socket;
                _fcntl = &iOSPal.fcntl;
                _setsockopt = &iOSPal.setsockopt;
                _getsockopt = &iOSPal.getsockopt;
                _connect = &iOSPal.connect;
                _close = &iOSPal.close;
                _sendto = &iOSPal.sendto;
                _recvfrom = &iOSPal.recvfrom;
                _select = &iOSPal.select;
                _inet_pton = &iOSPal.inet_pton;
                _getaddrinfo = &iOSPal.getaddrinfo;
                _freeaddrinfo = &iOSPal.freeaddrinfo;
                _inet_ntop = &iOSPal.inet_ntop;
                _getnameinfo = &iOSPal.getnameinfo;
            }

            _Initialized = 1;
        }

#if !NET6_0_OR_GREATER
        private static ReadOnlySpan<(int errno, SocketError error)> SocketErrors => new[]
        {
            (0, SocketError.Success),
            (1, SocketError.AccessDenied),
            (2, SocketError.AddressNotAvailable),
            (4, SocketError.Interrupted),
            (6, SocketError.HostNotFound),
            (7, SocketError.MessageSize),
            (9, SocketError.OperationAborted),
            (11, SocketError.WouldBlock),
            (12, SocketError.NoBufferSpaceAvailable),
            (13, SocketError.AccessDenied),
            (14, SocketError.Fault),
            (20, SocketError.InvalidArgument),
            (22, SocketError.InvalidArgument),
            (23, SocketError.TooManyOpenSockets),
            (24, SocketError.TooManyOpenSockets),
            (28, SocketError.NoBufferSpaceAvailable),
            (32, SocketError.Shutdown),
            (36, SocketError.InvalidArgument),
            (40, SocketError.AccessDenied),
            (59, SocketError.TooManyOpenSockets),
            (61, SocketError.NoData),
            (63, SocketError.NoBufferSpaceAvailable),
            (67, SocketError.NetworkUnreachable),
            (70, SocketError.ConnectionReset),
            (72, SocketError.NetworkUnreachable),
            (74, SocketError.InvalidArgument),
            (75, SocketError.MessageSize),
            (84, SocketError.InvalidArgument),
            (88, SocketError.NotSocket),
            (89, SocketError.DestinationAddressRequired),
            (90, SocketError.MessageSize),
            (91, SocketError.ProtocolType),
            (92, SocketError.ProtocolOption),
            (93, SocketError.ProtocolNotSupported),
            (94, SocketError.SocketNotSupported),
            (96, SocketError.ProtocolFamilyNotSupported),
            (97, SocketError.AddressFamilyNotSupported),
            (98, SocketError.AddressAlreadyInUse),
            (99, SocketError.AddressNotAvailable),
            (100, SocketError.NetworkDown),
            (101, SocketError.NetworkUnreachable),
            (102, SocketError.NetworkReset),
            (103, SocketError.ConnectionAborted),
            (104, SocketError.ConnectionReset),
            (105, SocketError.NoBufferSpaceAvailable),
            (106, SocketError.IsConnected),
            (107, SocketError.NotConnected),
            (108, SocketError.Disconnecting),
            (110, SocketError.TimedOut),
            (111, SocketError.ConnectionRefused),
            (112, SocketError.HostDown),
            (113, SocketError.HostUnreachable),
            (114, SocketError.AlreadyInProgress),
            (115, SocketError.InProgress)
        };
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetLastSocketError()
        {
#if NET6_0_OR_GREATER
            return (SocketError)Marshal.GetLastPInvokeError();
#else
            int errno = Marshal.GetLastWin32Error();
            int low = 0;
            int high = SocketErrors.Length - 1;
            ref (int errno, SocketError error) reference = ref MemoryMarshal.GetReference(SocketErrors);
            int index;
            while (low <= high)
            {
                int i = (int)(((uint)high + (uint)low) >> 1);
                int c = errno.CompareTo(Unsafe.Add(ref reference, i).errno);
                switch (c)
                {
                    case 0:
                        index = i;
                        goto label;
                    case > 0:
                        low = i + 1;
                        break;
                    default:
                        high = i - 1;
                        break;
                }
            }

            index = ~low;
            label:
            return index >= 0 ? SocketErrors[index].error : (SocketError)errno;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Initialize() => _Initialized == 1 ? SocketError.Success : SocketError.SocketError;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Cleanup() => SocketError.Success;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Create(bool ipv6)
        {
            int family = ipv6 ? ADDRESS_FAMILY_INTER_NETWORK_V6 : (int)AddressFamily.InterNetwork;
            nint _socket = socket(family, (int)SocketType.Dgram, 0);
            return _socket;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Close(nint socket)
        {
            SocketError errorCode = close(socket);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetDualMode6(nint socket, bool dualMode)
        {
            int optionValue = dualMode ? 0 : 1;
            SocketError errorCode = SetOption(socket, SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, &optionValue);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind4(nint socket, sockaddr_in* socketAddress)
        {
            sockaddr_in __socketAddress_native;
            if (socketAddress == null)
            {
                __socketAddress_native = new sockaddr_in();
                SetIP4(&__socketAddress_native, "0.0.0.0");
            }
            else
            {
                __socketAddress_native = *socketAddress;
                __socketAddress_native.sin_port = WinSock2.HOST_TO_NET_16(socketAddress->sin_port);
            }

            SocketError errorCode = bind(socket, (sockaddr*)&__socketAddress_native, sizeof(sockaddr_in));
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind6(nint socket, sockaddr_in6* socketAddress)
        {
            sockaddr_in6 __socketAddress_native;
            if (socketAddress == null)
            {
                __socketAddress_native = new sockaddr_in6();
                SetIP6(&__socketAddress_native, "::");
            }
            else
            {
                __socketAddress_native = *socketAddress;
                __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);
            }

            SocketError errorCode = bind(socket, (sockaddr*)&__socketAddress_native, sizeof(sockaddr_in6));
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect4(nint socket, sockaddr_in* socketAddress)
        {
            sockaddr_in __socketAddress_native = *socketAddress;
            __socketAddress_native.sin_port = WinSock2.HOST_TO_NET_16(socketAddress->sin_port);

            SocketError errorCode = connect(socket, (sockaddr*)&__socketAddress_native, sizeof(sockaddr_in));
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect6(nint socket, sockaddr_in6* socketAddress)
        {
            sockaddr_in6 __socketAddress_native = *socketAddress;
            __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);

            SocketError errorCode = connect(socket, (sockaddr*)&__socketAddress_native, sizeof(sockaddr_in6));
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetOption(nint socket, SocketOptionLevel optionLevel, SocketOptionName optionName, int* optionValue, int optionLength = sizeof(int))
        {
            SocketError errorCode = setsockopt(socket, optionLevel, optionName, optionValue, optionLength);
            return errorCode == SocketError.SocketError ? GetLastSocketError() : SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetOption(nint socket, SocketOptionLevel level, SocketOptionName optionName, int* optionValue, int* optionLength = null)
        {
            int num = sizeof(int);
            if (optionLength == null)
                optionLength = &num;

            SocketError errorCode = getsockopt(socket, (int)level, (int)optionName, (byte*)optionValue, optionLength);
            return errorCode == SocketError.SocketError ? GetLastSocketError() : SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetBlocking(nint socket, bool shouldBlock)
        {
            int flags = fcntl(socket, 3, 0);
            if (flags == -1)
                return GetLastSocketError();

            flags = shouldBlock ? flags & ~2048 : flags | 2048;
            if (fcntl(socket, 4, flags) == -1)
                return GetLastSocketError();

            return SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Poll(nint socket, int microseconds, SelectMode mode, out bool status)
        {
            nint* fileDescriptorSet = stackalloc nint[2] { 1, socket };
            TimeValue timeout = default;
            int socketCount;
            if (microseconds != -1)
            {
                MicrosecondsToTimeValue((uint)microseconds, ref timeout);
                socketCount = select(0, mode == SelectMode.SelectRead ? fileDescriptorSet : null, mode == SelectMode.SelectWrite ? fileDescriptorSet : null, mode == SelectMode.SelectError ? fileDescriptorSet : null, &timeout);
            }
            else
            {
                socketCount = select(0, mode == SelectMode.SelectRead ? fileDescriptorSet : null, mode == SelectMode.SelectWrite ? fileDescriptorSet : null, mode == SelectMode.SelectError ? fileDescriptorSet : null, null);
            }

            if ((SocketError)socketCount == SocketError.SocketError)
            {
                status = false;
                return GetLastSocketError();
            }

            status = (int)fileDescriptorSet[0] != 0 && fileDescriptorSet[1] == socket;
            return SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo4(nint socket, void* buffer, int length, sockaddr_in* socketAddress)
        {
            if (socketAddress != null)
            {
                sockaddr_in __socketAddress_native = *socketAddress;
                __socketAddress_native.sin_port = WinSock2.HOST_TO_NET_16(socketAddress->sin_port);
                return sendto(socket, (byte*)buffer, length, SocketFlags.None, (byte*)&__socketAddress_native, sizeof(sockaddr_in));
            }

            int num = sendto(socket, (byte*)buffer, length, SocketFlags.None, null, sizeof(sockaddr_in));
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo6(nint socket, void* buffer, int length, sockaddr_in6* socketAddress)
        {
            if (socketAddress != null)
            {
                sockaddr_in6 __socketAddress_native = *socketAddress;
                __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);
                return sendto(socket, (byte*)buffer, length, SocketFlags.None, (byte*)&__socketAddress_native, sizeof(sockaddr_in6));
            }

            int num = sendto(socket, (byte*)buffer, length, SocketFlags.None, null, sizeof(sockaddr_in6));
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom4(nint socket, void* buffer, int length, sockaddr_in* socketAddress)
        {
            sockaddr_storage addressStorage = new sockaddr_storage();
            int socketAddressSize = sizeof(sockaddr_storage);

            int num = recvfrom(socket, (byte*)buffer, length, SocketFlags.None, (byte*)&addressStorage, &socketAddressSize);

            if (num > 0 && socketAddress != null)
            {
                socketAddress->sin_family = (ushort)AddressFamily.InterNetwork;
                sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                *socketAddress = *__socketAddress_native;
                socketAddress->sin_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
            }

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom6(nint socket, void* buffer, int length, sockaddr_in6* socketAddress)
        {
            sockaddr_storage addressStorage = new sockaddr_storage();
            int socketAddressSize = sizeof(sockaddr_storage);

            int num = recvfrom(socket, (byte*)buffer, length, SocketFlags.None, (byte*)&addressStorage, &socketAddressSize);

            if (num > 0 && socketAddress != null)
            {
                if (addressStorage.ss_family == (int)AddressFamily.InterNetwork)
                {
                    socketAddress->sin6_family = ADDRESS_FAMILY_INTER_NETWORK_V6;
                    sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                    Unsafe.InitBlockUnaligned(socketAddress->sin6_addr, 0, 8);
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 8, -0x10000);
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr + 12, &__socketAddress_native->sin_addr, 4);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
                }
                else if (addressStorage.ss_family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                {
                    sockaddr_in6* __socketAddress_native = (sockaddr_in6*)&addressStorage;
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr, __socketAddress_native->sin6_addr, 20);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin6_port);
                }
            }

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName4(nint socket, sockaddr_in* socketAddress)
        {
            sockaddr_storage addressStorage = new sockaddr_storage();
            int socketAddressSize = sizeof(sockaddr_storage);
            SocketError errorCode = getsockname(socket, (sockaddr*)&addressStorage, &socketAddressSize);
            if (errorCode == SocketError.Success)
            {
                sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                *socketAddress = *__socketAddress_native;
                socketAddress->sin_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
            }

            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName6(nint socket, sockaddr_in6* socketAddress)
        {
            sockaddr_storage addressStorage = new sockaddr_storage();
            int socketAddressSize = sizeof(sockaddr_storage);
            SocketError errorCode = getsockname(socket, (sockaddr*)&addressStorage, &socketAddressSize);
            if (errorCode == SocketError.Success)
            {
                if (addressStorage.ss_family == (int)AddressFamily.InterNetwork)
                {
                    sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                    Unsafe.InitBlockUnaligned(socketAddress->sin6_addr, 0, 8);
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 8, -0x10000);
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr + 12, &__socketAddress_native->sin_addr, 4);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
                }
                else if (addressStorage.ss_family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                {
                    sockaddr_in6* __socketAddress_native = (sockaddr_in6*)&addressStorage;
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr, __socketAddress_native->sin6_addr, 20);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin6_port);
                }
            }

            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP4(sockaddr_in* socketAddress, ReadOnlySpan<char> ip)
        {
            void* pAddrBuf = &socketAddress->sin_addr;

            int byteCount = Encoding.ASCII.GetByteCount(ip);
            Span<byte> buffer = stackalloc byte[byteCount];
            Encoding.ASCII.GetBytes(ip, buffer);

            int addressFamily = (int)AddressFamily.InterNetwork;

            int error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), pAddrBuf);

            switch (error)
            {
                case 1:
                    return SocketError.Success;
                case 0:
                    return SocketError.InvalidArgument;
                default:
                    return SocketError.Fault;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP6(sockaddr_in6* socketAddress, ReadOnlySpan<char> ip)
        {
            void* pAddrBuf = socketAddress->sin6_addr;

            int byteCount = Encoding.ASCII.GetByteCount(ip);
            Span<byte> buffer = stackalloc byte[byteCount];
            Encoding.ASCII.GetBytes(ip, buffer);

            int addressFamily = (int)ADDRESS_FAMILY_INTER_NETWORK_V6;
            if (ip.IndexOf(':') < 0)
            {
                addressFamily = (int)AddressFamily.InterNetwork;
                Unsafe.InitBlockUnaligned(pAddrBuf, 0, 8);
                Unsafe.WriteUnaligned((byte*)pAddrBuf + 8, -0x10000);
                pAddrBuf = (byte*)pAddrBuf + 12;
            }

            int error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), pAddrBuf);

            switch (error)
            {
                case 1:
                    return SocketError.Success;
                case 0:
                    return SocketError.InvalidArgument;
                default:
                    return SocketError.Fault;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP4(sockaddr_in* socketAddress, Span<byte> buffer)
        {
            void* pAddrBuf = &socketAddress->sin_addr;

            if (inet_ntop((int)AddressFamily.InterNetwork, pAddrBuf, ref MemoryMarshal.GetReference(buffer), (nuint)buffer.Length) == null)
                return SocketError.Fault;

            return SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP6(sockaddr_in6* socketAddress, Span<byte> buffer)
        {
            void* pAddrBuf = socketAddress->sin6_addr;

            ref int reference = ref Unsafe.AsRef<int>(pAddrBuf);
            if (Unsafe.Add<int>(ref reference, 2) == -0x10000 && reference == 0 && Unsafe.Add(ref reference, 1) == 0)
            {
                if (inet_ntop((int)AddressFamily.InterNetwork, (byte*)pAddrBuf + 12, ref MemoryMarshal.GetReference(buffer), (nuint)buffer.Length) == null)
                    return SocketError.Fault;
            }
            else if (inet_ntop((int)ADDRESS_FAMILY_INTER_NETWORK_V6, pAddrBuf, ref MemoryMarshal.GetReference(buffer), (nuint)buffer.Length) == null)
            {
                return SocketError.Fault;
            }

            return SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName4(sockaddr_in* socketAddress, ReadOnlySpan<char> hostName)
        {
            void* pAddrBuf = &socketAddress->sin_addr;

            int byteCount = Encoding.ASCII.GetByteCount(hostName);
            Span<byte> buffer = stackalloc byte[byteCount];
            Encoding.ASCII.GetBytes(hostName, buffer);

            addrinfo addressInfo = new addrinfo();
            addrinfo* hint, results = null;

            if (getaddrinfo((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), null, &addressInfo, &results) != 0)
                return SocketError.Fault;

            for (hint = results; hint != null; hint = hint->ai_next)
            {
                if (hint->ai_addr != null && hint->ai_addrlen >= (nuint)sizeof(sockaddr_in))
                {
                    if (hint->ai_family == (int)AddressFamily.InterNetwork)
                    {
                        sockaddr_in* __socketAddress_native = (sockaddr_in*)hint->ai_addr;

                        *socketAddress = *__socketAddress_native;
                        socketAddress->sin_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);

                        freeaddrinfo(results);

                        return 0;
                    }
                }
            }

            if (results != null)
                freeaddrinfo(results);

            const int addressFamily = (int)AddressFamily.InterNetwork;

            int error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), pAddrBuf);

            switch (error)
            {
                case 1:
                    return SocketError.Success;
                case 0:
                    return SocketError.InvalidArgument;
                default:
                    return SocketError.Fault;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName6(sockaddr_in6* socketAddress, ReadOnlySpan<char> hostName)
        {
            void* pAddrBuf = socketAddress->sin6_addr;

            int byteCount = Encoding.ASCII.GetByteCount(hostName);
            Span<byte> buffer = stackalloc byte[byteCount];
            Encoding.ASCII.GetBytes(hostName, buffer);

            addrinfo addressInfo = new addrinfo();
            addrinfo* hint, results = null;

            if (getaddrinfo((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), null, &addressInfo, &results) != 0)
                return SocketError.Fault;

            for (hint = results; hint != null; hint = hint->ai_next)
            {
                if (hint->ai_addr != null && hint->ai_addrlen >= (nuint)sizeof(sockaddr_in))
                {
                    if (hint->ai_family == (int)AddressFamily.InterNetwork)
                    {
                        sockaddr_in* __socketAddress_native = (sockaddr_in*)hint->ai_addr;

                        Unsafe.InitBlockUnaligned(pAddrBuf, 0, 8);
                        Unsafe.WriteUnaligned((byte*)pAddrBuf + 8, -0x10000);
                        Unsafe.WriteUnaligned((byte*)pAddrBuf + 12, __socketAddress_native->sin_addr.S_addr);

                        freeaddrinfo(results);

                        return 0;
                    }

                    if (hint->ai_family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                    {
                        sockaddr_in6* __socketAddress_native = (sockaddr_in6*)hint->ai_addr;

                        Unsafe.CopyBlockUnaligned(pAddrBuf, __socketAddress_native->sin6_addr, 20);

                        freeaddrinfo(results);

                        return 0;
                    }
                }
            }

            if (results != null)
                freeaddrinfo(results);

            int addressFamily = (int)ADDRESS_FAMILY_INTER_NETWORK_V6;
            if (buffer.IndexOf((byte)':') == -1)
            {
                addressFamily = (int)AddressFamily.InterNetwork;
                Unsafe.InitBlockUnaligned(pAddrBuf, 0, 8);
                Unsafe.WriteUnaligned((byte*)pAddrBuf + 8, -0x10000);
                pAddrBuf = (byte*)pAddrBuf + 12;
            }

            int error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), pAddrBuf);

            switch (error)
            {
                case 1:
                    return SocketError.Success;
                case 0:
                    return SocketError.InvalidArgument;
                default:
                    return SocketError.Fault;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName4(sockaddr_in* socketAddress, Span<byte> buffer)
        {
            sockaddr_in __socketAddress_native = *socketAddress;

            __socketAddress_native.sin_port = WinSock2.HOST_TO_NET_16(socketAddress->sin_port);

            int error = getnameinfo((sockaddr*)&__socketAddress_native, sizeof(sockaddr_in), ref MemoryMarshal.GetReference(buffer), (ulong)buffer.Length, null, 0, 0x4);

            if (error == 0)
            {
                if (Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)) != null && buffer.Length > 0 && buffer.IndexOf((byte)'\0') < 0)
                    return SocketError.Fault;

                return SocketError.Success;
            }

            if (error != 0x2AF9L)
                return SocketError.Fault;

            return GetIP4(socketAddress, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName6(sockaddr_in6* socketAddress, Span<byte> buffer)
        {
            sockaddr_in6 __socketAddress_native = *socketAddress;

            __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);

            int error = getnameinfo((sockaddr*)&__socketAddress_native, sizeof(sockaddr_in6), ref MemoryMarshal.GetReference(buffer), (ulong)buffer.Length, null, 0, 0x4);

            if (error == 0)
            {
                if (Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)) != null && buffer.Length > 0 && buffer.IndexOf((byte)'\0') < 0)
                    return SocketError.Fault;

                return SocketError.Success;
            }

            if (error != 0x2AF9L)
                return SocketError.Fault;

            return GetIP6(socketAddress, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MicrosecondsToTimeValue(long microseconds, ref TimeValue socketTime)
        {
            const int microcnv = 1000000;
            socketTime.Seconds = (int)(microseconds / microcnv);
            socketTime.Microseconds = (int)(microseconds % microcnv);
        }

        private static readonly int _Initialized;
        private static readonly delegate* managed<nint, sockaddr*, int, SocketError> _bind;
        private static readonly delegate* managed<nint, sockaddr*, int*, SocketError> _getsockname;
        private static readonly delegate* managed<int, int, int, nint> _socket;
        private static readonly delegate* managed<nint, int, int, int> _fcntl;
        private static readonly delegate* managed<nint, SocketOptionLevel, SocketOptionName, int*, int, SocketError> _setsockopt;
        private static readonly delegate* managed<nint, int, int, byte*, int*, SocketError> _getsockopt;
        private static readonly delegate* managed<nint, sockaddr*, int, SocketError> _connect;
        private static readonly delegate* managed<nint, SocketError> _close;
        private static readonly delegate* managed<nint, byte*, int, SocketFlags, byte*, int, int> _sendto;
        private static readonly delegate* managed<nint, byte*, int, SocketFlags, byte*, int*, int> _recvfrom;
        private static readonly delegate* managed<int, nint*, nint*, nint*, TimeValue*, int> _select;
        private static readonly delegate* managed<int, void*, void*, int> _inet_pton;
        private static readonly delegate* managed<byte*, byte*, addrinfo*, addrinfo**, int> _getaddrinfo;
        private static readonly delegate* managed<addrinfo*, void> _freeaddrinfo;
        private static readonly delegate* managed<int, void*, ref byte, nuint, byte*> _inet_ntop;
        private static readonly delegate* managed<sockaddr*, int, ref byte, ulong, byte*, ulong, int, int> _getnameinfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SocketError bind(nint socketHandle, sockaddr* socketAddress, int socketAddressSize) => _bind(socketHandle, socketAddress, socketAddressSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SocketError getsockname(nint socketHandle, sockaddr* socketAddress, int* socketAddressSize) => _getsockname(socketHandle, socketAddress, socketAddressSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nint socket(int af, int type, int protocol) => _socket(af, type, protocol);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int fcntl(nint fd, int cmd, int arg) => _fcntl(fd, cmd, arg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SocketError setsockopt(nint socketHandle, SocketOptionLevel optionLevel, SocketOptionName optionName, int* optionValue, int optionLength) => _setsockopt(socketHandle, optionLevel, optionName, optionValue, optionLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SocketError getsockopt(nint s, int level, int optname, byte* optval, int* optlen) => _getsockopt(s, level, optname, optval, optlen);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SocketError connect(nint s, sockaddr* name, int namelen) => _connect(s, name, namelen);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SocketError close(nint socketHandle) => _close(socketHandle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int sendto(nint socketHandle, byte* buffer, int length, SocketFlags flags, byte* socketAddress, int socketAddressSize) => _sendto(socketHandle, buffer, length, flags, socketAddress, socketAddressSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int recvfrom(nint socketHandle, byte* buffer, int length, SocketFlags flags, byte* socketAddress, int* socketAddressSize) => _recvfrom(socketHandle, buffer, length, flags, socketAddress, socketAddressSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int select(int ignoredParameter, nint* readfds, nint* writefds, nint* exceptfds, TimeValue* timeout) => _select(ignoredParameter, readfds, writefds, exceptfds, timeout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int inet_pton(int family, void* pszAddrString, void* pAddrBuf) => _inet_pton(family, pszAddrString, pAddrBuf);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int getaddrinfo(byte* pNodeName, byte* pServiceName, addrinfo* pHints, addrinfo** ppResult) => _getaddrinfo(pNodeName, pServiceName, pHints, ppResult);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void freeaddrinfo(addrinfo* pAddrInfo) => _freeaddrinfo(pAddrInfo);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte* inet_ntop(int family, void* pAddr, ref byte pStringBuf, nuint stringBufSize) => _inet_ntop(family, pAddr, ref pStringBuf, stringBufSize);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int getnameinfo(sockaddr* pSockaddr, int sockaddrLength, ref byte pNodeBuffer, ulong nodeBufferSize, byte* pServiceBuffer, ulong serviceBufferSize, int flags) => _getnameinfo(pSockaddr, sockaddrLength, ref pNodeBuffer, nodeBufferSize, pServiceBuffer, serviceBufferSize, flags);

        private static class UnixPal
        {
            private const string NATIVE_LIBRARY = "libc";

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int getpid();

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError bind(nint __socketHandle_native, sockaddr* __socketAddress_native, int __socketAddressSize_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError getsockname(nint __socketHandle_native, sockaddr* __socketAddress_native, int* __socketAddressSize_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern nint socket(int af, int type, int protocol);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int fcntl(nint fd, int cmd, int arg);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError setsockopt(nint __socketHandle_native, SocketOptionLevel __optionLevel_native, SocketOptionName __optionName_native, int* __optionValue_native, int __optionLength_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError getsockopt(nint s, int level, int optname, byte* optval, int* optlen);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError connect(nint s, sockaddr* name, int namelen);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError close(nint __socketHandle_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int sendto(nint __socketHandle_native, byte* __pinnedBuffer_native, int __len_native, SocketFlags __socketFlags_native, byte* __socketAddress_native, int __socketAddressSize_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int recvfrom(nint __socketHandle_native, byte* __pinnedBuffer_native, int __len_native, SocketFlags __socketFlags_native, byte* __socketAddress_native, int* __socketAddressSize_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int select(int __ignoredParameter_native, nint* __readfds_native, nint* __writefds_native, nint* __exceptfds_native, TimeValue* __timeout_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int inet_pton(int Family, void* pszAddrString, void* pAddrBuf);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int getaddrinfo(byte* pNodeName, byte* pServiceName, addrinfo* pHints, addrinfo** ppResult);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern void freeaddrinfo(addrinfo* pAddrInfo);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern byte* inet_ntop(int Family, void* pAddr, ref byte pStringBuf, nuint StringBufSize);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int getnameinfo(sockaddr* pSockaddr, int SockaddrLength, ref byte pNodeBuffer, ulong NodeBufferSize, byte* pServiceBuffer, ulong ServiceBufferSize, int Flags);
        }

        private static class iOSPal
        {
            private const string NATIVE_LIBRARY = "__Internal";

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int getpid();

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError bind(nint __socketHandle_native, sockaddr* __socketAddress_native, int __socketAddressSize_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError getsockname(nint __socketHandle_native, sockaddr* __socketAddress_native, int* __socketAddressSize_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern nint socket(int af, int type, int protocol);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int fcntl(nint fd, int cmd, int arg);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError setsockopt(nint __socketHandle_native, SocketOptionLevel __optionLevel_native, SocketOptionName __optionName_native, int* __optionValue_native, int __optionLength_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError getsockopt(nint s, int level, int optname, byte* optval, int* optlen);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError connect(nint s, sockaddr* name, int namelen);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern SocketError close(nint __socketHandle_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int sendto(nint __socketHandle_native, byte* __pinnedBuffer_native, int __len_native, SocketFlags __socketFlags_native, byte* __socketAddress_native, int __socketAddressSize_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int recvfrom(nint __socketHandle_native, byte* __pinnedBuffer_native, int __len_native, SocketFlags __socketFlags_native, byte* __socketAddress_native, int* __socketAddressSize_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int select(int __ignoredParameter_native, nint* __readfds_native, nint* __writefds_native, nint* __exceptfds_native, TimeValue* __timeout_native);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int inet_pton(int Family, void* pszAddrString, void* pAddrBuf);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int getaddrinfo(byte* pNodeName, byte* pServiceName, addrinfo* pHints, addrinfo** ppResult);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern void freeaddrinfo(addrinfo* pAddrInfo);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern byte* inet_ntop(int Family, void* pAddr, ref byte pStringBuf, nuint StringBufSize);

            [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.Cdecl)]
            public static extern int getnameinfo(sockaddr* pSockaddr, int SockaddrLength, ref byte pNodeBuffer, ulong NodeBufferSize, byte* pServiceBuffer, ulong ServiceBufferSize, int Flags);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TimeValue
        {
            public int Seconds;
            public int Microseconds;
        }
    }
}