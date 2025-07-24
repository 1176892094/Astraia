using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace winsock
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct addrinfo
    {
        public int ai_flags;
        public int ai_family;
        public int ai_socktype;
        public int ai_protocol;
        public nuint ai_addrlen;
        public char* ai_canonname;
        public sockaddr* ai_addr;
        public addrinfo* ai_next;
    }
}