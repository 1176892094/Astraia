using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using winsock;
#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

#pragma warning disable CS1591
#pragma warning disable CS8632
#pragma warning disable CS9084

// ReSharper disable ALL

namespace NativeSockets.Udp
{
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public unsafe struct MnSocketAddress
    {
        public static ref MnSocketAddress NullRef => ref Unsafe.NullRef<MnSocketAddress>();

        public Span<byte> IPv6
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateSpan(ref Unsafe.As<MnSocketAddress, byte>(ref Unsafe.AsRef(in this)), 16);
        }

        public Span<byte> IPv4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.As<MnSocketAddress, byte>(ref Unsafe.AsRef(in this)), 12), 4);
        }

        [FieldOffset(12)] public uint Address;
        [FieldOffset(16)] public ushort Port;
        [FieldOffset(20)] public uint ScopeId;

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref long reference = ref Unsafe.As<MnSocketAddress, long>(ref Unsafe.AsRef(in this));
                return reference != 0 || Unsafe.Add(ref reference, 1) != 0;
            }
        }

        public bool IsIPv4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref int reference = ref Unsafe.As<MnSocketAddress, int>(ref Unsafe.AsRef(in this));
                return Unsafe.Add(ref reference, 2) == -0x10000 && reference == 0 && Unsafe.Add(ref reference, 1) == 0;
            }
        }

        public bool IsIPv6
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsIPv4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref byte GetPinnableReference() => ref Unsafe.As<MnSocketAddress, byte>(ref Unsafe.AsRef(in this));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CreateFromIP(ReadOnlySpan<char> ip, out MnSocketAddress address)
        {
            address = new MnSocketAddress();
            return UdpPal6.SetIP(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref address), ip) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CreateFromHostName(ReadOnlySpan<char> name, out MnSocketAddress address)
        {
            address = new MnSocketAddress();
            return UdpPal6.SetHostName(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref address), name) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CreateFromIPAddress(IPAddress socketAddress, out MnSocketAddress address)
        {
            address = new MnSocketAddress();

            if (socketAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                socketAddress.TryWriteBytes(MemoryMarshal.CreateSpan(ref Unsafe.As<MnSocketAddress, byte>(ref address), 16), out _);
                address.ScopeId = (uint)socketAddress.ScopeId;

                return true;
            }

            if (socketAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                ref byte reference = ref Unsafe.As<MnSocketAddress, byte>(ref address);
                Unsafe.InitBlockUnaligned(ref reference, 0, 8);
                Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref reference, (nint)8), -0x10000);

                socketAddress.TryWriteBytes(MemoryMarshal.CreateSpan(ref Unsafe.As<uint, byte>(ref address.Address), 4), out _);

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CreateFromIPEndPoint(IPEndPoint socketAddress, out MnSocketAddress address)
        {
            if (CreateFromIPAddress(socketAddress.Address, out address))
            {
                address.Port = (ushort)socketAddress.Port;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CreateFromSocketAddress(SocketAddress socketAddress, out MnSocketAddress address)
        {
            address = new MnSocketAddress();

            if (socketAddress.Family == AddressFamily.InterNetworkV6)
            {
                if (socketAddress.Size != 28)
                    return false;

                sockaddr_in6 sockaddrIn6 = new sockaddr_in6();
                Span<byte> buffer = MemoryMarshal.CreateSpan(ref Unsafe.As<sockaddr_in6, byte>(ref sockaddrIn6), 28);

#if NET8_0_OR_GREATER
                socketAddress.Buffer.Span.Slice(0, 28).CopyTo(buffer);
#else
                for (int i = 0; i < 28; i++)
                    buffer[i] = socketAddress[i];
#endif

                Unsafe.CopyBlockUnaligned(Unsafe.AsPointer(ref address), sockaddrIn6.sin6_addr, 16);
                address.Port = WinSock2.NET_TO_HOST_16(sockaddrIn6.sin6_port);

                return true;
            }

            if (socketAddress.Family == AddressFamily.InterNetwork)
            {
                if (socketAddress.Size != 16)
                    return false;

                sockaddr_in sockaddrIn4 = new sockaddr_in();
                Span<byte> buffer = MemoryMarshal.CreateSpan(ref Unsafe.As<sockaddr_in, byte>(ref sockaddrIn4), 16);

#if NET8_0_OR_GREATER
                socketAddress.Buffer.Span.Slice(0, 16).CopyTo(buffer);
#else
                for (int i = 0; i < 16; i++)
                    buffer[i] = socketAddress[i];
#endif

                ref byte reference = ref Unsafe.As<MnSocketAddress, byte>(ref address);
                Unsafe.InitBlockUnaligned(ref reference, 0, 8);
                Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref reference, (nint)8), -0x10000);

                Unsafe.WriteUnaligned(Unsafe.AsPointer(ref address.Address), sockaddrIn4.sin_addr);
                address.Port = WinSock2.NET_TO_HOST_16(sockaddrIn4.sin_port);

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPEndPoint CreateIPEndPoint(bool ipv6 = false) => ipv6 || IsIPv6 ? ((SocketAddress6)this).CreateIPEndPoint() : ((SocketAddress4)this).CreateIPEndPoint();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SocketAddress CreateSocketAddress(bool ipv6 = false) => ipv6 || IsIPv6 ? ((SocketAddress6)this).CreateSocketAddress() : ((SocketAddress4)this).CreateSocketAddress();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetIP(int bufferSize, out string? ip)
        {
            Span<byte> buffer = stackalloc byte[bufferSize];
            SocketError error = UdpPal6.GetIP(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref Unsafe.AsRef(in this)), buffer);
            if (error == 0)
            {
                ip = Encoding.ASCII.GetString(buffer[..buffer.IndexOf((byte)'\0')]);
                return true;
            }

            ip = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetIP(int bufferSize, Span<byte> ip, out int byteCount)
        {
            Span<byte> buffer = stackalloc byte[bufferSize];
            SocketError error = UdpPal6.GetIP(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref Unsafe.AsRef(in this)), buffer);
            if (error == 0)
            {
                byteCount = buffer.IndexOf((byte)'\0');
                if (ip.Length < byteCount)
                    return false;
                Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(ip), ref MemoryMarshal.GetReference(buffer), (uint)byteCount);
                return true;
            }

            byteCount = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetHostName(int bufferSize, out string? ip)
        {
            Span<byte> buffer = stackalloc byte[bufferSize];
            SocketError error = UdpPal6.GetHostName(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref Unsafe.AsRef(in this)), buffer);
            if (error == 0)
            {
                ip = Encoding.ASCII.GetString(buffer[..buffer.IndexOf((byte)'\0')]);
                return true;
            }

            ip = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetHostName(int bufferSize, Span<byte> name, out int byteCount)
        {
            Span<byte> buffer = stackalloc byte[bufferSize];
            SocketError error = UdpPal6.GetHostName(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref Unsafe.AsRef(in this)), buffer);
            if (error == 0)
            {
                byteCount = buffer.IndexOf((byte)'\0');
                if (name.Length < byteCount)
                    return false;
                Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(name), ref MemoryMarshal.GetReference(buffer), (uint)byteCount);
                return true;
            }

            byteCount = 0;
            return false;
        }

        public bool Equals(MnSocketAddress other)
        {
#if NET7_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
                return Vector128.LoadUnsafe(ref Unsafe.As<MnSocketAddress, byte>(ref Unsafe.AsRef(in this))) == Vector128.LoadUnsafe(ref Unsafe.As<MnSocketAddress, byte>(ref other)) && Port == other.Port;
#endif
            ref int left = ref Unsafe.As<MnSocketAddress, int>(ref Unsafe.AsRef(in this));
            ref int right = ref Unsafe.As<MnSocketAddress, int>(ref other);
            return left == right && Unsafe.Add(ref left, 1) == Unsafe.Add(ref right, 1) && Unsafe.Add(ref left, 2) == Unsafe.Add(ref right, 2) && Unsafe.Add(ref left, 3) == Unsafe.Add(ref right, 3) && Unsafe.Add(ref left, 4) == Unsafe.Add(ref right, 4);
        }

        public override bool Equals(object? obj) => obj is MnSocketAddress socketAddress && Equals(socketAddress);

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
#if NET6_0_OR_GREATER
            hashCode.AddBytes(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<MnSocketAddress, byte>(ref Unsafe.AsRef(in this)), 24));
#else
            ref int reference = ref Unsafe.As<MnSocketAddress, int>(ref Unsafe.AsRef(in this));
            for (int i = 0; i < 6; i++)
                hashCode.Add(Unsafe.Add(ref reference, i));
#endif
            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            Span<byte> buffer = stackalloc byte[64];
            return UdpPal6.GetIP(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref Unsafe.AsRef(in this)), buffer) == 0 ? Encoding.ASCII.GetString(buffer[..buffer.IndexOf((byte)'\0')]) + ":" + Port : "ERROR";
        }

        public string ToString(int bufferSize)
        {
            Span<byte> buffer = stackalloc byte[bufferSize];
            return UdpPal6.GetIP(ref Unsafe.As<MnSocketAddress, SocketAddress6>(ref Unsafe.AsRef(in this)), buffer) == 0 ? Encoding.ASCII.GetString(buffer[..buffer.IndexOf((byte)'\0')]) + ":" + Port : "ERROR";
        }

        public static bool operator ==(MnSocketAddress left, MnSocketAddress right) => left.Equals(right);
        public static bool operator !=(MnSocketAddress left, MnSocketAddress right) => !(left == right);

        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.As<MnSocketAddress, byte>(ref Unsafe.AsRef(in this)), 24);
        public ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<MnSocketAddress, byte>(ref Unsafe.AsRef(in this)), 24);

        public static implicit operator SocketAddress6(MnSocketAddress socketAddress) => Unsafe.As<MnSocketAddress, SocketAddress6>(ref socketAddress);

        public static implicit operator SocketAddress4(MnSocketAddress socketAddress)
        {
            if (socketAddress.IsIPv6)
                throw new SocketException((int)SocketError.AddressFamilyNotSupported);

            return Unsafe.As<uint, SocketAddress4>(ref socketAddress.Address);
        }
    }
}