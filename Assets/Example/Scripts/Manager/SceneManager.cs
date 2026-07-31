using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    public struct OnPlayerUpdate : IEvent { }

    public struct OnPlatformUpdate : IEvent { }

    public class SceneManager : MonoBehaviour
    {
        public void FixedUpdate()
        {
            EventManager.Invoke(new OnPlayerUpdate()); //让玩家先更新
            EventManager.Invoke(new OnPlatformUpdate()); //再让平台更新
        }
    }
}