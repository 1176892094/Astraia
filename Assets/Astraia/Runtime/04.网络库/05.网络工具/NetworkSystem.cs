// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 21:01:21
// # Recently: 2025-01-11 14:01:56
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Astraia.Net
{
    internal static class NetworkSystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            AddLoopSystem(EarlyUpdate, ref playerLoop, typeof(EarlyUpdate));
            AddLoopSystem(AfterUpdate, ref playerLoop, typeof(PreLateUpdate));
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void EarlyUpdate()
        {
            if (!NetworkManager.Instance) return;
            NetworkManager.Server.EarlyUpdate();
            NetworkManager.Client.EarlyUpdate();
        }

        private static void AfterUpdate()
        {
            if (!NetworkManager.Instance) return;
            NetworkManager.Server.AfterUpdate();
            NetworkManager.Client.AfterUpdate();
        }

        private static bool AddLoopSystem(PlayerLoopSystem.UpdateFunction function, ref PlayerLoopSystem playerLoop, Type systemType)
        {
            if (playerLoop.type == systemType)
            {
                if (Array.FindIndex(playerLoop.subSystemList, system => system.updateDelegate == function) != -1)
                {
                    return true;
                }

                var oldLength = playerLoop.subSystemList?.Length ?? 0;
                Array.Resize(ref playerLoop.subSystemList, oldLength + 1);
                playerLoop.subSystemList[oldLength] = new PlayerLoopSystem
                {
                    type = typeof(NetworkManager),
                    updateDelegate = function
                };
                return true;
            }

            if (playerLoop.subSystemList != null)
            {
                for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
                {
                    if (AddLoopSystem(function, ref playerLoop.subSystemList[i], systemType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Tick(float sendRate, ref double sendTime)
        {
            var duration = 1.0 / sendRate;
            if (sendTime + duration <= Time.unscaledTimeAsDouble)
            {
                sendTime = (long)(Time.unscaledTimeAsDouble / duration) * duration;
                return true;
            }

            return false;
        }
    }
}