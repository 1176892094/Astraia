// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: Nevin
// // # Version: 1.0.0
// // # History: 2025-08-03 02:08:22
// // # Recently: 2025-08-03 02:08:22
// // # Copyright: 2024, Nevin
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable All

namespace Astraia
{
    internal static unsafe partial class Kcp
    {
        public const uint IKCP_RTO_NDL = 30;
        public const uint IKCP_RTO_MIN = 100;
        public const uint IKCP_RTO_DEF = 200;
        public const uint IKCP_RTO_MAX = 60000;
        public const uint IKCP_CMD_PUSH = 81;
        public const uint IKCP_CMD_ACK = 82;
        public const uint IKCP_CMD_WASK = 83;
        public const uint IKCP_CMD_WINS = 84;
        public const uint IKCP_ASK_SEND = 1;
        public const uint IKCP_ASK_TELL = 2;
        public const uint IKCP_WND_SND = 32;
        public const uint IKCP_WND_RCV = 128;
        public const uint IKCP_MTU_DEF = 1400;
        public const uint IKCP_ACK_FAST = 3;
        public const uint IKCP_INTERVAL = 100;
        public const uint IKCP_OVERHEAD = 24;
        public const uint IKCP_DEADLINK = 20;
        public const uint IKCP_THRESH_INIT = 2;
        public const uint IKCP_THRESH_MIN = 2;
        public const uint IKCP_PROBE_INIT = 7000;
        public const uint IKCP_PROBE_LIMIT = 120000;
        public const uint IKCP_FASTACK_LIMIT = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* ikcp_encode8u(byte* p, byte c)
        {
            *(byte*)p++ = c;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* ikcp_decode8u(byte* p, byte* c)
        {
            *c = *(byte*)p++;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* ikcp_encode16u(byte* p, ushort w)
        {
            memcpy(p, &w, (nuint)2);
            p += 2;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* ikcp_decode16u(byte* p, ushort* w)
        {
            memcpy(w, p, (nuint)2);
            p += 2;
            return p;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* ikcp_encode32u(byte* p, uint l)
        {
            memcpy(p, &l, (nuint)4);
            p += 4;
            return p;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* ikcp_decode32u(byte* p, uint* l)
        {
            memcpy(l, p, (nuint)4);
            p += 4;
            return p;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint _imin_(uint a, uint b)
        {
            return a <= b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint _imax_(uint a, uint b)
        {
            return a >= b ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint _ibound_(uint lower, uint middle, uint upper)
        {
            return _imin_(_imax_(lower, middle), upper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint _iceilpow2_(uint x)
        {
            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            ++x;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long _itimediff(uint later, uint earlier)
        {
            return ((int)(later - earlier));
        }

        public static delegate* managed<nuint, void*> ikcp_malloc_hook = null;
        public static delegate* managed<void*, void> ikcp_free_hook = null;

        public static void* ikcp_malloc(nuint size)
        {
            if (ikcp_malloc_hook != null)
            {
                return ikcp_malloc_hook(size);
            }

            return malloc(size);
        }

        public static void ikcp_free(void* ptr)
        {
            if (ikcp_free_hook != null)
            {
                ikcp_free_hook(ptr);
            }
            else
            {
                free(ptr);
            }
        }

        public static void ikcp_allocator(delegate* managed<nuint, void*> new_malloc, delegate* managed<void*, void> new_free)
        {
            ikcp_malloc_hook = new_malloc;
            ikcp_free_hook = new_free;
        }

        public static IKCPSEG* ikcp_segment_new(IKCPCB* kcp, int size)
        {
            return (IKCPSEG*)ikcp_malloc((nuint)(sizeof(IKCPSEG) + size));
        }

        public static void ikcp_segment_delete(IKCPCB* kcp, IKCPSEG* seg)
        {
            ikcp_free(seg);
        }

        public static void ikcp_output(IKCPCB* kcp, int size, byte[] destination, Action<byte[], int> output)
        {
            assert(kcp != null);
            assert(output != null);

            if (size == 0) return;
            output(destination, size);
        }

        public static IKCPCB* ikcp_create(uint conv, ref byte[] buffer)
        {
            IKCPCB* kcp = (IKCPCB*)ikcp_malloc((nuint)sizeof(IKCPCB));
            if (kcp == null) return null;
            kcp->conv = conv;
            kcp->snd_una = 0;
            kcp->snd_nxt = 0;
            kcp->rcv_nxt = 0;
            kcp->ts_recent = 0;
            kcp->ts_lastack = 0;
            kcp->ts_probe = 0;
            kcp->probe_wait = 0;
            kcp->snd_wnd = IKCP_WND_SND;
            kcp->rcv_wnd = IKCP_WND_RCV;
            kcp->rmt_wnd = IKCP_WND_RCV;
            kcp->cwnd = 0;
            kcp->incr = 0;
            kcp->probe = 0;
            kcp->mtu = IKCP_MTU_DEF;
            kcp->mss = kcp->mtu - IKCP_OVERHEAD;
            kcp->stream = 0;

            buffer = new byte[(kcp->mtu + IKCP_OVERHEAD) * 3];

            iqueue_init(&kcp->snd_queue);
            iqueue_init(&kcp->rcv_queue);
            iqueue_init(&kcp->snd_buf);
            iqueue_init(&kcp->rcv_buf);
            kcp->nrcv_buf = 0;
            kcp->nsnd_buf = 0;
            kcp->nrcv_que = 0;
            kcp->nsnd_que = 0;
            kcp->state = 0;
            kcp->acklist = null;
            kcp->ackblock = 0;
            kcp->ackcount = 0;
            kcp->rx_srtt = 0;
            kcp->rx_rttval = 0;
            kcp->rx_rto = (int)IKCP_RTO_DEF;
            kcp->rx_minrto = (int)IKCP_RTO_MIN;
            kcp->current = 0;
            kcp->interval = IKCP_INTERVAL;
            kcp->ts_flush = IKCP_INTERVAL;
            kcp->nodelay = 0;
            kcp->updated = 0;
            kcp->ssthresh = IKCP_THRESH_INIT;
            kcp->fastresend = 0;
            kcp->fastlimit = (int)IKCP_FASTACK_LIMIT;
            kcp->nocwnd = 0;
            kcp->xmit = 0;
            kcp->dead_link = IKCP_DEADLINK;

            return kcp;
        }

        public static void ikcp_release(IKCPCB* kcp)
        {
            assert(kcp != null);
            if (kcp != null)
            {
                IKCPSEG* seg;
                while (!iqueue_is_empty(&kcp->snd_buf))
                {
                    seg = iqueue_entry<IKCPSEG>(kcp->snd_buf.next);
                    iqueue_del(&seg->node);
                    ikcp_segment_delete(kcp, seg);
                }

                while (!iqueue_is_empty(&kcp->rcv_buf))
                {
                    seg = iqueue_entry<IKCPSEG>(kcp->rcv_buf.next);
                    iqueue_del(&seg->node);
                    ikcp_segment_delete(kcp, seg);
                }

                while (!iqueue_is_empty(&kcp->snd_queue))
                {
                    seg = iqueue_entry<IKCPSEG>(kcp->snd_queue.next);
                    iqueue_del(&seg->node);
                    ikcp_segment_delete(kcp, seg);
                }

                while (!iqueue_is_empty(&kcp->rcv_queue))
                {
                    seg = iqueue_entry<IKCPSEG>(kcp->rcv_queue.next);
                    iqueue_del(&seg->node);
                    ikcp_segment_delete(kcp, seg);
                }

                if (kcp->acklist != null)
                {
                    ikcp_free(kcp->acklist);
                }

                kcp->nrcv_buf = 0;
                kcp->nsnd_buf = 0;
                kcp->nrcv_que = 0;
                kcp->nsnd_que = 0;
                kcp->ackcount = 0;
                kcp->acklist = null;
                ikcp_free(kcp);
            }
        }

        public static int ikcp_recv(IKCPCB* kcp, byte* buffer, int len)
        {
            IQUEUEHEAD* p;
            int ispeek = (len < 0) ? 1 : 0;
            int peeksize;
            int recover = 0;
            IKCPSEG* seg;
            assert(kcp != null);

            if (iqueue_is_empty(&kcp->rcv_queue))
                return -1;

            if (len < 0) len = -len;

            peeksize = ikcp_peeksize(kcp);

            if (peeksize < 0)
                return -2;

            if (peeksize > len)
                return -3;

            if (kcp->nrcv_que >= kcp->rcv_wnd)
                recover = 1;


            for (len = 0, p = kcp->rcv_queue.next; p != &kcp->rcv_queue;)
            {
                int fragment;
                seg = iqueue_entry<IKCPSEG>(p);
                p = p->next;

                if (buffer != null)
                {
                    memcpy(buffer, seg->data, (nuint)seg->len);
                    buffer += seg->len;
                }

                len += (int)seg->len;
                fragment = (int)seg->frg;

                if (ispeek == 0)
                {
                    iqueue_del(&seg->node);
                    ikcp_segment_delete(kcp, seg);
                    kcp->nrcv_que--;
                }

                if (fragment == 0)
                    break;
            }

            assert(len == peeksize);


            while (!iqueue_is_empty(&kcp->rcv_buf))
            {
                seg = iqueue_entry<IKCPSEG>(kcp->rcv_buf.next);
                if (seg->sn == kcp->rcv_nxt && kcp->nrcv_que < kcp->rcv_wnd)
                {
                    iqueue_del(&seg->node);
                    kcp->nrcv_buf--;
                    iqueue_add_tail(&seg->node, &kcp->rcv_queue);
                    kcp->nrcv_que++;
                    kcp->rcv_nxt++;
                }
                else
                {
                    break;
                }
            }


            if (kcp->nrcv_que < kcp->rcv_wnd && (recover != 0))
            {
                kcp->probe |= IKCP_ASK_TELL;
            }

            return len;
        }

        public static int ikcp_peeksize(IKCPCB* kcp)
        {
            IQUEUEHEAD* p;
            IKCPSEG* seg;
            int length = 0;

            assert(kcp != null);

            if (iqueue_is_empty(&kcp->rcv_queue)) return -1;

            seg = iqueue_entry<IKCPSEG>(kcp->rcv_queue.next);
            if (seg->frg == 0) return (int)seg->len;

            if (kcp->nrcv_que < seg->frg + 1) return -1;

            for (p = kcp->rcv_queue.next; p != &kcp->rcv_queue; p = p->next)
            {
                seg = iqueue_entry<IKCPSEG>(p);
                length += (int)seg->len;
                if (seg->frg == 0) break;
            }

            return length;
        }

        public static int ikcp_send(IKCPCB* kcp, byte* buffer, int len)
        {
            IKCPSEG* seg;
            int count, i;
            int sent = 0;

            assert(kcp->mss > 0);
            if (len < 0) return -1;

            if (kcp->stream != 0)
            {
                if (!iqueue_is_empty(&kcp->snd_queue))
                {
                    IKCPSEG* old = iqueue_entry<IKCPSEG>(kcp->snd_queue.prev);
                    if (old->len < kcp->mss)
                    {
                        int capacity = (int)(kcp->mss - old->len);
                        int extend = (len < capacity) ? len : capacity;
                        seg = ikcp_segment_new(kcp, (int)(old->len + extend));
                        assert(seg != null);
                        if (seg == null)
                        {
                            return -2;
                        }

                        iqueue_add_tail(&seg->node, &kcp->snd_queue);
                        memcpy(seg->data, old->data, (nuint)old->len);
                        if (buffer != null)
                        {
                            memcpy(seg->data + old->len, buffer, (nuint)extend);
                            buffer += extend;
                        }

                        seg->len = (uint)(old->len + extend);
                        seg->frg = 0;
                        len -= extend;
                        iqueue_del_init(&old->node);
                        ikcp_segment_delete(kcp, old);
                        sent = extend;
                    }
                }

                if (len <= 0)
                {
                    return sent;
                }
            }

            if (len <= (int)kcp->mss) count = 1;
            else count = (int)((len + kcp->mss - 1) / kcp->mss);

            if (kcp->stream == 0 && count > 255) return -2;

            if (count >= (int)kcp->rcv_wnd)
            {
                if (kcp->stream != 0 && sent > 0)
                    return sent;
                return -2;
            }

            if (count == 0) count = 1;

            for (i = 0; i < count; i++)
            {
                int size = len > (int)kcp->mss ? (int)kcp->mss : len;
                seg = ikcp_segment_new(kcp, size);
                assert(seg != null);
                if (seg == null)
                {
                    return -2;
                }

                if ((buffer != null) && len > 0)
                {
                    memcpy(seg->data, buffer, (nuint)size);
                }

                seg->len = (uint)size;
                seg->frg = (uint)((kcp->stream == 0) ? (count - i - 1) : 0);
                iqueue_init(&seg->node);
                iqueue_add_tail(&seg->node, &kcp->snd_queue);
                kcp->nsnd_que++;
                if (buffer != null)
                {
                    buffer += size;
                }

                len -= size;
                sent += size;
            }

            return sent;
        }

        public static void ikcp_update_ack(IKCPCB* kcp, int rtt)
        {
            int rto = 0;
            if (kcp->rx_srtt == 0)
            {
                kcp->rx_srtt = rtt;
                kcp->rx_rttval = rtt / 2;
            }
            else
            {
                long delta = rtt - kcp->rx_srtt;
                if (delta < 0) delta = -delta;
                kcp->rx_rttval = (int)((3 * kcp->rx_rttval + delta) / 4);
                kcp->rx_srtt = (7 * kcp->rx_srtt + rtt) / 8;
                if (kcp->rx_srtt < 1) kcp->rx_srtt = 1;
            }

            rto = (int)(kcp->rx_srtt + _imax_(kcp->interval, (uint)(4 * kcp->rx_rttval)));
            kcp->rx_rto = (int)_ibound_((uint)kcp->rx_minrto, (uint)rto, IKCP_RTO_MAX);
        }

        public static void ikcp_shrink_buf(IKCPCB* kcp)
        {
            IQUEUEHEAD* p = kcp->snd_buf.next;
            if (p != &kcp->snd_buf)
            {
                IKCPSEG* seg = iqueue_entry<IKCPSEG>(p);
                kcp->snd_una = seg->sn;
            }
            else
            {
                kcp->snd_una = kcp->snd_nxt;
            }
        }

        public static void ikcp_parse_ack(IKCPCB* kcp, uint sn)
        {
            IQUEUEHEAD* p, next;

            if (_itimediff(sn, kcp->snd_una) < 0 || _itimediff(sn, kcp->snd_nxt) >= 0)
                return;

            for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = next)
            {
                IKCPSEG* seg = iqueue_entry<IKCPSEG>(p);
                next = p->next;
                if (sn == seg->sn)
                {
                    iqueue_del(p);
                    ikcp_segment_delete(kcp, seg);
                    kcp->nsnd_buf--;
                    break;
                }

                if (_itimediff(sn, seg->sn) < 0)
                {
                    break;
                }
            }
        }

        public static void ikcp_parse_una(IKCPCB* kcp, uint una)
        {
            IQUEUEHEAD* p, next;
            for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = next)
            {
                IKCPSEG* seg = iqueue_entry<IKCPSEG>(p);
                next = p->next;
                if (_itimediff(una, seg->sn) > 0)
                {
                    iqueue_del(p);
                    ikcp_segment_delete(kcp, seg);
                    kcp->nsnd_buf--;
                }
                else
                {
                    break;
                }
            }
        }

        public static void ikcp_parse_fastack(IKCPCB* kcp, uint sn, uint ts)
        {
            IQUEUEHEAD* p, next;

            if (_itimediff(sn, kcp->snd_una) < 0 || _itimediff(sn, kcp->snd_nxt) >= 0)
                return;

            for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = next)
            {
                IKCPSEG* seg = iqueue_entry<IKCPSEG>(p);
                next = p->next;
                if (_itimediff(sn, seg->sn) < 0)
                {
                    break;
                }
                else if (sn != seg->sn)
                {
                    seg->fastack++;
                }
            }
        }

        public static void ikcp_ack_push(IKCPCB* kcp, uint sn, uint ts)
        {
            uint newsize = kcp->ackcount + 1;
            uint* ptr;

            if (newsize > kcp->ackblock)
            {
                uint* acklist;
                uint newblock;

                newblock = newsize <= 8 ? 8 : _iceilpow2_(newsize);
                acklist = (uint*)ikcp_malloc((nuint)(newblock * sizeof(uint) * 2));

                if (acklist == null)
                {
                    assert(acklist != null);
                }

                if (kcp->acklist != null)
                {
                    uint x;
                    for (x = 0; x < kcp->ackcount; x++)
                    {
                        acklist[x * 2 + 0] = kcp->acklist[x * 2 + 0];
                        acklist[x * 2 + 1] = kcp->acklist[x * 2 + 1];
                    }

                    ikcp_free(kcp->acklist);
                }

                kcp->acklist = acklist;
                kcp->ackblock = newblock;
            }

            ptr = &kcp->acklist[kcp->ackcount * 2];
            ptr[0] = sn;
            ptr[1] = ts;
            kcp->ackcount++;
        }

        public static void ikcp_ack_get(IKCPCB* kcp, int p, uint* sn, uint* ts)
        {
            if (sn != null) sn[0] = kcp->acklist[p * 2 + 0];
            if (ts != null) ts[0] = kcp->acklist[p * 2 + 1];
        }

        public static void ikcp_parse_data(IKCPCB* kcp, IKCPSEG* newseg)
        {
            IQUEUEHEAD* p, prev;
            uint sn = newseg->sn;
            int repeat = 0;

            if (_itimediff(sn, kcp->rcv_nxt + kcp->rcv_wnd) >= 0 ||
                _itimediff(sn, kcp->rcv_nxt) < 0)
            {
                ikcp_segment_delete(kcp, newseg);
                return;
            }

            for (p = kcp->rcv_buf.prev; p != &kcp->rcv_buf; p = prev)
            {
                IKCPSEG* seg = iqueue_entry<IKCPSEG>(p);
                prev = p->prev;
                if (seg->sn == sn)
                {
                    repeat = 1;
                    break;
                }

                if (_itimediff(sn, seg->sn) > 0)
                {
                    break;
                }
            }

            if (repeat == 0)
            {
                iqueue_init(&newseg->node);
                iqueue_add(&newseg->node, p);
                kcp->nrcv_buf++;
            }
            else
            {
                ikcp_segment_delete(kcp, newseg);
            }

            while (!iqueue_is_empty(&kcp->rcv_buf))
            {
                IKCPSEG* seg = iqueue_entry<IKCPSEG>(kcp->rcv_buf.next);
                if (seg->sn == kcp->rcv_nxt && kcp->nrcv_que < kcp->rcv_wnd)
                {
                    iqueue_del(&seg->node);
                    kcp->nrcv_buf--;
                    iqueue_add_tail(&seg->node, &kcp->rcv_queue);
                    kcp->nrcv_que++;
                    kcp->rcv_nxt++;
                }
                else
                {
                    break;
                }
            }
        }

        public static int ikcp_input(IKCPCB* kcp, byte* data, long size)
        {
            uint prev_una = kcp->snd_una;
            uint maxack = 0, latest_ts = 0;
            int flag = 0;

            if (data == null || (int)size < (int)IKCP_OVERHEAD) return -1;

            while (true)
            {
                uint ts, sn, len, una, conv;
                ushort wnd;
                byte cmd, frg;
                IKCPSEG* seg;

                if (size < (int)IKCP_OVERHEAD) break;

                data = ikcp_decode32u(data, &conv);
                if (conv != kcp->conv) return -1;

                data = ikcp_decode8u(data, &cmd);
                data = ikcp_decode8u(data, &frg);
                data = ikcp_decode16u(data, &wnd);
                data = ikcp_decode32u(data, &ts);
                data = ikcp_decode32u(data, &sn);
                data = ikcp_decode32u(data, &una);
                data = ikcp_decode32u(data, &len);

                size -= IKCP_OVERHEAD;

                if ((long)size < (long)len || (int)len < 0) return -2;

                if (cmd != IKCP_CMD_PUSH && cmd != IKCP_CMD_ACK &&
                    cmd != IKCP_CMD_WASK && cmd != IKCP_CMD_WINS)
                    return -3;

                kcp->rmt_wnd = wnd;
                ikcp_parse_una(kcp, una);
                ikcp_shrink_buf(kcp);

                if (cmd == IKCP_CMD_ACK)
                {
                    if (_itimediff(kcp->current, ts) >= 0)
                    {
                        ikcp_update_ack(kcp, (int)_itimediff(kcp->current, ts));
                    }

                    ikcp_parse_ack(kcp, sn);
                    ikcp_shrink_buf(kcp);
                    if (flag == 0)
                    {
                        flag = 1;
                        maxack = sn;
                        latest_ts = ts;
                    }
                    else
                    {
                        if (_itimediff(sn, maxack) > 0)
                        {
                            maxack = sn;
                            latest_ts = ts;
                        }
                    }
                }
                else if (cmd == IKCP_CMD_PUSH)
                {
                    if (_itimediff(sn, kcp->rcv_nxt + kcp->rcv_wnd) < 0)
                    {
                        ikcp_ack_push(kcp, sn, ts);
                        if (_itimediff(sn, kcp->rcv_nxt) >= 0)
                        {
                            seg = ikcp_segment_new(kcp, (int)len);
                            seg->conv = conv;
                            seg->cmd = cmd;
                            seg->frg = frg;
                            seg->wnd = wnd;
                            seg->ts = ts;
                            seg->sn = sn;
                            seg->una = una;
                            seg->len = len;

                            if (len > 0)
                            {
                                memcpy(seg->data, data, (nuint)len);
                            }

                            ikcp_parse_data(kcp, seg);
                        }
                    }
                }
                else if (cmd == IKCP_CMD_WASK)
                {
                    kcp->probe |= IKCP_ASK_TELL;
                }
                else if (cmd == IKCP_CMD_WINS)
                {
                }
                else
                {
                    return -3;
                }

                data += len;
                size -= len;
            }

            if (flag != 0)
            {
                ikcp_parse_fastack(kcp, maxack, latest_ts);
            }

            if (_itimediff(kcp->snd_una, prev_una) > 0)
            {
                if (kcp->cwnd < kcp->rmt_wnd)
                {
                    uint mss = kcp->mss;
                    if (kcp->cwnd < kcp->ssthresh)
                    {
                        kcp->cwnd++;
                        kcp->incr += mss;
                    }
                    else
                    {
                        if (kcp->incr < mss) kcp->incr = mss;
                        kcp->incr += (mss * mss) / kcp->incr + (mss / 16);
                        if ((kcp->cwnd + 1) * mss <= kcp->incr)
                        {
                            kcp->cwnd = (kcp->incr + mss - 1) / ((mss > 0) ? mss : 1);
                        }
                    }

                    if (kcp->cwnd > kcp->rmt_wnd)
                    {
                        kcp->cwnd = kcp->rmt_wnd;
                        kcp->incr = kcp->rmt_wnd * mss;
                    }
                }
            }

            return 0;
        }

        public static byte* ikcp_encode_seg(byte* ptr, IKCPSEG* seg)
        {
            ptr = ikcp_encode32u(ptr, seg->conv);
            ptr = ikcp_encode8u(ptr, (byte)seg->cmd);
            ptr = ikcp_encode8u(ptr, (byte)seg->frg);
            ptr = ikcp_encode16u(ptr, (ushort)seg->wnd);
            ptr = ikcp_encode32u(ptr, seg->ts);
            ptr = ikcp_encode32u(ptr, seg->sn);
            ptr = ikcp_encode32u(ptr, seg->una);
            ptr = ikcp_encode32u(ptr, seg->len);
            return ptr;
        }

        public static int ikcp_wnd_unused(IKCPCB* kcp)
        {
            if (kcp->nrcv_que < kcp->rcv_wnd)
            {
                return (int)(kcp->rcv_wnd - kcp->nrcv_que);
            }

            return 0;
        }

        public static void ikcp_flush(IKCPCB* kcp, byte* buffer, byte[] destination, Action<byte[], int> output)
        {
            uint current = kcp->current;
            byte* ptr = buffer;
            int count, size, i;
            uint resent, cwnd;
            uint rtomin;
            IQUEUEHEAD* p;
            int change = 0;
            int lost = 0;
            IKCPSEG seg;

            if (kcp->updated == 0) return;

            seg.conv = kcp->conv;
            seg.cmd = IKCP_CMD_ACK;
            seg.frg = 0;
            seg.wnd = (uint)ikcp_wnd_unused(kcp);
            seg.una = kcp->rcv_nxt;
            seg.len = 0;
            seg.sn = 0;
            seg.ts = 0;

            count = (int)kcp->ackcount;
            for (i = 0; i < count; i++)
            {
                size = (int)(ptr - buffer);
                if (size + (int)IKCP_OVERHEAD > (int)kcp->mtu)
                {
                    ikcp_output(kcp, size, destination, output);
                    ptr = buffer;
                }

                ikcp_ack_get(kcp, i, &seg.sn, &seg.ts);
                ptr = ikcp_encode_seg(ptr, &seg);
            }

            kcp->ackcount = 0;

            if (kcp->rmt_wnd == 0)
            {
                if (kcp->probe_wait == 0)
                {
                    kcp->probe_wait = IKCP_PROBE_INIT;
                    kcp->ts_probe = kcp->current + kcp->probe_wait;
                }
                else
                {
                    if (_itimediff(kcp->current, kcp->ts_probe) >= 0)
                    {
                        if (kcp->probe_wait < IKCP_PROBE_INIT)
                            kcp->probe_wait = IKCP_PROBE_INIT;
                        kcp->probe_wait += kcp->probe_wait / 2;
                        if (kcp->probe_wait > IKCP_PROBE_LIMIT)
                            kcp->probe_wait = IKCP_PROBE_LIMIT;
                        kcp->ts_probe = kcp->current + kcp->probe_wait;
                        kcp->probe |= IKCP_ASK_SEND;
                    }
                }
            }
            else
            {
                kcp->ts_probe = 0;
                kcp->probe_wait = 0;
            }

            if ((kcp->probe & IKCP_ASK_SEND) != 0)
            {
                seg.cmd = IKCP_CMD_WASK;
                size = (int)(ptr - buffer);
                if (size + (int)IKCP_OVERHEAD > (int)kcp->mtu)
                {
                    ikcp_output(kcp, size, destination, output);
                    ptr = buffer;
                }

                ptr = ikcp_encode_seg(ptr, &seg);
            }

            if ((kcp->probe & IKCP_ASK_TELL) != 0)
            {
                seg.cmd = IKCP_CMD_WINS;
                size = (int)(ptr - buffer);
                if (size + (int)IKCP_OVERHEAD > (int)kcp->mtu)
                {
                    ikcp_output(kcp, size, destination, output);
                    ptr = buffer;
                }

                ptr = ikcp_encode_seg(ptr, &seg);
            }

            kcp->probe = 0;

            cwnd = _imin_(kcp->snd_wnd, kcp->rmt_wnd);
            if (kcp->nocwnd == 0) cwnd = _imin_(kcp->cwnd, cwnd);

            while (_itimediff(kcp->snd_nxt, kcp->snd_una + cwnd) < 0)
            {
                IKCPSEG* newseg;
                if (iqueue_is_empty(&kcp->snd_queue)) break;

                newseg = iqueue_entry<IKCPSEG>(kcp->snd_queue.next);

                iqueue_del(&newseg->node);
                iqueue_add_tail(&newseg->node, &kcp->snd_buf);
                kcp->nsnd_que--;
                kcp->nsnd_buf++;

                newseg->conv = kcp->conv;
                newseg->cmd = IKCP_CMD_PUSH;
                newseg->wnd = seg.wnd;
                newseg->ts = current;
                newseg->sn = kcp->snd_nxt++;
                newseg->una = kcp->rcv_nxt;
                newseg->resendts = current;
                newseg->rto = (uint)kcp->rx_rto;
                newseg->fastack = 0;
                newseg->xmit = 0;
            }

            resent = (kcp->fastresend > 0) ? (uint)kcp->fastresend : 0xffffffff;
            rtomin = (uint)((kcp->nodelay == 0) ? (kcp->rx_rto >> 3) : 0);

            for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = p->next)
            {
                IKCPSEG* segment = iqueue_entry<IKCPSEG>(p);
                int needsend = 0;
                if (segment->xmit == 0)
                {
                    needsend = 1;
                    segment->xmit++;
                    segment->rto = (uint)kcp->rx_rto;
                    segment->resendts = current + segment->rto + rtomin;
                }
                else if (_itimediff(current, segment->resendts) >= 0)
                {
                    needsend = 1;
                    segment->xmit++;
                    kcp->xmit++;
                    if (kcp->nodelay == 0)
                    {
                        segment->rto += _imax_(segment->rto, (uint)kcp->rx_rto);
                    }
                    else
                    {
                        int step = (kcp->nodelay < 2) ? ((int)(segment->rto)) : kcp->rx_rto;
                        segment->rto += (uint)(step / 2);
                    }

                    segment->resendts = current + segment->rto;
                    lost = 1;
                }
                else if (segment->fastack >= resent)
                {
                    if ((int)segment->xmit <= kcp->fastlimit ||
                        kcp->fastlimit <= 0)
                    {
                        needsend = 1;
                        segment->xmit++;
                        segment->fastack = 0;
                        segment->resendts = current + segment->rto;
                        change++;
                    }
                }

                if (needsend != 0)
                {
                    int need;
                    segment->ts = current;
                    segment->wnd = seg.wnd;
                    segment->una = kcp->rcv_nxt;

                    size = (int)(ptr - buffer);
                    need = (int)(IKCP_OVERHEAD + segment->len);

                    if (size + need > (int)kcp->mtu)
                    {
                        ikcp_output(kcp, size, destination, output);
                        ptr = buffer;
                    }

                    ptr = ikcp_encode_seg(ptr, segment);

                    if (segment->len > 0)
                    {
                        memcpy(ptr, segment->data, (nuint)segment->len);
                        ptr += segment->len;
                    }

                    if (segment->xmit >= kcp->dead_link)
                    {
                        kcp->state = unchecked((uint)(-1));
                    }
                }
            }

            size = (int)(ptr - buffer);
            if (size > 0)
            {
                ikcp_output(kcp, size, destination, output);
            }

            if (change != 0)
            {
                uint inflight = kcp->snd_nxt - kcp->snd_una;
                kcp->ssthresh = inflight / 2;
                if (kcp->ssthresh < IKCP_THRESH_MIN)
                    kcp->ssthresh = IKCP_THRESH_MIN;
                kcp->cwnd = kcp->ssthresh + resent;
                kcp->incr = kcp->cwnd * kcp->mss;
            }

            if (lost != 0)
            {
                kcp->ssthresh = cwnd / 2;
                if (kcp->ssthresh < IKCP_THRESH_MIN)
                    kcp->ssthresh = IKCP_THRESH_MIN;
                kcp->cwnd = 1;
                kcp->incr = kcp->mss;
            }

            if (kcp->cwnd < 1)
            {
                kcp->cwnd = 1;
                kcp->incr = kcp->mss;
            }
        }

        public static void ikcp_update(IKCPCB* kcp, uint current, byte* buffer, byte[] destination, Action<byte[], int> output)
        {
            int slap;

            kcp->current = current;

            if (kcp->updated == 0)
            {
                kcp->updated = 1;
                kcp->ts_flush = kcp->current;
            }

            slap = (int)_itimediff(kcp->current, kcp->ts_flush);

            if (slap >= 10000 || slap < -10000)
            {
                kcp->ts_flush = kcp->current;
                slap = 0;
            }

            if (slap >= 0)
            {
                kcp->ts_flush += kcp->interval;
                if (_itimediff(kcp->current, kcp->ts_flush) >= 0)
                {
                    kcp->ts_flush = kcp->current + kcp->interval;
                }

                ikcp_flush(kcp, buffer, destination, output);
            }
        }

        public static uint ikcp_check(IKCPCB* kcp, uint current)
        {
            uint ts_flush = kcp->ts_flush;
            int tm_flush = 0x7fffffff;
            int tm_packet = 0x7fffffff;
            uint minimal = 0;
            IQUEUEHEAD* p;

            if (kcp->updated == 0)
            {
                return current;
            }

            if (_itimediff(current, ts_flush) >= 10000 ||
                _itimediff(current, ts_flush) < -10000)
            {
                ts_flush = current;
            }

            if (_itimediff(current, ts_flush) >= 0)
            {
                return current;
            }

            tm_flush = (int)_itimediff(ts_flush, current);

            for (p = kcp->snd_buf.next; p != &kcp->snd_buf; p = p->next)
            {
                IKCPSEG* seg = iqueue_entry<IKCPSEG>(p);
                int diff = (int)_itimediff(seg->resendts, current);
                if (diff <= 0)
                {
                    return current;
                }

                if (diff < tm_packet) tm_packet = diff;
            }

            minimal = (uint)(tm_packet < tm_flush ? tm_packet : tm_flush);
            if (minimal >= kcp->interval) minimal = kcp->interval;

            return current + minimal;
        }

        public static int ikcp_setmtu(IKCPCB* kcp, int mtu, ref byte[] buffer)
        {
            if (mtu < 50 || mtu < (int)IKCP_OVERHEAD)
                return -1;
            buffer = new byte[(mtu + IKCP_OVERHEAD) * 3];
            if (buffer == null)
                return -2;
            kcp->mtu = (uint)mtu;
            kcp->mss = kcp->mtu - IKCP_OVERHEAD;
            return 0;
        }

        public static int ikcp_interval(IKCPCB* kcp, int interval)
        {
            if (interval > 5000) interval = 5000;
            else if (interval < 10) interval = 10;
            kcp->interval = (uint)interval;
            return 0;
        }

        public static int ikcp_nodelay(IKCPCB* kcp, int nodelay, int interval, int resend, int nc)
        {
            if (nodelay >= 0)
            {
                kcp->nodelay = (uint)nodelay;
                if (nodelay != 0)
                {
                    kcp->rx_minrto = (int)IKCP_RTO_NDL;
                }
                else
                {
                    kcp->rx_minrto = (int)IKCP_RTO_MIN;
                }
            }

            if (interval >= 0)
            {
                if (interval > 5000) interval = 5000;
                else if (interval < 10) interval = 10;
                kcp->interval = (uint)interval;
            }

            if (resend >= 0)
            {
                kcp->fastresend = resend;
            }

            if (nc >= 0)
            {
                kcp->nocwnd = nc;
            }

            return 0;
        }

        public static int ikcp_wndsize(IKCPCB* kcp, int sndwnd, int rcvwnd)
        {
            if (kcp != null)
            {
                if (sndwnd > 0)
                {
                    kcp->snd_wnd = (uint)sndwnd;
                }

                if (rcvwnd > 0)
                {
                    kcp->rcv_wnd = _imax_((uint)rcvwnd, IKCP_WND_RCV);
                }
            }

            return 0;
        }

        public static int ikcp_waitsnd(IKCPCB* kcp)
        {
            return (int)(kcp->nsnd_buf + kcp->nsnd_que);
        }

        public static uint ikcp_getconv(void* ptr)
        {
            uint conv;
            ikcp_decode32u((byte*)ptr, &conv);
            return conv;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct IQUEUEHEAD
    {
        public IQUEUEHEAD* next;
        public IQUEUEHEAD* prev;
    }

    internal static unsafe partial class Kcp
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_init(IQUEUEHEAD* head)
        {
            head->next = head;
            head->prev = head;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* iqueue_entry<T>(IQUEUEHEAD* ptr) where T : unmanaged => ((T*)(((byte*)((T*)ptr))));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_add(IQUEUEHEAD* node, IQUEUEHEAD* head)
        {
            node->prev = head;
            node->next = head->next;
            head->next->prev = node;
            head->next = node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_add_tail(IQUEUEHEAD* node, IQUEUEHEAD* head)
        {
            node->prev = head->prev;
            node->next = head;
            head->prev->next = node;
            head->prev = node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_del(IQUEUEHEAD* entry)
        {
            entry->next->prev = entry->prev;
            entry->prev->next = entry->next;
            entry->next = (IQUEUEHEAD*)0;
            entry->prev = (IQUEUEHEAD*)0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void iqueue_del_init(IQUEUEHEAD* entry)
        {
            iqueue_del(entry);
            iqueue_init(entry);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool iqueue_is_empty(IQUEUEHEAD* entry) => entry == entry->next;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct IKCPSEG
    {
        public IQUEUEHEAD node;
        public uint conv;
        public uint cmd;
        public uint frg;
        public uint wnd;
        public uint ts;
        public uint sn;
        public uint una;
        public uint len;
        public uint resendts;
        public uint rto;
        public uint fastack;
        public uint xmit;
        public fixed byte data[1];
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct IKCPCB
    {
        public uint conv, mtu, mss, state;
        public uint snd_una, snd_nxt, rcv_nxt;
        public uint ts_recent, ts_lastack, ssthresh;
        public int rx_rttval, rx_srtt, rx_rto, rx_minrto;
        public uint snd_wnd, rcv_wnd, rmt_wnd, cwnd, probe;
        public uint current, interval, ts_flush, xmit;
        public uint nrcv_buf, nsnd_buf;
        public uint nrcv_que, nsnd_que;
        public uint nodelay, updated;
        public uint ts_probe, probe_wait;
        public uint dead_link, incr;
        public IQUEUEHEAD snd_queue;
        public IQUEUEHEAD rcv_queue;
        public IQUEUEHEAD snd_buf;
        public IQUEUEHEAD rcv_buf;
        public uint* acklist;
        public uint ackcount;
        public uint ackblock;
        public int fastresend;
        public int fastlimit;
        public int nocwnd, stream;
    }

    internal static unsafe partial class Kcp
    {
        public static void* malloc(nuint size)
        {
            return (void*)Marshal.AllocHGlobal((nint)size);
        }

        public static void free(void* memory)
        {
            Marshal.FreeHGlobal((nint)memory);
        }

        public static void memcpy(void* dst, void* src, nuint length)
        {
            Buffer.MemoryCopy(src, dst, length, length);
        }

        [Conditional("DEBUG")]
        public static void assert(bool condition) => Debug.Assert(condition);
    }
}