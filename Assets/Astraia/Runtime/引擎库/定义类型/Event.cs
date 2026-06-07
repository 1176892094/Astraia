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
    public struct OnLoadBundle : IEvent
    {
        public long bytes;

        public OnLoadBundle(long bytes)
        {
            this.bytes = bytes;
        }
    }

    public struct OnBundleUpdate : IEvent
    {
        public string name;
        public ulong bytes;

        public OnBundleUpdate(string name, ulong bytes)
        {
            this.name = name;
            this.bytes = bytes;
        }
    }

    public struct OnBundleComplete : IEvent
    {
        public int status;
        public string message;

        public OnBundleComplete(int status, string message)
        {
            this.status = status;
            this.message = message;
        }
    }

    public struct OnLoadAsset : IEvent
    {
        public string[] names;

        public OnLoadAsset(string[] names)
        {
            this.names = names;
        }
    }

    public struct OnAssetUpdate : IEvent
    {
        public string name;

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
        public string name;

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

    public readonly struct OnSceneComplete : IEvent
    {
        public readonly string sceneName;

        public OnSceneComplete(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }

    public struct OnDataComplete : IEvent
    {
    }
}