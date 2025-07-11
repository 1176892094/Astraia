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
using Astraia.Common;
using Astraia.Net;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime
{
    public class Player : NetworkBehaviour, IStartAuthority
    {
        [SyncVar(nameof(OnColorValueChanged))] public Color syncColor;
        [ShowInInspector] public PlayerMachine machine => this.Find<PlayerMachine>();
        [ShowInInspector] public PlayerAttribute attribute => this.Find<PlayerAttribute>();
        [ShowInInspector] public PlayerOperation operation => this.Find<PlayerOperation>();
        public Ray2D downLeftRay => new Ray2D(transform.position - Vector3.right * 0.075f, Vector3.down);
        public Ray2D downRightRay => new Ray2D(transform.position + Vector3.right * 0.075f, Vector3.down);
        public Ray2D rightUpRay => new Ray2D(transform.position + Vector3.up * 0.1f, Vector3.right * transform.localScale.x);
        public Ray2D rightDownRay => new Ray2D(transform.position - Vector3.up * 0.075f, Vector3.right * transform.localScale.x);


        private void Awake()
        {
            this.Show<PlayerAttribute>(typeof(PlayerAttribute));
            this.Show<PlayerOperation>(typeof(PlayerOperation));
            this.Show<PlayerMachine>(typeof(PlayerMachine));
        }

        private void Update()
        {
            operation.Update();
            attribute.Update();
            machine.Update();
        }

        public void OnStartAuthority()
        {
            machine.AddState<PlayerIdle>(typeof(PlayerIdle));
            machine.AddState<PlayerWalk>(typeof(PlayerWalk));
            machine.AddState<PlayerJump>(typeof(PlayerJump));
            machine.AddState<PlayerGrab>(typeof(PlayerGrab));
            machine.AddState<PlayerDash>(typeof(PlayerDash));
            machine.AddState<PlayerHop>(typeof(PlayerHop));
            machine.AddState<PlayerCrash>(typeof(PlayerCrash));
            machine.ChangeState<PlayerIdle>();
            GameManager.Instance.SetCamera(this, new Vector3(0, 3, 0), new Vector2(30, 8));
        }

        private void OnDestroy()
        {
            this.Hide<PlayerMachine>();
            this.Hide<PlayerOperation>();
            this.Hide<PlayerAttribute>();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(rightDownRay.origin, rightDownRay.direction * 0.12f);
            Gizmos.DrawRay(rightUpRay.origin, rightUpRay.direction * 0.12f);
            Gizmos.DrawRay(downLeftRay.origin, downRightRay.direction * 0.12f);
            Gizmos.DrawRay(downRightRay.origin, downRightRay.direction * 0.12f);
        }

        private void OnColorValueChanged(Color oldValue, Color newValue)
        {
            machine.renderer.color = newValue;
        }

        [ServerRpc]
        public void LoadEffectServerRpc(Vector3 position)
        {
            SpawnManager.Instance.LoadEffectClientRpc(position);
        }

        [ServerRpc]
        public void SyncColorServerRpc(Color syncColor)
        {
            this.syncColor = syncColor;
        }
    }
}