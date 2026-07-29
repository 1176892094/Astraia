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

        public Fixation MoveSpeed;
        public Fixation DashSpeed;
        public Fixation RushSpeed;
        public Fixation JumpForce;
        public Fixation GrabForce;

        public float Platform;
        public int RushCount;
        public int RushInput;

        public int GrabInput;
        public float GrabTimer;

        public int JumpCount;
        public float JumpInput;
        public float JumpTimer;
        public float JumpCD;

        public int DashCount;
        public float DashInput;
        public float DashCD;

        protected override void Dequeue()
        {
            MoveSpeed = 200F / 40 / 60;
            GrabForce = MoveSpeed * 2.5F;
            JumpForce = MoveSpeed * 3;
            DashSpeed = MoveSpeed * 5;
            RushSpeed = DashSpeed / 2;
        }
    }
}