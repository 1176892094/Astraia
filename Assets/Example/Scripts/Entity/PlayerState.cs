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

using Astraia;
using UnityEngine;

namespace Runtime
{
    public class PlayerIdle : PlayerState
    {
        public override void OnEnter()
        {
            Feature.CrashCount = 0;
            Feature.CrashPoint = Vector3.zero;
            owner.Sender.SyncColorServerRpc(Color.white);
        }

        public override void OnUpdate()
        {
            if (isFall)
            {
                Machine.Switch(Animations.Fall);
                return;
            }

            if (isWalk)
            {
                Machine.Switch(Animations.Walk);
            }

            Move(Feature.MoveSpeed);
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

            if (!isWalk)
            {
                Machine.Switch(Animations.Idle);
            }

            Move(Feature.MoveSpeed);
        }

        public override void OnExit()
        {
        }
    }

    public class PlayerJump : PlayerState
    {
        private int moveX;
        private float waitTime;

        public override void OnEnter()
        {
            Feature.JumpCount--;
            State |= State.跳跃;
            waitTime = Time.fixedTime + 0.1F;
            owner.Sender.SyncColorServerRpc(Color.yellow);

            if (isWall && !isGround)
            {
                State |= State.侧跳;
                Direction = -Direction;
                moveX = State.HasFlag(State.左墙) ? 1 : -1;
                velocityX = moveX * Feature.JumpForce / 2;
            }

            velocityY = Mathf.Max(velocityY + Feature.JumpForce, Feature.JumpForce);
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer > Time.fixedTime)
            {
                if (InputManager.MoveY != 1)
                {
                    if (State.HasFlag(State.地面) && isWalk)
                    {
                        Machine.Switch(Animations.Crash);
                        return;
                    }
                }

                Feature.DashTimer = Time.fixedTime;
                velocityY += Feature.JumpForce * 2 / 10;
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

                if (isRoad)
                {
                    Machine.Switch(Animations.Idle);
                    return;
                }
            }
            else
            {
                if (State.HasFlag(State.侧跳))
                {
                    if (moveX != 0 && InputManager.MoveX == moveX)
                    {
                        velocityX += moveX * 5;
                    }
                    else
                    {
                        velocityX += InputManager.MoveX;
                    }

                    positionX += velocityX;
                    positionY += velocityY;
                    transform.position = new Vector3(positionX, positionY) / FIX;
                    return;
                }
            }

            Move(Feature.MoveSpeed, 2);
        }

        public override void OnExit()
        {
            State &= ~State.跳跃;
            State &= ~State.侧跳;
        }
    }

    public class PlayerFall : PlayerState
    {
        public override void OnEnter()
        {
            State |= State.下落;
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

            Move(Feature.MoveSpeed, 2);
        }

        public override void OnExit()
        {
            State &= ~State.下落;
        }
    }

    public class PlayerDash : PlayerState
    {
        private Vector2 direction;

        public override void OnEnter()
        {
            Feature.DashCount--;
            State |= State.冲刺;
            Feature.DashTimer = Time.fixedTime + 0.20F;
            owner.Sender.SyncColorServerRpc(Color.magenta);
            direction = InputManager.Direction.normalized;
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer < Time.fixedTime)
            {
                Machine.Switch(Animations.Idle);
                return;
            }

            if (Vector3.Distance(transform.position, Feature.CrashPoint) >= 1.2f)
            {
                Feature.CrashPoint = transform.position;
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            velocityX = (int)(direction.x * Feature.DashSpeed);
            velocityY = (int)(direction.y * Feature.DashSpeed);

            if (direction == Vector2.zero)
            {
                velocityX = Direction * Feature.DashSpeed;
            }
            else if (direction.y < 0)
            {
                if (isGround)
                {
                    velocityX = Dash();
                }
            }
            else if (direction.y > 0)
            {
                if (State.HasFlag(State.头顶))
                {
                    Dash();
                }
            }

            Contact();
        }

        public override void OnExit()
        {
            velocityX = 0;
            velocityY = 0;
            State &= ~State.冲刺;
        }
    }

    public class PlayerGrab : PlayerState
    {
        public override void OnEnter()
        {
            State |= State.攀爬;
            owner.Sender.SyncColorServerRpc(Color.cyan);
        }

        public override void OnUpdate()
        {
            if (!isGrab)
            {
                Machine.Switch(Animations.Idle);
                return;
            }

            if (Hold())
            {
                return;
            }

            Move(Feature.MoveSpeed);
        }

        public override void OnExit()
        {
            State &= ~State.攀爬;
        }
    }

    public class PlayerCrash : PlayerState
    {
        private float waitTime;
        private int moveX;

        public override void OnEnter()
        {
            State |= State.冲跳;
            waitTime = Time.fixedTime + 0.1F;
            moveX = Direction;
            velocityX = Direction * (Feature.CrashSpeed + Feature.CrashSpeed * Feature.CrashCount / 4);
            Feature.CrashCount++;
        }

        public override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                if (isRoad)
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

            if (Vector3.Distance(transform.position, Feature.CrashPoint) >= 1.2f)
            {
                Feature.CrashPoint = transform.position;
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            Gravity();
            Contact();
        }

        public override void OnExit()
        {
            velocityX /= 2;
            State &= ~State.冲跳;
        }
    }
}