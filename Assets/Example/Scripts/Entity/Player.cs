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

using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    public class Player : NetworkEntity
    {
        public PlayerSender Sender => GetAgent<PlayerSender>();
        public PlayerMachine Machine => GetAgent<PlayerMachine>();
        public PlayerFeature Feature => GetAgent<PlayerFeature>();
        public PlayerOperation Operation => GetAgent<PlayerOperation>();
        public NetworkTransform Transform => GetAgent<NetworkTransform>();
        public Ray2D downLeftRay => new Ray2D(transform.position - Vector3.right * 0.075f, Vector3.down);
        public Ray2D downRightRay => new Ray2D(transform.position + Vector3.right * 0.075f, Vector3.down);
        public Ray2D rightUpRay => new Ray2D(transform.position + Vector3.up * 0.1f, Vector3.right * transform.localScale.x);
        public Ray2D rightDownRay => new Ray2D(transform.position - Vector3.up * 0.075f, Vector3.right * transform.localScale.x);

        protected override void Awake()
        {
            base.Awake();
            AddAgent(typeof(PlayerFeature));
            AddAgent(typeof(PlayerMachine));
            AddAgent(typeof(PlayerOperation));
            Transform.syncDirection = SyncMode.Client;
        }

        private void Update()
        {
            if (isOwner)
            {
                Operation.OnUpdate();
                Machine.OnUpdate();
                Feature.OnUpdate();
            }

            Transform.OnUpdate();
        }

        private void LateUpdate()
        {
            Transform.OnLateUpdate();
        }

      

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(rightDownRay.origin, rightDownRay.direction * 0.12f);
            Gizmos.DrawRay(rightUpRay.origin, rightUpRay.direction * 0.12f);
            Gizmos.DrawRay(downLeftRay.origin, downRightRay.direction * 0.12f);
            Gizmos.DrawRay(downRightRay.origin, downRightRay.direction * 0.12f);
        }
    }
}