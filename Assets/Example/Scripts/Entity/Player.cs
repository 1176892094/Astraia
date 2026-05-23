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
        public State State = State.默认;
        public PlayerInput Input;
        public PlayerSender Sender;
        public PlayerMachine Machine;
        public PlayerFeature Feature;

        protected override void Awake()
        {
            base.Awake();
            Sender = gameObject.GetComponent<PlayerSender>();
            Feature = gameObject.AddComponent<PlayerFeature>();
            Machine = gameObject.AddComponent<PlayerMachine>();
        }

        private void Update()
        {
            if (isOwner)
            {
                Input.Tick();
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

    public class PlayerFeature : MonoBehaviour
    {
        public int MoveSpeed;
        public int VelocityX;
        public int VelocityY;
        public int PositionX;
        public int PositionY;

        public int JumpForce;
        public int JumpCount;
        public float JumpInput;
        public float JumpTimer;
        public float JumpCD;

        public int DashSpeed;
        public int DashCount;
        public float DashInput;
        public float DashTimer;
        public float DashCD;

        public int CrashSpeed;
        public int CrashCount;
        public Vector3 CrashPoint;

        private void Awake()
        {
            MoveSpeed = 10;
            JumpForce = MoveSpeed * 5 / 2;
            DashSpeed = MoveSpeed * 4;
            CrashSpeed = MoveSpeed * 3;
        }

        private void OnDrawGizmos()
        {
            var bounds = GetComponent<Collider2D>().bounds;
            var origin = (Vector2)bounds.center;
            var velocity = new Vector2(VelocityX, VelocityY);
            var position = origin + velocity.normalized * velocity.magnitude / 100f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(position, bounds.size);
        }
    }
}