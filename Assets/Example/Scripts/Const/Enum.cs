// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:31
// // # Recently: 2025-04-20 19:04:31
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;

namespace Runtime
{
    public enum Attribute
    {
        MoveSpeed,
        JumpForce,
        JumpGrace,
        JumpInput,
        JumpCount,
        DashInput,
        DashCount,
        DashSpeed,
        DashFrame,
        WaitFrame,
    }

    [Flags]
    public enum StateType
    {
        None,
        Stop = 1 << 0,
        Jump = 1 << 1,
        Jumping = 1 << 2,
        Jumped = 1 << 3,
        Climb = 1 << 4,
        Grab = 1 << 5,
        Ground = 1 << 6,
        Wall = 1 << 8,
        Dash = 1 << 9,
        Crash = 1 << 10,
    }

    public static class StateConst
    {
        public const int Hop = 866648894;
        public const int Idle = 2081823275;
        public const int Walk = 765711723;
        public const int Jump = 125937960;
        public const int Dash = 995914302;
        public const int Grab = -1347921612;
        public const int Crash = 371843035;
    }
}