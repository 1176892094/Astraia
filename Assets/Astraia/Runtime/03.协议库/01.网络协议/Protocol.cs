// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-03 02:08:22
// // # Recently: 2025-08-03 02:08:22
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Threading;

namespace Astraia.Common
{
    internal sealed unsafe class Protocol : IDisposable
    {
        private byte[] buffer;
        private IKCPCB* kcp;
        private Action<byte[], int> output;
        public uint State => kcp->state;
        public uint Count => kcp->nrcv_buf + kcp->nrcv_que + kcp->nsnd_buf + kcp->nsnd_que;
        public uint Death
        {
            get => kcp->dead_link;
            set => kcp->dead_link = value;
        }

        public Protocol(uint conv, Action<byte[], int> output)
        {
            this.output = output;
            kcp = Kcp.ikcp_create(conv, ref buffer);
        }

        public int Input(byte[] buffer, int offset, int length)
        {
            fixed (byte* ptr = &buffer[offset])
            {
                return Kcp.ikcp_input(kcp, ptr, length);
            }
        }

        public int Receive(byte[] buffer, int length)
        {
            fixed (byte* ptr = buffer)
            {
                return Kcp.ikcp_recv(kcp, ptr, length);
            }
        }

        public int Send(byte[] buffer, int offset, int length)
        {
            fixed (byte* ptr = &buffer[offset])
            {
                return Kcp.ikcp_send(kcp, ptr, length);
            }
        }

        public void Update(uint current)
        {
            fixed (byte* ptr = buffer)
            {
                Kcp.ikcp_update(kcp, current, ptr, buffer, output);
            }
        }

        public int PeekSize()
        {
            return Kcp.ikcp_peeksize(kcp);
        }

        public void SetMtu(int mtu)
        {
            Kcp.ikcp_setmtu(kcp, mtu, ref buffer);
        }

        public void SetNoDelay(int noDelay, int interval, int resend, int nc)
        {
            Kcp.ikcp_nodelay(kcp, noDelay, interval, resend, nc);
        }

        public void SetWindowSize(int sendWindow, int receiveWindow)
        {
            Kcp.ikcp_wndsize(kcp, sendWindow, receiveWindow);
        }

        private int dispose;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref dispose, 1, 0) != 0)
            {
                return;
            }

            Kcp.ikcp_release(kcp);
            kcp = null;
            output = null;
            buffer = null;
            GC.SuppressFinalize(this);
        }

        ~Protocol() => Dispose();
    }
}