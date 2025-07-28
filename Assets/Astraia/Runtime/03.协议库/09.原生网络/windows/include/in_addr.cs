using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace winsock
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public unsafe struct in_addr
    {
        [FieldOffset(0)] public fixed byte S_un_b[4];
        [FieldOffset(0)] public fixed ushort S_un_w[2];
        [FieldOffset(0)] public uint S_addr;
    }
}