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
    public struct PackAwake : IEvent
    {
        public readonly int[] sizes;

        public PackAwake(int[] sizes)
        {
            this.sizes = sizes;
        }
    }

    public struct PackUpdate : IEvent
    {
        public readonly string name;
        public readonly float progress;

        public PackUpdate(string name, float progress)
        {
            this.name = name;
            this.progress = progress;
        }
    }

    public struct PackComplete : IEvent
    {
        public readonly int status;
        public readonly string message;

        public PackComplete(int status, string message)
        {
            this.status = status;
            this.message = message;
        }
    }

    public struct AssetAwake : IEvent
    {
        public readonly string[] names;

        public AssetAwake(string[] names)
        {
            this.names = names;
        }
    }

    public struct AssetUpdate : IEvent
    {
        public readonly string name;

        public AssetUpdate(string name)
        {
            this.name = name;
        }
    }

    public struct AssetComplete : IEvent
    {
    }

    public struct SceneAwake : IEvent
    {
        public readonly string name;

        public SceneAwake(string name)
        {
            this.name = name;
        }
    }

    public struct SceneUpdate : IEvent
    {
        public readonly float progress;

        public SceneUpdate(float progress)
        {
            this.progress = progress;
        }
    }

    public struct SceneComplete : IEvent
    {
    }

    public struct DataAwake : IEvent
    {
        public readonly string[] names;

        public DataAwake(string[] names)
        {
            this.names = names;
        }
    }

    public struct DataUpdate : IEvent
    {
        public readonly string name;

        public DataUpdate(string name)
        {
            this.name = name;
        }
    }

    public struct DataComplete : IEvent
    {
    }
}