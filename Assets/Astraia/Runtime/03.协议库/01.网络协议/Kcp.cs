// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-11-30 17:11:16
// # Recently: 2024-12-22 20:12:11
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    internal class Kcp
    {
        private struct AckItem
        {
            public readonly uint sn;
            public readonly uint ts;

            public AckItem(uint sn, uint ts)
            {
                this.sn = sn;
                this.ts = ts;
            }
        }
        public const int PING_INTERVAL = 1000;
        public const int METADATA_SIZE = sizeof(byte) + sizeof(int);
        public const int FRG_MAX = byte.MaxValue; // 最大分片数。KCP将分片编号编码为字节类型，因此最大分片数为255。
        public const int MTU_DEF = 1200;          // 默认的最大传输单元（MTU）。设置为1200以适应所有情况。
        public const int RTO_NDL = 30;            // 无延迟情况下的最小重传超时。用于在无延迟模式下，减少数据包的重传等待时间。
        public const int RTO_MIN = 100;           // 正常情况下的最小重传超时。确保即使网络环境很好，也不会频繁地重传数据包。
        public const int RTO_DEF = 200;           // 默认的重传超时设置。用于在一般网络环境下设置初始重传等待时间。
        public const int RTO_MAX = 60000;         // 最大重传超时。用于避免重传超时无限增长，设置一个合理的上限。
        public const int CMD_PUSH = 81;           // 推送数据的命令。用于发送实际的数据包。
        public const int CMD_ACK = 82;            // 确认收到的命令。用于确认收到数据包，通知发送方。
        public const int CMD_W_ASK = 83;          // 窗口探测（询问）命令。用于探测接收方的窗口大小。
        public const int CMD_W_INS = 84;          // 窗口大小（告诉/插入）命令。用于告知发送方当前的接收窗口大小。
        public const int ASK_SEND = 1;            // 表示需要发送窗口探测命令（CMD_W_ASK）。
        public const int ASK_TELL = 2;            // 表示需要发送窗口大小命令（CMD_W_INS）。
        public const int WND_SND = 32;            // 默认的发送窗口大小。表示可以在发送窗口中未确认的数据包数。
        public const int WND_RCV = 128;           // 默认的接收窗口大小。表示可以在接收窗口中接收的最大数据包数，必须大于或等于最大分片大小。
        public const int INTERVAL = 100;          // KCP的更新间隔。表示KCP内部定时器的触发间隔。
        public const int OVERHEAD = 24;           // 头部开销。表示每个KCP数据包的头部大小。
        public const int THRESH_DEF = 2;          // 拥塞窗口初始阈值。用于设置拥塞窗口大小的初始值。
        public const int THRESH_MIN = 2;          // 拥塞窗口最小阈值。用于设置拥塞窗口大小的下限。
        public const int PROBE_DEF = 7000;        // 窗口探测的初始间隔。表示窗口探测的初始时间间隔。
        public const int PROBE_LIM = 120000;      // 窗口探测的最大间隔。表示窗口探测的最大时间间隔。
        public const int FAST_LIM = 5;            // 快速确认的阈值。表示在触发快速重传前，需要接收到的重复确认次数。
        public const int TIME_OUT = 10000;        // 长时间未收到数据包。网络数据超时时间。
        public const int DEAD_LINK = 20;          // 认为连接中断的最大重传次数。在尝试重传20次后，如果仍然未收到确认，将认为连接中断。
        
        public int state;                         // 当前连接的状态，表示连接的不同阶段（例如连接中、已连接、断开）。
        public uint dead_link;                    // 重传次数的最大值，超过此值认为连接断开。
        
        private int rx_rto;                       // 当前的重传超时值，根据网络状况动态调整。
        private int rx_rto_min;                   // 最小重传超时，防止RTO过小导致频繁重传。
        private int rx_rtt_val;                   // RTT的平均偏差，用于测量RTT的抖动，影响RTO计算。
        private int rx_rtt_avg;                   // 平滑的RTT值，是RTT的加权平均值，影响RTO计算。
        
        private uint mtu;                         // 最大传输单元，决定了每个数据包的最大字节数。
        private uint mss;                         // 最大分段大小，计算方式为 MTU - OVERHEAD（头部开销）。
        private uint snd_una;                     // 发送但未确认的最小序号。比如，snd_una为9，表示序号8已经确认，序号9和10已经发送但未确认。
        private uint snd_nxt;                     // 发送序号的计数器，不断增长，用于生成新的序列号。
        private uint rcv_nxt;                     // 接收序号的计数器，不断增长，用于确认接收到的序列号。
        private uint ss_thresh;                   // 慢启动阈值，用于控制拥塞窗口的增长速率。
        private uint snd_wnd;                     // 发送窗口大小，表示可以发送但未确认的数据包数。
        private uint rcv_wnd;                     // 接收窗口大小，表示可以接收的数据包数。
        private uint rmt_wnd;                     // 远端窗口大小，表示远端接收窗口的大小。
        private uint cmd_wnd;                     // 拥塞窗口大小，用于控制发送速率，防止网络拥塞。
        private uint probe;                       // 探测标志位，用于探测远端窗口大小。
        private uint interval;                    // KCP内部状态更新的时间间隔。
        private uint ts_flush;                    // 上次刷新时间戳，用于定时刷新状态。
        private bool updated;                     // 是否已更新的标志位，用于标记KCP状态是否已更新。
        private uint ts_probe;                    // 探测时间戳，用于窗口探测。
        private uint probe_wait;                  // 探测等待时间，用于控制探测间隔。
        private uint incr;                        // 增量值，用于拥塞控制计算。
        private uint current;                     // 当前时间，KCP内部使用的时间戳。
        private uint fast_resend;                 // 快速重传的设置，用于提高重传效率。
        private uint no_delay;                    // 无延迟模式的设置，用于减少延迟但可能增加网络抖动。
        private bool noc_wnd;                     // 是否启用拥塞控制，启用后会严重限制发送和接收窗口大小。
        private byte[] buffer;                    // MTU可以在运行时改变，从而调整缓冲区的大小。
        private readonly uint conv;               // 会话标识符，用于唯一标识一个会话，以区分不同的会话数据。
        private readonly Action<byte[], int> output;
        private readonly List<AckItem> ackList = new List<AckItem>(16);
        
        public readonly List<Segment> sendBuffer = new List<Segment>(16);
        public readonly List<Segment> receiveBuffer = new List<Segment>(16);
        public readonly Queue<Segment> sendQueue = new Queue<Segment>(16);
        public readonly Queue<Segment> receiveQueue = new Queue<Segment>(16);

        public Kcp(uint conv, Action<byte[], int> output)
        {
            this.conv = conv;
            this.output = output;
            snd_wnd = WND_SND;
            rcv_wnd = WND_RCV;
            rmt_wnd = WND_RCV;
            mtu = MTU_DEF;
            mss = mtu - OVERHEAD;
            rx_rto = RTO_DEF;
            rx_rto_min = RTO_MIN;
            interval = INTERVAL;
            ts_flush = INTERVAL;
            ss_thresh = THRESH_DEF;
            dead_link = DEAD_LINK;
            buffer = new byte[(mtu + OVERHEAD) * 3];
        }
        
        public int Receive(byte[] buffer, int len)
        {
            if (len < 0)
            {
                throw new NotSupportedException("Receive is peek for negative len is not supported!");
            }

            if (receiveQueue.Count == 0)
            {
                return -1;
            }

            int peekSize = PeekSize();

            if (peekSize < 0)
            {
                return -2;
            }

            if (peekSize > len)
            {
                return -3;
            }

            len = 0;
            var offset = 0;
            var recover = receiveQueue.Count >= rcv_wnd;

            while (receiveQueue.Count > 0)
            {
                var seg = receiveQueue.Dequeue();
                Buffer.BlockCopy(seg.data.GetBuffer(), 0, buffer, offset, (int)seg.data.Position);
                
                offset += (int)seg.data.Position;
                len += (int)seg.data.Position;
                var fragment = seg.frg;
                Segment.Enqueue(seg);

                if (fragment == 0)
                {
                    break;
                }
            }


            int removed = 0;
            foreach (var seg in receiveBuffer)
            {
                if (seg.sn == rcv_nxt && receiveQueue.Count < rcv_wnd)
                {
                    ++removed;
                    receiveQueue.Enqueue(seg);
                    rcv_nxt++;
                }
                else
                {
                    break;
                }
            }

            receiveBuffer.RemoveRange(0, removed);
            
            if (receiveQueue.Count < rcv_wnd && recover)
            {
                probe |= ASK_TELL;
            }

            return len;
        }


        public int PeekSize()
        {
            int length = 0;
            if (receiveQueue.Count == 0)
            {
                return -1;
            }
            
            var seq = receiveQueue.Peek();
            if (seq.frg == 0)
            {
                return (int)seq.data.Position;
            }

            if (receiveQueue.Count < seq.frg + 1)
            {
                return -1;
            }
            
            foreach (var seg in receiveQueue)
            {
                length += (int)seg.data.Position;
                if (seg.frg == 0)
                {
                    break;
                }
            }

            return length;
        }


        public int Send(byte[] buffer, int offset, int len)
        {
            int count;

            if (len < 0)
            {
                return -1;
            }

            if (len <= mss)
            {
                count = 1;
            }
            else
            {
                count = (int)((len + mss - 1) / mss);
            }

            if (count > FRG_MAX)
            {
                throw new Exception($"Send len={len} requires {count} fragments, but kcp can only handle up to {FRG_MAX} fragments.");
            }

            if (count >= rcv_wnd)
            {
                return -2;
            }

            if (count == 0)
            {
                count = 1;
            }

            for (int i = 0; i < count; i++)
            {
                int size = len > (int)mss ? (int)mss : len;
                var seg = Segment.Dequeue();
                seg.Reset();

                if (len > 0)
                {
                    seg.data.Write(buffer, offset, size);
                }

                seg.frg = (uint)(count - i - 1);
                sendQueue.Enqueue(seg);
                offset += size;
                len -= size;
            }

            return 0;
        }

        private void UpdateAck(int rtt)
        {
            if (rx_rtt_avg == 0)
            {
                rx_rtt_avg = rtt;
                rx_rtt_val = rtt / 2;
            }
            else
            {
                int delta = rtt - rx_rtt_avg;
                if (delta < 0) delta = -delta;
                rx_rtt_val = (3 * rx_rtt_val + delta) / 4;
                rx_rtt_avg = (7 * rx_rtt_avg + rtt) / 8;
                if (rx_rtt_avg < 1) rx_rtt_avg = 1;
            }

            int rto = rx_rtt_avg + Math.Max((int)interval, 4 * rx_rtt_val);

            if (rto < rx_rto_min)
            {
                rto = rx_rto_min;
            }
            else if (rto > RTO_MAX)
            {
                rto = RTO_MAX;
            }

            rx_rto = rto;
        }

        private void ShrinkBuf()
        {
            if (sendBuffer.Count > 0)
            {
                var seg = sendBuffer[0];
                snd_una = seg.sn;
            }
            else
            {
                snd_una = snd_nxt;
            }
        }

        private void ParseAck(uint sn)
        {
            if (Utils.Compare(sn, snd_una) < 0 || Utils.Compare(sn, snd_nxt) >= 0)
            {
                return;
            }

            for (int i = 0; i < sendBuffer.Count; ++i)
            {
                var seg = sendBuffer[i];
                if (sn == seg.sn)
                {
                    sendBuffer.RemoveAt(i);
                    Segment.Enqueue(seg);
                    break;
                }

                if (Utils.Compare(sn, seg.sn) < 0)
                {
                    break;
                }
            }
        }


        private void ParseUna(uint una)
        {
            int removed = 0;
            foreach (Segment seg in sendBuffer)
            {
                if (seg.sn < una)
                {
                    ++removed;
                    Segment.Enqueue(seg);
                }
                else
                {
                    break;
                }
            }

            sendBuffer.RemoveRange(0, removed);
        }


        private void ParseFastAck(uint sn, uint ts)
        {
            if (sn < snd_una)
            {
                return;
            }

            if (sn >= snd_nxt)
            {
                return;
            }

            foreach (var seg in sendBuffer)
            {
                if (sn < seg.sn)
                {
                    break;
                }

                if (sn != seg.sn)
                {
                    seg.fast_ack++;
                }
            }
        }


        private void ParseData(Segment segment)
        {
            var sn = segment.sn;
            if (Utils.Compare(sn, rcv_nxt + rcv_wnd) >= 0 || Utils.Compare(sn, rcv_nxt) < 0)
            {
                Segment.Enqueue(segment);
                return;
            }

            InsertSegmentInReceiveBuffer(segment);
            MoveReceiveBufferReadySegmentsToQueue();
        }


        private void InsertSegmentInReceiveBuffer(Segment segment)
        {
            var repeat = false;
            int i;
            for (i = receiveBuffer.Count - 1; i >= 0; i--)
            {
                var seg = receiveBuffer[i];
                if (seg.sn == segment.sn)
                {
                    repeat = true;
                    break;
                }

                if (Utils.Compare(segment.sn, seg.sn) > 0)
                {
                    break;
                }
            }
            
            if (!repeat)
            {
                receiveBuffer.Insert(i + 1, segment);
            }

            else
            {
                Segment.Enqueue(segment);
            }
        }


        private void MoveReceiveBufferReadySegmentsToQueue()
        {
            var removed = 0;
            foreach (var seg in receiveBuffer)
            {
                if (seg.sn == rcv_nxt && receiveQueue.Count < rcv_wnd)
                {
                    ++removed;
                    receiveQueue.Enqueue(seg);

                    rcv_nxt++;
                }
                else
                {
                    break;
                }
            }

            receiveBuffer.RemoveRange(0, removed);
        }


        public int Input(byte[] data, int offset, int size)
        {
            var flag = 0;
            var prev_una = snd_una;
            uint max_ack = 0;
            uint latest_ts = 0;
            
            if (data == null || size < OVERHEAD)
            {
                return -1;
            }

            while (true)
            {
                if (size < OVERHEAD)
                {
                    break;
                }

                offset += Utils.Decode32U(data, offset, out uint conv_);
                if (conv_ != conv) return -1;
                offset += Utils.Decode8U(data, offset, out byte cmd);
                offset += Utils.Decode8U(data, offset, out byte frg);
                offset += Utils.Decode16U(data, offset, out ushort wnd);
                offset += Utils.Decode32U(data, offset, out uint ts);
                offset += Utils.Decode32U(data, offset, out uint sn);
                offset += Utils.Decode32U(data, offset, out uint una);
                offset += Utils.Decode32U(data, offset, out uint length);
                size -= OVERHEAD;

                if (size < length)
                {
                    return -2;
                }

                if (cmd != CMD_PUSH && cmd != CMD_ACK && cmd != CMD_W_ASK && cmd != CMD_W_INS)
                {
                    return -3;
                }

                rmt_wnd = wnd;
                ParseUna(una);
                ShrinkBuf();

                if (cmd == CMD_ACK)
                {
                    if (Utils.Compare(current, ts) >= 0)
                    {
                        UpdateAck(Utils.Compare(current, ts));
                    }

                    ParseAck(sn);
                    ShrinkBuf();
                    if (flag == 0)
                    {
                        flag = 1;
                        max_ack = sn;
                        latest_ts = ts;
                    }
                    else
                    {
                        if (Utils.Compare(sn, max_ack) > 0)
                        {
                            max_ack = sn;
                            latest_ts = ts;
                        }
                    }
                }
                else if (cmd == CMD_PUSH)
                {
                    if (Utils.Compare(sn, rcv_nxt + rcv_wnd) < 0)
                    {
                        ackList.Add(new AckItem(sn, ts));
                        if (Utils.Compare(sn, rcv_nxt) >= 0)
                        {
                            var seg = Segment.Dequeue();
                            seg.Reset();
                            seg.conv = conv_;
                            seg.cmd = cmd;
                            seg.frg = frg;
                            seg.wnd = wnd;
                            seg.ts = ts;
                            seg.sn = sn;
                            seg.una = una;
                            if (length > 0)
                            {
                                seg.data.Write(data, offset, (int)length);
                            }

                            ParseData(seg);
                        }
                    }
                }
                else if (cmd == CMD_W_ASK)
                {
                    probe |= ASK_TELL;
                }

                offset += (int)length;
                size -= (int)length;
            }

            if (flag != 0)
            {
                ParseFastAck(max_ack, latest_ts);
            }

            if (Utils.Compare(snd_una, prev_una) > 0)
            {
                if (cmd_wnd < rmt_wnd)
                {
                    if (cmd_wnd < ss_thresh)
                    {
                        cmd_wnd++;
                        incr += mss;
                    }
                    else
                    {
                        if (incr < mss)
                        {
                            incr = mss;
                        }

                        incr += mss * mss / incr + mss / 16;
                        if ((cmd_wnd + 1) * mss <= incr)
                        {
                            cmd_wnd = (incr + mss - 1) / (mss > 0 ? mss : 1);
                        }
                    }

                    if (cmd_wnd > rmt_wnd)
                    {
                        cmd_wnd = rmt_wnd;
                        incr = rmt_wnd * mss;
                    }
                }
            }

            return 0;
        }

        private uint WndUnused()
        {
            if (receiveQueue.Count < rcv_wnd)
            {
                return rcv_wnd - (uint)receiveQueue.Count;
            }

            return 0;
        }

        private void MakeSpace(ref int size, int space)
        {
            if (size + space > mtu)
            {
                output(buffer, size);
                size = 0;
            }
        }


        private void FlushBuffer(int size)
        {
            if (size > 0)
            {
                output(buffer, size);
            }
        }

        private void Flush()
        {
            var size = 0;
            var lost = false;

            if (!updated)
            {
                return;
            }
            
            var seg = Segment.Dequeue();
            seg.Reset();
            seg.conv = conv;
            seg.cmd = CMD_ACK;
            seg.wnd = WndUnused();
            seg.una = rcv_nxt;

            foreach (var ack in ackList)
            {
                MakeSpace(ref size, OVERHEAD);
                seg.sn = ack.sn;
                seg.ts = ack.ts;
                size += seg.Encode(buffer, size);
            }

            ackList.Clear();

            if (rmt_wnd == 0)
            {
                if (probe_wait == 0)
                {
                    probe_wait = PROBE_DEF;
                    ts_probe = current + probe_wait;
                }
                else
                {
                    if (Utils.Compare(current, ts_probe) >= 0)
                    {
                        if (probe_wait < PROBE_DEF)
                        {
                            probe_wait = PROBE_DEF;
                        }

                        probe_wait += probe_wait / 2;
                        if (probe_wait > PROBE_LIM)
                        {
                            probe_wait = PROBE_LIM;
                        }

                        ts_probe = current + probe_wait;
                        probe |= ASK_SEND;
                    }
                }
            }
            else
            {
                ts_probe = 0;
                probe_wait = 0;
            }


            if ((probe & ASK_SEND) != 0)
            {
                seg.cmd = CMD_W_ASK;
                MakeSpace(ref size, OVERHEAD);
                size += seg.Encode(buffer, size);
            }


            if ((probe & ASK_TELL) != 0)
            {
                seg.cmd = CMD_W_INS;
                MakeSpace(ref size, OVERHEAD);
                size += seg.Encode(buffer, size);
            }

            probe = 0;
            uint c_wnd_ = Math.Min(snd_wnd, rmt_wnd);

            if (!noc_wnd)
            {
                c_wnd_ = Math.Min(cmd_wnd, c_wnd_);
            }


            while (Utils.Compare(snd_nxt, snd_una + c_wnd_) < 0)
            {
                if (sendQueue.Count == 0)
                {
                    break;
                }

                var segment = sendQueue.Dequeue();
                segment.conv = conv;
                segment.cmd = CMD_PUSH;
                segment.wnd = seg.wnd;
                segment.ts = current;
                segment.sn = snd_nxt;
                snd_nxt += 1;
                segment.una = rcv_nxt;
                segment.rsd_ts = current;
                segment.rto = rx_rto;
                segment.fast_ack = 0;
                segment.rsd_c = 0;
                sendBuffer.Add(segment);
            }

            var resent = fast_resend > 0 ? fast_resend : 0xffffffff;
            var rto_min = no_delay == 0 ? (uint)rx_rto >> 3 : 0;

            int change = 0;
            foreach (var segment in sendBuffer)
            {
                var needSend = false;
                if (segment.rsd_c == 0)
                {
                    needSend = true;
                    segment.rsd_c++;
                    segment.rto = rx_rto;
                    segment.rsd_ts = current + (uint)segment.rto + rto_min;
                }

                else if (Utils.Compare(current, segment.rsd_ts) >= 0)
                {
                    needSend = true;
                    segment.rsd_c++;
                    if (no_delay == 0)
                    {
                        segment.rto += Math.Max(segment.rto, rx_rto);
                    }
                    else
                    {
                        int step = no_delay < 2 ? segment.rto : rx_rto;
                        segment.rto += step / 2;
                    }

                    segment.rsd_ts = current + (uint)segment.rto;
                    lost = true;
                }

                else if (segment.fast_ack >= resent)
                {
                    if (segment.rsd_c <= FAST_LIM || FAST_LIM <= 0)
                    {
                        needSend = true;
                        segment.rsd_c++;
                        segment.fast_ack = 0;
                        segment.rsd_ts = current + (uint)segment.rto;
                        change++;
                    }
                }

                if (needSend)
                {
                    segment.ts = current;
                    segment.wnd = seg.wnd;
                    segment.una = rcv_nxt;

                    var need = OVERHEAD + (int)segment.data.Position;
                    MakeSpace(ref size, need);

                    size += segment.Encode(buffer, size);

                    if (segment.data.Position > 0)
                    {
                        Buffer.BlockCopy(segment.data.GetBuffer(), 0, buffer, size, (int)segment.data.Position);
                        size += (int)segment.data.Position;
                    }


                    if (segment.rsd_c >= dead_link)
                    {
                        state = -1;
                    }
                }
            }
            
            Segment.Enqueue(seg);
            FlushBuffer(size);

            if (change > 0)
            {
                var inflight = snd_nxt - snd_una;
                ss_thresh = inflight / 2;
                if (ss_thresh < THRESH_MIN)
                {
                    ss_thresh = THRESH_MIN;
                }

                cmd_wnd = ss_thresh + resent;
                incr = cmd_wnd * mss;
            }

            if (lost)
            {
                ss_thresh = c_wnd_ / 2;
                if (ss_thresh < THRESH_MIN)
                {
                    ss_thresh = THRESH_MIN;
                }

                cmd_wnd = 1;
                incr = mss;
            }

            if (cmd_wnd < 1)
            {
                cmd_wnd = 1;
                incr = mss;
            }
        }


        public void Update(uint currentTime)
        {
            current = currentTime;
            
            if (!updated)
            {
                updated = true;
                ts_flush = current;
            }

            int slap = Utils.Compare(current, ts_flush);
            if (slap >= 10000 || slap < -10000)
            {
                ts_flush = current;
                slap = 0;
            }

            if (slap >= 0)
            {
                ts_flush += interval;
                if (current >= ts_flush)
                {
                    ts_flush = current + interval;
                }

                Flush();
            }
        }

        public void SetMtu(uint mtu)
        {
            if (mtu < 50)
            {
                throw new ArgumentException("MTU must be higher than 50 and higher than OVERHEAD");
            }

            buffer = new byte[(mtu + OVERHEAD) * 3];
            this.mtu = mtu;
            mss = mtu - OVERHEAD;
        }

        public void SetNoDelay(uint no_delay, uint interval = INTERVAL, uint resend = 0, bool noc_wnd = false)
        {
            this.no_delay = no_delay;
            rx_rto_min = no_delay != 0 ? RTO_NDL : RTO_MIN;

            if (interval > 5000)
            {
                interval = 5000;
            }
            else if (interval < 10)
            {
                interval = 10;
            }

            this.interval = interval;
            fast_resend = resend;
            this.noc_wnd = noc_wnd;
        }

        public void SetWindowSize(uint sendWindow, uint receiveWindow)
        {
            if (sendWindow > 0)
            {
                snd_wnd = sendWindow;
            }

            if (receiveWindow > 0)
            {
                rcv_wnd = Math.Max(receiveWindow, WND_RCV);
            }
        }
        
        public static int ReliableSize(int mtu, uint rcv_wnd)
        {
            return (mtu - OVERHEAD - METADATA_SIZE) * ((int)Math.Min(rcv_wnd, FRG_MAX) - 1) - 1;
        }

        public static int UnreliableSize(int mtu)
        {
            return mtu - METADATA_SIZE - 1;
        }
    }
}