// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 22:04:35
// // # Recently: 2025-04-09 22:04:35
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

namespace Astraia.Common
{
    public struct OnLoadBundle : IEvent
    {
        public readonly int count;
        public readonly long size;

        public OnLoadBundle(int count, long size)
        {
            this.count = count;
            this.size = size;
        }
    }

    public struct OnBundleUpdate : IEvent
    {
        public readonly string name;
        public readonly float progress;

        public OnBundleUpdate(string name, float progress)
        {
            this.name = name;
            this.progress = progress;
        }
    }

    public struct OnBundleComplete : IEvent
    {
        public readonly int status;
        public readonly string message;

        public OnBundleComplete(int status, string message)
        {
            this.status = status;
            this.message = message;
        }
    }

    public struct OnLoadAsset : IEvent
    {
        public readonly string[] names;

        public OnLoadAsset(string[] names)
        {
            this.names = names;
        }
    }

    public struct OnAssetUpdate : IEvent
    {
        public readonly string name;

        public OnAssetUpdate(string name)
        {
            this.name = name;
        }
    }

    public struct OnAssetComplete : IEvent
    {
    }

    public struct OnLoadScene : IEvent
    {
        public readonly string name;

        public OnLoadScene(string name)
        {
            this.name = name;
        }
    }

    public struct OnSceneUpdate : IEvent
    {
        public readonly float progress;

        public OnSceneUpdate(float progress)
        {
            this.progress = progress;
        }
    }

    public struct OnSceneComplete : IEvent
    {
    }

    public struct OnDataComplete : IEvent
    {
    }
}