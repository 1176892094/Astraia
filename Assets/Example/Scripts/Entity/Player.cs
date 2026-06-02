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

using System.Collections.Generic;
using Astraia.Net;
using UnityEngine.Tilemaps;

namespace Runtime
{
    public class Player : NetworkEntity
    {
        public PlayerAction Action;
        public PlayerSender Sender;
        public PlayerFeature Feature;
        public PlayerMachine Machine;

        public Tilemap collision;
        public List<Rigidbody> collisions = new List<Rigidbody>();

        protected override void Awake()
        {
            base.Awake();
            collision = FindFirstObjectByType<Tilemap>();
        }

        private void Update()
        {
            if (isOwner)
            {
                Action.Tick();
            }
        }

        private void FixedUpdate()
        {
            if (isOwner)
            {
                Machine.Tick();
            }
        }
    }
}