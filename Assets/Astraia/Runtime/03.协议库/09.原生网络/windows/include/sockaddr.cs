using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace winsock
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct sockaddr
    {
        public ushort sa_family;
        public fixed byte sa_data[14];
    }
}