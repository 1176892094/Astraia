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
    public unsafe struct SocketAddress6 : IEquatable<SocketAddress6>
    {
        public static ref SocketAddress6 NullRef => ref Unsafe.NullRef<SocketAddress6>();

        public Span<byte> IPv6
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateSpan(ref Unsafe.As<SocketAddress6, byte>(ref Unsafe.AsRef(in this)), 16);
        }

        public Span<byte> IPv4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateSpan(ref Unsafe.Add(ref Unsafe.As<SocketAddress6, byte>(ref Unsafe.AsRef(in this)), 12), 4);
        }

        [FieldOffset(16)] public ushort Port;
        [FieldOffset(20)] public uint ScopeId;

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref long reference = ref Unsafe.As<SocketAddress6, long>(ref Unsafe.AsRef(in this));
                return reference != 0 || Unsafe.Add(ref reference, 1) != 0;
            }
        }

        public bool IsIPv4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref int reference = ref Unsafe.As<SocketAddress6, int>(ref Unsafe.AsRef(in this));
                return Unsafe.Add(ref reference, 2) == -0x10000 && reference == 0 && Unsafe.Add(ref reference, 1) == 0;
            }
        }

        public bool IsIPv6
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !IsIPv4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref byte GetPinnableReference() => ref Unsafe.As<SocketAddress6, byte>(ref Unsafe.AsRef(in this));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CreateFromIP(ReadOnlySpan<char> ip, out SocketAddress6 address)
        {
            address = new SocketAddress6();
            return UdpPal6.SetIP(ref address, ip) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CreateFromHostName(ReadOnlySpan<char> name, out SocketAddress6 address)
        {
            address = new SocketAddress6();
            return UdpPal6.SetHostName(ref address, name) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPAddress CreateIPAddress() => new IPAddress(IPv6, ScopeId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPEndPoint CreateIPEndPoint() => new IPEndPoint(CreateIPAddress(), Port);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SocketAddress CreateSocketAddress()
        {
            SocketAddress socketAddress = new SocketAddress(AddressFamily.InterNetworkV6);
            ReadOnlySpan<byte> buffer = (ReadOnlySpan<byte>)IPv6;
            ushort port = WinSock2.HOST_TO_NET_16(Port);
#if NET8_0_OR_GREATER
            buffer.CopyTo(socketAddress.Buffer.Span.Slice(8));
            Unsafe.WriteUnaligned(ref socketAddress.Buffer.Span[2], port);
#else
            for (int i = 8; i < 24; ++i)
                socketAddress[i] = buffer[i - 8];
            buffer = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ushort, byte>(ref port), 2);
            for (int i = 2; i < 4; ++i)
                socketAddress[i] = buffer[i - 2];
#endif
            return socketAddress;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetIP(int bufferSize, out string? ip)
        {
            Span<byte> buffer = stackalloc byte[bufferSize];
            SocketError error = UdpPal6.GetIP(ref Unsafe.AsRef(in this), buffer);
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
            SocketError error = UdpPal6.GetIP(ref Unsafe.AsRef(in this), buffer);
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
            SocketError error = UdpPal6.GetHostName(ref Unsafe.AsRef(in this), buffer);
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
            SocketError error = UdpPal6.GetHostName(ref Unsafe.AsRef(in this), buffer);
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

        public bool Equals(SocketAddress6 other)
        {
#if NET7_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
                return Vector128.LoadUnsafe(ref Unsafe.As<SocketAddress6, byte>(ref Unsafe.AsRef(in this))) == Vector128.LoadUnsafe(ref Unsafe.As<SocketAddress6, byte>(ref other)) && Port == other.Port;
#endif
            ref int left = ref Unsafe.As<SocketAddress6, int>(ref Unsafe.AsRef(in this));
            ref int right = ref Unsafe.As<SocketAddress6, int>(ref other);
            return left == right && Unsafe.Add(ref left, 1) == Unsafe.Add(ref right, 1) && Unsafe.Add(ref left, 2) == Unsafe.Add(ref right, 2) && Unsafe.Add(ref left, 3) == Unsafe.Add(ref right, 3) && Unsafe.Add(ref left, 4) == Unsafe.Add(ref right, 4);
        }

        public override bool Equals(object? obj) => obj is SocketAddress6 socketAddress && Equals(socketAddress);

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
#if NET6_0_OR_GREATER
            hashCode.AddBytes(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<SocketAddress6, byte>(ref Unsafe.AsRef(in this)), 24));
#else
            ref int reference = ref Unsafe.As<SocketAddress6, int>(ref Unsafe.AsRef(in this));
            for (int i = 0; i < 6; i++)
                hashCode.Add(Unsafe.Add(ref reference, i));
#endif
            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            Span<byte> buffer = stackalloc byte[64];
            return UdpPal6.GetIP(ref Unsafe.AsRef(in this), buffer) == 0 ? Encoding.ASCII.GetString(buffer[..buffer.IndexOf((byte)'\0')]) + ":" + Port : "ERROR";
        }

        public string ToString(int bufferSize)
        {
            Span<byte> buffer = stackalloc byte[bufferSize];
            return UdpPal6.GetIP(ref Unsafe.AsRef(in this), buffer) == 0 ? Encoding.ASCII.GetString(buffer[..buffer.IndexOf((byte)'\0')]) + ":" + Port : "ERROR";
        }

        public static bool operator ==(SocketAddress6 left, SocketAddress6 right) => left.Equals(right);
        public static bool operator !=(SocketAddress6 left, SocketAddress6 right) => !(left == right);

        public Span<byte> AsSpan() => MemoryMarshal.CreateSpan(ref Unsafe.As<SocketAddress6, byte>(ref Unsafe.AsRef(in this)), 24);
        public ReadOnlySpan<byte> AsReadOnlySpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<SocketAddress6, byte>(ref Unsafe.AsRef(in this)), 24);

        public static implicit operator MnSocketAddress(SocketAddress6 socketAddress) => Unsafe.As<SocketAddress6, MnSocketAddress>(ref socketAddress);

        public static implicit operator SocketAddress4(SocketAddress6 socketAddress)
        {
            if (socketAddress.IsIPv6)
                throw new SocketException((int)SocketError.AddressFamilyNotSupported);

            return Unsafe.As<byte, SocketAddress4>(ref Unsafe.AddByteOffset(ref Unsafe.As<SocketAddress6, byte>(ref socketAddress), (nint)12));
        }
    }
}