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
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime
{
    public class Player : MonoBehaviour
    {
        [ShowInInspector] public PlayerMachine machine => this.Find<PlayerMachine>();
        [ShowInInspector] public PlayerAttribute attribute => this.Find<PlayerAttribute>();

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
            machine.ChangeState<PlayerIdle>();
        }

        private void OnDestroy()
        {
            this.Hide<PlayerMachine>();
            this.Hide<PlayerAttribute>();
            this.Hide<PlayerOperation>();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawRay(transform.position, Vector3.down * 0.11f);
            Gizmos.DrawRay(transform.position, Vector3.left * 0.11f);
            Gizmos.DrawRay(transform.position, Vector3.right * 0.11f);
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.right * transform.localScale.x * 0.11f);
        }
    }
}