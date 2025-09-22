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
        public PlayerSender Sender => FindComponent<PlayerSender>();
        public PlayerMachine Machine => FindComponent<PlayerMachine>();
        public PlayerFeature Feature => FindComponent<PlayerFeature>();
        public PlayerOperate Operate => FindComponent<PlayerOperate>();
        public NetworkTransform Transform => FindComponent<NetworkTransform>();
        private Ray2D DLRay => new Ray2D(transform.position - Vector3.right * 0.075f, Vector3.down);
        private Ray2D DRRay => new Ray2D(transform.position + Vector3.right * 0.075f, Vector3.down);
        private Ray2D RURay => new Ray2D(transform.position + Vector3.up * 0.1f, Vector3.right * transform.localScale.x);
        private Ray2D RDRay => new Ray2D(transform.position - Vector3.up * 0.075f, Vector3.right * transform.localScale.x);
        public RaycastHit2D DLHit => Physics2D.Raycast(DLRay.origin, DLRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D DRHit => Physics2D.Raycast(DRRay.origin, DRRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D RUHit => Physics2D.Raycast(RURay.origin, RURay.direction, 0.12f, 1 << 6);
        public RaycastHit2D RDHit => Physics2D.Raycast(RDRay.origin, RDRay.direction, 0.12f, 1 << 6);

        protected override void Awake()
        {
            base.Awake();
            AddComponent(typeof(PlayerFeature));
            AddComponent(typeof(PlayerMachine));
            AddComponent(typeof(PlayerOperate));
            Transform.syncDirection = SyncMode.Client;
        }


        private void Update()
        {
            if (isOwner)
            {
                Operate.OnUpdate();
                Machine.OnUpdate();
                Feature.OnUpdate();
            }

            Transform.Update();
        }

        private void LateUpdate()
        {
            Transform.LateUpdate();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(RDRay.origin, RDRay.direction * 0.12f);
            Gizmos.DrawRay(RURay.origin, RURay.direction * 0.12f);
            Gizmos.DrawRay(DLRay.origin, DRRay.direction * 0.12f);
            Gizmos.DrawRay(DRRay.origin, DRRay.direction * 0.12f);
        }
    }
}