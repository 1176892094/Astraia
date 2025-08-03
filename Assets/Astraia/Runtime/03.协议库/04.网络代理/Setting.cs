// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-08 19:01:30
// # Recently: 2025-01-08 20:01:58
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using Astraia;

namespace Astraia
{
    [Serializable]
    internal struct Setting
    {
        public uint MaxUnit;
        public uint Timeout;
        public uint Interval;
        public uint DeadLink;
        public uint FastResend;
        public uint SendWindow;
        public uint ReceiveWindow;
        public bool NoDelay;
        public bool DualMode;
        public bool Congestion;

        public Setting(
            uint MaxUnit = Kcp.IKCP_MTU_DEF,
            uint Timeout = 10000,
            uint Interval = 10,
            uint DeadLink = Kcp.IKCP_DEADLINK,
            uint FastResend = 0,
            uint SendWindow = Kcp.IKCP_WND_SND,
            uint ReceiveWindow = Kcp.IKCP_WND_RCV,
            bool NoDelay = true,
            bool DualMode = true,
            bool Congestion = false)
        {
            this.MaxUnit = MaxUnit;
            this.Timeout = Timeout;
            this.Interval = Interval;
            this.DeadLink = DeadLink;
            this.FastResend = FastResend;
            this.SendWindow = SendWindow;
            this.ReceiveWindow = ReceiveWindow;
            this.NoDelay = NoDelay;
            this.DualMode = DualMode;
            this.Congestion = Congestion;
        }
    }
}