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

using Astraia;
using Astraia.Net;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime
{
    public class Player : NetworkBehaviour
    {
        [ShowInInspector] public PlayerMachine machine => this.Find<PlayerMachine>();
        [ShowInInspector] public PlayerAttribute attribute => this.Find<PlayerAttribute>();
        public Ray2D leftDownRay => new Ray2D(transform.position - Vector3.right * 0.075f, Vector3.down);
        public Ray2D rightDownRay => new Ray2D(transform.position + Vector3.right * 0.075f, Vector3.down);
        public Ray2D rightRay => new Ray2D(transform.position - Vector3.up * 0.075f, Vector3.right * transform.localScale.x);
        public Ray2D rightUpRay => new Ray2D(transform.position + Vector3.up * 0.1f, Vector3.right * transform.localScale.x);


        private void Awake()
        {
            this.Show<PlayerAttribute>(typeof(PlayerAttribute));
            this.Show<PlayerOperation>(typeof(PlayerOperation));
            this.Show<PlayerMachine>(typeof(PlayerMachine));
        }

        private void Start()
        {
            machine.AddState<PlayerIdle>(typeof(PlayerIdle));
            machine.AddState<PlayerWalk>(typeof(PlayerWalk));
            machine.AddState<PlayerJump>(typeof(PlayerJump));
            machine.AddState<PlayerGrab>(typeof(PlayerGrab));
            machine.AddState<PlayerDash>(typeof(PlayerDash));
            machine.AddState<PlayerHop>(typeof(PlayerHop));
            machine.ChangeState<PlayerIdle>();
        }

        private void OnDestroy()
        {
            this.Hide<PlayerMachine>();
            this.Hide<PlayerOperation>();
            this.Hide<PlayerAttribute>();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(rightRay.origin, rightRay.direction * 0.12f);
            Gizmos.DrawRay(rightUpRay.origin, rightUpRay.direction * 0.12f);
            Gizmos.DrawRay(leftDownRay.origin, rightDownRay.direction * 0.12f);
            Gizmos.DrawRay(rightDownRay.origin, rightDownRay.direction * 0.12f);
        }
        
        
        [ServerRpc]
        public void LoadEffectServerRpc(Vector3 position)
        {
            SpawnManager.Instance.LoadEffectClientRpc(position);
        }
    }
}