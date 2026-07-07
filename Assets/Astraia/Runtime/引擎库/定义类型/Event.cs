// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-04-09 22:04:35
// # Recently: 2025-04-09 22:04:35
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia.Core
{
    public readonly struct OnLoadBundle : IEvent
    {
        public readonly long bytes;

        public OnLoadBundle(long bytes)
        {
            this.bytes = bytes;
        }
    }

    public readonly struct OnBundleUpdate : IEvent
    {
        public readonly string name;
        public readonly ulong bytes;

        public OnBundleUpdate(string name, ulong bytes)
        {
            this.name = name;
            this.bytes = bytes;
        }
    }

    public readonly struct OnBundleComplete : IEvent
    {
        public readonly int opcode;
        public readonly string message;

        public OnBundleComplete(int opcode, string message)
        {
            this.opcode = opcode;
            this.message = message;
        }
    }

    public readonly struct OnLoadAsset : IEvent
    {
        public readonly string[] names;

        public OnLoadAsset(string[] names)
        {
            this.names = names;
        }
    }

    public readonly struct OnAssetUpdate : IEvent
    {
        public readonly string name;

        public OnAssetUpdate(string name)
        {
            this.name = name;
        }
    }

    public readonly struct OnAssetComplete : IEvent
    {
    }

    public readonly struct OnLoadScene : IEvent
    {
        public readonly string name;

        public OnLoadScene(string name)
        {
            this.name = name;
        }
    }

    public readonly struct OnSceneUpdate : IEvent
    {
        public readonly float progress;

        public OnSceneUpdate(float progress)
        {
            this.progress = progress;
        }
    }

    public readonly struct OnSceneComplete : IEvent
    {
        public readonly string sceneName;

        public OnSceneComplete(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }
}