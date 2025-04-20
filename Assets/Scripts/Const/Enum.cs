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

namespace Const
{
    public enum Attribute
    {
        Horizontal,
        Vertical,
        MoveSpeed,
        JumpForce,
        JumpInput,
        JumpCount,
    }

    [Flags]
    public enum StateType
    {
        None,
        Stop = 1 << 0,
        Jump = 1 << 1,
        Fly = 1 << 2,
        Fall = 1 << 3,
        Grab = 1 << 4,
        Ground = 1 << 5,
        Wall = 1 << 6,
        Dash = 1 << 7
    }
}