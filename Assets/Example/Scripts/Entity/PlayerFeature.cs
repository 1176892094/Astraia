using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class PlayerFeature : Module<Player>
    {
        public State State = State.默认;

        public MovePlatform platform;

        public Fixation MoveSpeed;
        public Fixation DashSpeed;
        public Fixation RushSpeed;
        public Fixation JumpForce;
        public Fixation GrabForce;

        public float FallSpeed;
        public float FallLimit;
        public float GrabSpeed;
        public float GrabLimit;

        public int RushCount;
        public int RushInput;

        public int JumpCount;
        public float JumpInput;
        public float JumpTimer;
        public float JumpTime;
        public float JumpCD;
        public int JumpDirection;
        public float JumpPlatform;

        public int DashCount;
        public float DashInput;
        public float DashTimer;
        public float DashTime = 0.18F;
        public float DashCD;
        public Vector2 DashDirection;
        public Vector2 QuadDirection;

        protected override void Dequeue()
        {
            MoveSpeed = 5F / 60;
            FallLimit = 15F / 60;
            FallSpeed = 1.5F / 60;
            GrabLimit = 3.0F / 60;
            GrabSpeed = 0.75F / 60;
            GrabForce = MoveSpeed * 2.5F;
            JumpForce = MoveSpeed * 3;
            DashSpeed = MoveSpeed * 5;
            RushSpeed = DashSpeed / 2;
        }
    }
}