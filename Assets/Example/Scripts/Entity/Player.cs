// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:03
// // # Recently: 2025-04-20 19:04:03
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using Astraia;
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class Player : NetworkEntity, IEvent<OnPlayerUpdate>
    {
        public PlayerFeature Feature => GetComponent<PlayerFeature>();
        public PlayerMachine Machine => GetComponent<PlayerMachine>();
        public PlayerSender Sender => GetComponent<PlayerSender>();
        public PlayerModule Module => GetComponent<PlayerModule>();
        public PlayerAction Action => GetComponent<PlayerAction>();

        public int Direction
        {
            get => Math.Sign(transform.localScale.x);
            set
            {
                if ((value > 0 && Direction < 0) || (value < 0 && Direction > 0))
                {
                    if (isOwner)
                    {
                        Sender.SetDirectionServerRpc(value);
                    }

                    transform.localScale = new Vector3(value, 1, 1);
                }
            }
        }

        public void Execute(OnPlayerUpdate message)
        {
            Module.Tick();

            if (isOwner)
            {
                Action.Tick();
                Machine.Tick();
            }
            else
            {
                Machine.SyncTransform();
            }
        }
    }
}