// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-21 02:04:44
// // # Recently: 2025-04-21 02:04:44
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using UnityEngine;

namespace Runtime
{
    public class PlayerIdle : PlayerState
    {
        public override void OnEnter()
        {
            Feature.CrashCount = 0;
            Feature.CrashPoint = Vector3.down * 1000;
            owner.Sender.SyncColorServerRpc(Color.white);
        }

        public override void OnUpdate()
        {
            if (isFall)
            {
                Machine.Switch(Animations.Fall);
                return;
            }

            if (InputManager.MoveX != 0)
            {
                Machine.Switch(Animations.Walk);
            }

            Move();
        }

        public override void OnExit()
        {
        }
    }

    public class PlayerWalk : PlayerState
    {
        public override void OnEnter()
        {
            owner.Sender.SyncColorServerRpc(Color.green);
        }

        public override void OnUpdate()
        {
            if (isGrab)
            {
                Machine.Switch(Animations.Grab);
                return;
            }

            if (isFall)
            {
                Machine.Switch(Animations.Fall);
                return;
            }

            if (InputManager.MoveX == 0)
            {
                Machine.Switch(Animations.Idle);
            }

            Move();
        }

        public override void OnExit()
        {
        }
    }

    public class PlayerJump : PlayerState
    {
        private float waitTime;

        public override void OnEnter()
        {
            Feature.JumpCount--;
            state |= State.跳跃;
            waitTime = Time.fixedTime + 0.1F;
            owner.Sender.SyncColorServerRpc(Color.yellow);

            if (!isGround && Feature.WallTimer > Time.fixedTime)
            {
                state |= State.侧跳;
                direction = Feature.WallInput;
                velocityX = Feature.WallInput * Feature.JumpForce / 2;
            }

            velocityY = Mathf.Max(velocityY + Feature.JumpForce, Feature.JumpForce);
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer > Time.fixedTime)
            {
                if (isGround && isCrash)
                {
                    Machine.Switch(Animations.Crash);
                    return;
                }

                Feature.DashTimer = Time.fixedTime;
            }

            if (waitTime < Time.fixedTime)
            {
                if (isGrab)
                {
                    Machine.Switch(Animations.Grab);
                    return;
                }

                if (isFall)
                {
                    Machine.Switch(Animations.Fall);
                }

                if (isCorner)
                {
                    Machine.Switch(Animations.Idle);
                    return;
                }

                Feature.WallInput = InputManager.MoveX;
            }
            else
            {
                if (state.HasFlag(State.侧跳))
                {
                    if (InputManager.MoveX == Feature.WallInput)
                    {
                        velocityX = Feature.WallForce * Feature.WallInput;
                    }

                    Collision();
                    return;
                }
            }

            Move(2);
        }

        public override void OnExit()
        {
            state &= ~State.跳跃;
            state &= ~State.侧跳;
        }
    }

    public class PlayerFall : PlayerState
    {
        public override void OnEnter()
        {
            state |= State.下落;
            owner.Sender.SyncColorServerRpc(Color.red);
        }

        public override void OnUpdate()
        {
            if (isGrab)
            {
                Machine.Switch(Animations.Grab);
                return;
            }

            if (isGround)
            {
                Machine.Switch(Animations.Idle);
            }

            Move(2);
        }

        public override void OnExit()
        {
            state &= ~State.下落;
        }
    }

    public class PlayerDash : PlayerState
    {
        private Vector2 normalize;

        public override void OnEnter()
        {
            Feature.DashCount--;
            state |= State.冲刺;
            Feature.DashTimer = Time.fixedTime + 0.2F;
            owner.Sender.SyncColorServerRpc(Color.magenta);
            normalize = InputManager.Direction.normalized;
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer < Time.fixedTime)
            {
                Machine.Switch(Animations.Idle);
                return;
            }

            if (Vector3.Distance(transform.position, Feature.CrashPoint) >= 1.45f)
            {
                Feature.CrashPoint = transform.position;
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            velocityX = Mathf.RoundToInt(normalize.x * Feature.DashSpeed);
            velocityY = Mathf.RoundToInt(normalize.y * Feature.DashSpeed);

            if (normalize != Vector2.zero)
            {
                switch (normalize.y)
                {
                    case < 0 when isGround:
                        velocityX = direction * Feature.DashSpeed;
                        break;
                    case > 0 when isHead && Machine.Checked(Feature.MoveSpeed, out var output):
                        var result = Math.Sign(output);
                        Feature.JumpCount = 1;
                        Feature.WallInput = result;
                        Feature.JumpTimer = Time.time + 0.1F;
                        Feature.WallTimer = Time.fixedTime + 0.1F;
                        velocityX = (int)(output * Rigidbody.FIX);
                        break;
                }
            }
            else
            {
                velocityX = direction * Feature.DashSpeed;
            }

            Collision();
        }

        public override void OnExit()
        {
            velocityX = 0;
            velocityY = 0;
            state &= ~State.冲刺;
        }
    }

    public class PlayerGrab : PlayerState
    {
        public override void OnEnter()
        {
            state |= State.攀爬;
            owner.Sender.SyncColorServerRpc(Color.cyan);
        }

        public override void OnUpdate()
        {
            if (Machine.OverlapHold(Feature.MoveSpeed * InputManager.MoveX))
            {
                velocityY = Feature.MoveSpeed;
                Collision();
                return;
            }

            if (!isGrab)
            {
                Machine.Switch(Animations.Idle);
                return;
            }

            Move();
        }

        public override void OnExit()
        {
            state &= ~State.攀爬;
        }
    }

    public class PlayerCrash : PlayerState
    {
        private float waitTime;
        private int moveX;

        public override void OnEnter()
        {
            state |= State.冲跳;
            waitTime = Time.fixedTime + 0.1F;
            moveX = direction;
            velocityX = direction * (Feature.CrashSpeed + Feature.CrashSpeed * Feature.CrashCount / 4);
            Feature.CrashCount++;
        }

        public override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                if (isCorner)
                {
                    Machine.Switch(Animations.Idle);
                    return;
                }

                if (InputManager.MoveX != 0 && moveX != InputManager.MoveX)
                {
                    Machine.Switch(Animations.Idle);
                    return;
                }
            }

            if (Vector3.Distance(transform.position, Feature.CrashPoint) >= 1.45f)
            {
                Feature.CrashPoint = transform.position;
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            Gravity();
            Collision();
        }

        public override void OnExit()
        {
            velocityX /= 2;
            state &= ~State.冲跳;
        }
    }
}