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


namespace System.Runtime.CompilerServices
{
    public static class IsExternalInit
    {
    }
}

namespace Astraia.Common
{
    public record OnLoadBundle(int count, long amount) : IEvent;

    public record OnBundleUpdate(string name, float progress) : IEvent;

    public record OnBundleComplete(int status, string message) : IEvent;

    public record OnLoadAsset(string[] names) : IEvent;

    public record OnAssetUpdate(string name) : IEvent;

    public record OnAssetComplete : IEvent;

    public record OnLoadScene(string name) : IEvent;

    public record OnSceneUpdate(float progress) : IEvent;

    public record OnSceneComplete(string sceneName) : IEvent;

    public record OnDataComplete : IEvent;

    public record OnVisibleUpdate(int x, int y, int z) : IEvent;
}