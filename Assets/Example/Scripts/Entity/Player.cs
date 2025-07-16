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
            AddAgent(typeof(PlayerSender));
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
                Machine.OnUpdate();
                Feature.OnUpdate();
                Operation.OnUpdate();
            }

            Transform.OnUpdate();
        }

        private void LateUpdate()
        {
            Transform.OnLateUpdate();
        }

        public void OnStartAuthority()
        {
            Machine.AddState<PlayerIdle>(typeof(PlayerIdle));
            Machine.AddState<PlayerWalk>(typeof(PlayerWalk));
            Machine.AddState<PlayerJump>(typeof(PlayerJump));
            Machine.AddState<PlayerGrab>(typeof(PlayerGrab));
            Machine.AddState<PlayerDash>(typeof(PlayerDash));
            Machine.AddState<PlayerHop>(typeof(PlayerHop));
            Machine.AddState<PlayerCrash>(typeof(PlayerCrash));
            Machine.ChangeState<PlayerIdle>();
            GameManager.Instance.SetCamera(this, new Vector3(0, 3, 0), new Vector2(30, 8));
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