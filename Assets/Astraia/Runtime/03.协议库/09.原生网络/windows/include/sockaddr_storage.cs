using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace winsock
{
    [StructLayout(LayoutKind.Explicit, Size = 128)]
    public struct sockaddr_storage
    {
        [FieldOffset(0)] public ushort ss_family;
        [FieldOffset(8)] public long __ss_align;
    }
}