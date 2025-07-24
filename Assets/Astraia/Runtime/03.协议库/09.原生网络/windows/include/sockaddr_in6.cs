using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace winsock
{
    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public unsafe struct sockaddr_in6
    {
        [FieldOffset(0)] public ushort sin6_family;
        [FieldOffset(2)] public ushort sin6_port;
        [FieldOffset(4)] public uint sin6_flowinfo;
        [FieldOffset(8)] public fixed byte sin6_addr[16];
        [FieldOffset(24)] public uint sin6_scope_id;
    }
}