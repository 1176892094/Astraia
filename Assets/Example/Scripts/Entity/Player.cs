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
    public class Player : NetworkEntity
    {
        public PlayerFeature Feature => GetComponent<PlayerFeature>(0);
        public PlayerMachine Machine => GetComponent<PlayerMachine>(1);
        public PlayerSender Sender => GetComponent<PlayerSender>(2);
        public PlayerAction Action => GetComponent<PlayerAction>(4);

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

        private void FixedUpdate()
        {
            if (isOwner)
            {
                Action.Tick();
                Machine.Tick();
            }
        }
    }

    public class PlayerMachine : Rigidbody
    {
        private readonly StateMachine machine = new StateMachine();

        protected override void Enqueue()
        {
            machine.Clear();
        }

        public void Tick()
        {
            machine.Update();
        }

        public void Update(int value)
        {
            machine.Update(value);
        }

        public void Create<T>(int value)
        {
            machine.Create<T>(owner, value);
        }

        public void Switch(int value)
        {
            machine.Switch(value);
        }
    }

    public class PlayerFeature : Module<Player>
    {
        public State State = State.默认;

        public int MoveSpeed;
        public float Platform;

        public int GrabForce;
        public int GrabInput;
        public float GrabTimer;
        public float GrabCD;

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

        public int RushSpeed;
        public int RushCount;
        public Vector3 RushPoint;

        protected override void Dequeue()
        {
            MoveSpeed = 30;
            GrabForce = MoveSpeed * 3 / 2;
            JumpForce = MoveSpeed * 2;
            DashSpeed = MoveSpeed * 3;
            RushSpeed = MoveSpeed * 2;
        }
    }
}