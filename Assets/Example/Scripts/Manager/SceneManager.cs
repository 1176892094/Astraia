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
            EventManager.Invoke(new OnPlatformUpdate());
            Physics2D.SyncTransforms();
            EventManager.Invoke(new OnPlayerUpdate());
        }
    }
}