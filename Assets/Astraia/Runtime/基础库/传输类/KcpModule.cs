// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-08-03 02:08:22
// # Recently: 2025-08-03 02:08:22
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections.Generic;

namespace Astraia
{
    internal unsafe class KcpModule
    {
        private static readonly Dictionary<int, SendDelegate> methods = new Dictionary<int, SendDelegate>();
        private int key;
        private IKCPCB* kcp;

        public uint State => kcp->state;
        public uint Death => kcp->dead_link;
        public uint Count => kcp->nrcv_buf + kcp->nrcv_que + kcp->nsnd_buf + kcp->nsnd_que;

        public static void Build(KcpModule module, SendDelegate onSend)
        {
            module.Release();
            int key;
            do
            {
                key = Seed.Next();
            } while (methods.ContainsKey(key));

            module.key = key;
            module.kcp = Kcp.ikcp_create(0, (void*)key);

            methods.Add(key, onSend);
            module.kcp->dead_link = Const.DEAD_LINK;
            Kcp.ikcp_setmtu(module.kcp, Const.MTU_DEF - Const.HEAD_SIZE);
            Kcp.ikcp_nodelay(module.kcp, 1, Const.STEP_TIME, Const.FAST_SEND, 1);
            Kcp.ikcp_wndsize(module.kcp, Const.SED_WIN, Const.REV_WIN);
            Kcp.ikcp_setoutput(module.kcp, &Output);
        }

        private static int Output(byte* bytes, int count, IKCPCB* kcp, void* user)
        {
            if (methods.TryGetValue((int)user, out var method))
            {
                method(bytes, count);
            }

            return count;
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

        public void Flush()
        {
            Kcp.ikcp_flush(kcp);
            methods.Remove(key);
        }

        public void Update(uint current)
        {
            Kcp.ikcp_update(kcp, current);
        }

        public int PeekSize()
        {
            return Kcp.ikcp_peeksize(kcp);
        }

        public void Release()
        {
            if (kcp != null)
            {
                Kcp.ikcp_release(kcp);
                methods.Remove(key);
                kcp = null;
            }
        }

        ~KcpModule()
        {
            Release();
        }
    }
}