using System;
using UnityEngine;

namespace Runtime
{
    public class PlayerIdle : PlayerState
    {
        protected override void OnUpdate()
        {
            if (isHold)
            {
                Machine.Update(Animations.Hold);
                return;
            }

            if (isDash)
            {
                Machine.Update(Animations.Dash);
                return;
            }

            if (isRush)
            {
                Machine.Update(Animations.Rush);
                return;
            }

            if (isJump)
            {
                Machine.Update(Animations.Jump);
                return;
            }

            if (isWalk && isPlane)
            {
                Machine.Update(Animations.Walk);
                return;
            }

            if (isPlane)
            {
                Machine.Update(Animations.Wait);
                return;
            }

            if (isGrab)
            {
                Machine.Update(Animations.Grab);
                return;
            }

            Machine.Update(Animations.Fall);
        }
    }

    public class PlayerWait : PlayerState
    {
        protected override void OnEnter()
        {
            Feature.RushCount = 0;
            owner.Sender.SyncColorServerRpc(Color.white);
        }

        protected override void OnUpdate()
        {
            InputX(InputManager.MoveX);
            InputY();
            Apply();
        }
    }

    public class PlayerWalk : PlayerState
    {
        protected override void OnEnter()
        {
            Feature.RushCount = 0;
            owner.Sender.SyncColorServerRpc(Color.green);
        }

        protected override void OnUpdate()
        {
            InputX(InputManager.MoveX);
            InputY();
            Apply();
        }
    }

    public class PlayerFall : PlayerState
    {
        protected override void OnEnter()
        {
            Feature.RushCount = 0;
            owner.Sender.SyncColorServerRpc(Color.red);
        }

        protected override void OnUpdate()
        {
            InputX(InputManager.MoveX, 1.2F, 0.08F);
            InputY();
            Apply();
        }
    }

    public class PlayerJump : PlayerState
    {
        private float waitTime;
        private float stepTime;

        protected override void OnEnter()
        {
            Feature.JumpCD = Time.fixedTime + 0.3F;
            Feature.JumpCount--;

            waitTime = Time.fixedTime + 0.15F;
            owner.Sender.SyncColorServerRpc(Color.yellow);

            if (!isPlane && Feature.GrabTimer > Time.fixedTime)
            {
                stepTime = Time.fixedTime + 0.1F;
                direction = Feature.GrabInput;
                if ((state & State.竖冲) != 0)
                {
                    velocityX = Feature.GrabInput * Feature.JumpForce;
                }
                else
                {
                    velocityX = Feature.GrabInput * Feature.GrabForce;
                }

                Debug.Log(velocityX);
            }

            velocityY = Mathf.Max(velocityY + Feature.JumpForce, Feature.JumpForce);
        }

        protected override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                state &= ~State.跳跃;
                return;
            }

            if (stepTime > Time.fixedTime)
            {
                if (InputManager.MoveX == Feature.GrabInput)
                {
                    velocityX = Mathf.Max(Feature.GrabInput * Feature.GrabForce, velocityX);
                }
            }
            else
            {
                InputX(InputManager.MoveX, 1.2F, 0.08F);
            }

            InputY();
            Apply();
        }

        protected override void OnExit()
        {
            state &= ~State.竖冲;
            state &= ~State.跳跃;
        }
    }

    public class PlayerGrab : PlayerState
    {
        protected override void OnEnter()
        {
            state |= State.攀爬;
            Feature.RushCount = 0;
            owner.Sender.SyncColorServerRpc(Color.magenta);
        }

        protected override void OnUpdate()
        {
            if (collision.RaycastX(Feature.GrabForce * InputManager.MoveX, LayerConst.Ground))
            {
                state |= State.悬挂;
                return;
            }

            InputX(InputManager.MoveX);
            InputY();
            Apply();
        }

        protected override void OnExit()
        {
            state &= ~State.攀爬;
        }
    }

    public class PlayerHold : PlayerState
    {
        protected override void OnEnter()
        {
            owner.Sender.SyncColorServerRpc(Color.orange);
        }

        protected override void OnUpdate()
        {
            if (!collision.RaycastX(Feature.GrabForce * InputManager.MoveX, LayerConst.Ground))
            {
                state &= ~State.悬挂;
            }

            InputX(direction);
            InputY(2);
            Apply();
        }

        protected override void OnExit()
        {
            velocityY = 0;
            state |= State.地面;
            state &= ~State.悬挂;
        }
    }

    public class PlayerDash : PlayerState
    {
        private State oldState;
        private float waitTime;
        private Vector2 normalize;

        protected override void OnEnter()
        {
            Feature.DashCount--;
            Feature.DashCD = Time.fixedTime + 0.4F;

            oldState = state;
            waitTime = Time.fixedTime + 0.18F;
            direction = InputManager.MoveX;
            normalize = InputManager.Direction;
        }

        protected override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                state &= ~State.冲刺;
                return;
            }

            velocityX = normalize.x * Feature.DashSpeed;
            velocityY = normalize.y * Feature.DashSpeed;
            switch (normalize.y)
            {
                case 0 when normalize.x < 0 && isWall && collision.RaycastX(velocityX, LayerConst.Ground, out var output):
                    velocityY = output;
                    break;
                case 0 when normalize.x > 0 && isWall && collision.RaycastX(velocityX, LayerConst.Ground, out var output):
                    velocityY = output;
                    break;
                case 0 when normalize.x == 0:
                    velocityX = direction * Feature.DashSpeed;
                    break;
                case < 0 when isGround && !isPlatform && collision.RaycastY(velocityY, LayerConst.Ground, out var output):
                    velocityX = output;
                    break;
                case < 0 when isPlane:
                    velocityX = direction * Feature.DashSpeed;
                    break;
                case > 0 when isHead && collision.RaycastY(velocityY, LayerConst.Ground, out var output):
                    velocityX = output;
                    Feature.JumpCount = 1;
                    Feature.GrabInput = Math.Sign(output);
                    Feature.GrabTimer = Time.fixedTime + 0.1F;
                    Feature.JumpTimer = Feature.GrabTimer;
                    break;
            }

            state &= ~State.碰撞;
            Apply();

            oldState |= state;
            if (isJump)
            {
                if (normalize.y <= 0)
                {
                    if ((oldState & State.平面) != 0 && isWalk)
                    {
                        state |= State.横冲;
                        state &= ~State.冲刺;
                        Feature.DashCount = 1;
                        Machine.Update(Animations.Jump);
                    }
                }
                else if ((oldState & State.墙顶) != 0)
                {
                    state |= State.竖冲;
                    state &= ~State.冲刺;
                    Machine.Update(Animations.Jump);
                    velocityY += Feature.JumpForce * 0.5F;
                }
            }
        }

        protected override void OnExit()
        {
            velocityX = 0;
            velocityY = 0;
            state &= ~State.冲刺;
        }
    }

    public class PlayerRush : PlayerState
    {
        private float waitTime;

        protected override void OnEnter()
        {
            if (Feature.RushInput != direction)
            {
                Feature.RushCount = 0;
                Feature.RushInput = direction;
            }

            waitTime = Time.fixedTime + 0.1F;
            velocityX = Feature.RushInput * (Feature.RushSpeed + Feature.RushCount * Feature.RushSpeed / 5);
            Feature.RushCount++;
        }

        protected override void OnUpdate()
        {
            if (InputManager.MoveX != Feature.RushInput)
            {
                state &= ~State.横冲;
                return;
            }

            if (waitTime < Time.fixedTime)
            {
                if (velocityX == 0)
                {
                    state &= ~State.横冲;
                }

                if (isCorner)
                {
                    state &= ~State.横冲;
                }
            }

            InputY();
            Apply();
        }

        protected override void OnExit()
        {
            state &= ~State.横冲;
        }
    }
}