using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace winsock
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe struct sockaddr_in
    {
        [FieldOffset(0)] public ushort sin_family;
        [FieldOffset(2)] public ushort sin_port;
        [FieldOffset(4)] public in_addr sin_addr;
        [FieldOffset(8)] public fixed byte sin_zero[8];
    }
}