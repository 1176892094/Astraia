using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    public class PlayerIdle : PlayerState
    {
        protected override void OnUpdate()
        {
            if (isShuttle)
            {
                Machine.Update(Animations.Shuttle);
                return;
            }

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
            InputX(GameManager.MoveX);
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
            InputX(GameManager.MoveX);
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
            InputX(GameManager.MoveX, 1.2F, 0.08F);
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

            if (!isPlane && Feature.WallTimer > Time.fixedTime)
            {
                stepTime = Time.fixedTime + 0.1F;
                direction = Feature.WallInput;
                if ((state & State.竖冲) != 0)
                {
                    velocityX = Feature.WallInput * Feature.JumpForce;
                }
                else
                {
                    velocityX = Feature.WallInput * Feature.GrabForce;
                }
            }

            velocityY = Mathf.Max(velocityY + Feature.JumpForce, Feature.JumpForce);

            if ((state & State.竖冲) != 0)
            {
                owner.Sender.LoadEffectServerRpc(Machine.position, Machine.velocity);
                Machine.cachePosition = Machine.position;
            }

            Apply();
        }

        protected override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                state &= ~State.跳跃;
                return;
            }

            if ((state & State.竖冲) != 0)
            {
                if (Distance(Machine.position, Machine.cachePosition) >= 2.2f)
                {
                    owner.Sender.LoadEffectServerRpc(Machine.position, Machine.velocity);
                    Machine.cachePosition = Machine.position;
                }
            }

            if (stepTime > Time.fixedTime)
            {
                if (GameManager.MoveX == Feature.WallInput)
                {
                    velocityX = Mathf.Max(Feature.WallInput * Feature.GrabForce, velocityX);
                }
            }
            else
            {
                InputX(GameManager.MoveX, 1.2F, 0.08F);
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
            if (collision.RaycastX(Feature.GrabForce * GameManager.MoveX, LayerConst.GroundAndCollision))
            {
                state |= State.悬挂;
                return;
            }

            InputX(GameManager.MoveX);
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
            if (!collision.RaycastX(Feature.GrabForce * GameManager.MoveX, LayerConst.GroundAndCollision))
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

        protected override void OnEnter()
        {
            Feature.DashCount--;
            Feature.DashCD = Time.fixedTime + 0.4F;
            oldState = state;
            Feature.DashTimer = Time.fixedTime + Feature.DashTime;
            Feature.DashDirection = GameManager.Direction;
            direction = GameManager.MoveX;
            Machine.cachePosition = new Position(0, -100);
        }

        protected override void OnUpdate()
        {
            if (Feature.DashTimer < Time.fixedTime)
            {
                state &= ~State.冲刺;
                return;
            }

            if (Distance(Machine.position, Machine.cachePosition) >= 1.4f)
            {
                Machine.cachePosition = Machine.position;
                owner.Sender.LoadEffectServerRpc(Machine.position);
            }

            var normalize = Feature.DashDirection;
            velocityX = normalize.x * Feature.DashSpeed;
            velocityY = normalize.y * Feature.DashSpeed;
            switch (normalize.y)
            {
                case 0 when normalize.x < 0 && isWall && collision.RaycastX(velocityX, LayerConst.GroundAndCollision, out var output):
                    velocityY = output;
                    break;
                case 0 when normalize.x > 0 && isWall && collision.RaycastX(velocityX, LayerConst.GroundAndCollision, out var output):
                    velocityY = output;
                    break;
                case 0 when normalize.x == 0:
                    velocityX = direction * Feature.DashSpeed;
                    break;
                case < 0 when isGround && !isPlatform && collision.RaycastY(velocityY, LayerConst.GroundAndCollision, out var output):
                    velocityX = output;
                    break;
                case < 0 when isPlane:
                    velocityX = direction * Feature.DashSpeed;
                    break;
                case > 0 when isHead && collision.RaycastY(velocityY, LayerConst.GroundAndCollision, out var output):
                    velocityX = output;
                    Feature.JumpCount = 1;
                    Feature.WallInput = Math.Sign(output);
                    Feature.WallTimer = Time.fixedTime + 0.1F;
                    Feature.JumpTimer = Feature.WallTimer;
                    break;
            }

            state &= ~State.碰撞;
            Apply();

            oldState |= state;
            if (isJump)
            {
                if (normalize.y <= 0)
                {
                    if ((oldState & State.平面) != 0 || isWalk)
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
            Machine.cachePosition = Machine.position;
            owner.Sender.LoadEffectServerRpc(Machine.position, Machine.velocity);
        }

        protected override void OnUpdate()
        {
            if (GameManager.MoveX != Feature.RushInput)
            {
                state &= ~State.横冲;
                return;
            }

            if (Distance(Machine.position, Machine.cachePosition) >= 3.5f)
            {
                Machine.cachePosition = Machine.position;
                owner.Sender.LoadEffectServerRpc(Machine.position, Machine.velocity);
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

    public class PlayerShuttle : PlayerState
    {
        protected override void OnEnter()
        {
            owner.Sender.SyncColorServerRpc(Color.skyBlue);
            Machine.cachePosition = Machine.position;
        }

        protected override void OnUpdate()
        {
            if (Distance(Machine.position, Machine.cachePosition) >= 0.5f)
            {
                Machine.cachePosition = Machine.position;
                owner.Sender.LoadEffectServerRpc(Machine.position, Machine.velocity);
            }

            velocityX = Feature.DashQuad.x * Feature.DashSpeed;
            velocityY = Feature.DashQuad.y * Feature.DashSpeed;
            Apply();
        }

        protected override void OnExit()
        {
            velocityX = 0;
            velocityY = 0;
            state &= ~State.穿梭;
        }
    }
}