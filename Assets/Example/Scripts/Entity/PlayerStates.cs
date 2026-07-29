using UnityEngine;

namespace Runtime
{
    public class PlayerIdle : PlayerState
    {
        protected override void OnEnter()
        {
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
                if ((state & State.竖冲) != 0)
                {
                    velocityX = Feature.GrabInput * Feature.JumpForce;
                }
                else
                {
                    velocityX = Feature.GrabInput * Feature.GrabForce;
                }

                direction = Feature.GrabInput;
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
}