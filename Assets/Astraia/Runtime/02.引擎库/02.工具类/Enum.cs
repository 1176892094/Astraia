// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 22:04:20
// // # Recently: 2025-04-09 22:04:20
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

namespace Astraia
{
    internal enum AssetPlatform : byte
    {
        StandaloneOSX = 2,
        StandaloneWindows = 5,
        IOS = 9,
        Android = 13
    }

    internal enum AssetData : byte
    {
        Assembly,
        Enum,
        Struct,
        DataTable,
        Input,
        Icons,
    }

    internal enum AssetMode : byte
    {
        Resource,
        Simulate,
        Actuator
    }

    internal enum InputMask : byte
    {
        Enable,
        Disable
    }

    internal enum BuildMode : byte
    {
        AssetBundlePath,
        StreamingAssets
    }

    public enum UIState : byte
    {
        Common,
        Freeze,
        Stable,
    }

    public enum NodeState : byte
    {
        Running,
        Success,
        Failure
    }

    public enum AudioState : byte
    {
        Play,
        Pause,
        Stop,
    }

    public enum AudioMode : byte
    {
        Once,
        Loop
    }
}