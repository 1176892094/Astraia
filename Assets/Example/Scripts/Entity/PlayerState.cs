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
    public abstract class PlayerState : State<Player>
    {
        protected const float FIX = 200;
        protected bool isWalk => InputManager.MoveX != 0 || InputManager.MoveY != 0;
        protected bool isWall => State.HasFlag(StateType.左墙) || State.HasFlag(StateType.右墙);
        protected bool isGround => isWall || State.HasFlag(StateType.地面);
        protected bool isGrab => isWall && State.HasFlag(StateType.攻击);
        protected bool isFall => !isWall && !State.HasFlag(StateType.地面) && velocityY < 0;
        protected Transform transform => owner.transform;
        protected PlayerMachine Machine => owner.Machine;
        protected PlayerFeature Feature => owner.Feature;

        protected int Direction
        {
            get => owner.Sender.Direction;
            set => owner.Sender.Direction = value;
        }

        protected StateType State
        {
            get => owner.State;
            set => owner.State = value;
        }

        protected int velocityX
        {
            get => Machine.velocityX;
            set => Machine.velocityX = value;
        }

        protected int velocityY
        {
            get => Machine.velocityY;
            set => Machine.velocityY = value;
        }

        private int positionX
        {
            get => Machine.positionX;
            set => Machine.positionX = value;
        }

        private int positionY
        {
            get => Machine.positionY;
            set => Machine.positionY = value;
        }

        private int Lerp(float begin, float end, float delta)
        {
            return (int)Mathf.Lerp(begin, end, delta);
        }

        protected void Move(int speed)
        {
            var moveX = InputManager.MoveX;
            if (moveX != 0)
            {
                speed *= moveX;
                Direction = moveX;
                if (Mathf.Abs(velocityX) < Mathf.Abs(speed))
                {
                    velocityX = speed;
                }

                velocityX = Lerp(velocityX, speed, 0.2F);
            }
            else if (Mathf.Abs(velocityX) > 0.01F)
            {
                velocityX = Lerp(velocityX, 0, 0.2F);
            }
            else
            {
                velocityX = 0;
            }

            Gravity();
            Contact();
        }

        protected void Gravity()
        {
            if (State.HasFlag(StateType.地面))
            {
                return;
            }

            if (State.HasFlag(StateType.抓墙))
            {
                velocityY = Lerp(velocityY, -10, 0.02F);
                return;
            }

            if (State.HasFlag(StateType.缓冲))
            {
                velocityY = Lerp(velocityY, -20, 0.02F);
                return;
            }

            velocityY = Lerp(velocityY, -20, 0.04F);
        }

        protected void Contact()
        {
            State &= ~(StateType.地面 | StateType.左墙 | StateType.右墙 | StateType.头顶);

            var extents = Machine.collider.bounds.extents;
            var velocity = new Vector2(velocityX, velocityY);
            foreach (var contact in Machine.collider.Contacts(velocity))
            {
                var point = contact.point;
                var normal = contact.normal;
                if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x))
                {
                    if (normal.y > 0)
                    {
                        if (!State.HasFlag(StateType.跳跃))
                        {
                            Feature.JumpCount = 1;
                        }

                        if (!State.HasFlag(StateType.冲刺))
                        {
                            Feature.DashCount = 1;
                        }

                        State |= StateType.地面;
                        velocityY = Mathf.Max(velocityY, 0);
                        positionY = (int)((point.y + extents.y) * FIX);
                    }
                    else
                    {
                        State |= StateType.头顶;
                        velocityY = Mathf.Min(velocityY, 0);
                        positionY = (int)((point.y - extents.y) * FIX);
                    }
                }
                else
                {
                    if (!State.HasFlag(StateType.跳跃))
                    {
                        Feature.JumpCount = 1;
                    }

                    if (normal.x > 0)
                    {
                        State |= StateType.左墙;
                        velocityX = Mathf.Max(velocityX, 0);
                        positionX = (int)((point.x + extents.x) * FIX);
                    }
                    else
                    {
                        State |= StateType.右墙;
                        velocityX = Mathf.Min(velocityX, 0);
                        positionX = (int)((point.x - extents.x) * FIX);
                    }
                }
            }

            positionX += velocityX;
            positionY += velocityY;
            transform.position = new Vector3(positionX, positionY) / FIX;
        }

        protected float Dash()
        {
            var extents = Machine.collider.bounds.extents;
            var velocity = new Vector2(velocityX, velocityY);
            foreach (var contact in Machine.collider.Contacts(velocity))
            {
                var bounds = contact.collider.bounds;
                if (positionX > (int)((bounds.max.x - extents.x) * FIX))
                {
                    positionX = (int)((bounds.max.x + extents.x) * FIX);
                    return 0;
                }

                if (positionX < (int)((bounds.min.x + extents.x) * FIX))
                {
                    positionX = (int)((bounds.min.x - extents.x) * FIX);
                    return 0;
                }
            }

            return Direction * Feature.DashSpeed;
        }

        protected void Hold()
        {
            var position = transform.position;
            var extents = Machine.collider.bounds.extents;
            var velocity = new Vector2(velocityX, velocityY);
            foreach (var contact in Machine.collider.Contacts(velocity))
            {
                var bounds = contact.collider.bounds;
                if (position.y > bounds.max.y - extents.y)
                {
                    position.y = Mathf.Lerp(position.y, bounds.max.y + extents.y, 0.2F);
                    transform.position = position;
                }

                if (position.x < bounds.min.x)
                {
                    position.x = Mathf.Lerp(position.x, bounds.min.x - extents.x, 0.2F);
                    transform.position = position;
                }

                if (position.x > bounds.max.x)
                {
                    position.x = Mathf.Lerp(position.x, bounds.max.x - extents.x, 0.2F);
                    transform.position = position;
                }
            }
        }
    }

    public class PlayerIdle : PlayerState
    {
        public override void OnEnter()
        {
            Feature.DashStack = 0;
            owner.Sender.SyncColorServerRpc(Color.white);
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
        private float waitTime;

        public override void OnEnter()
        {
            waitTime = Time.fixedTime + 0.1F;
            owner.Sender.SyncColorServerRpc(Color.yellow);

            if (isWall && !State.HasFlag(StateType.地面))
            {
                State |= StateType.蹬墙跳;
                Direction = -owner.Sender.Direction;
                velocityX = Direction * Feature.JumpForce / 2;
            }

            Feature.JumpCount--;
            State |= StateType.跳跃;
            velocityY = Mathf.Max(Machine.velocityY + Feature.JumpForce, Feature.JumpForce);
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer > Time.fixedTime)
            {
                if (InputManager.MoveY != 1)
                {
                    if (State.HasFlag(StateType.地面) && isWalk)
                    {
                        Machine.Switch(Animations.Crash);
                        return;
                    }
                }

                if (isWall && State.HasFlag(StateType.蹬墙跳))
                {
                    State |= StateType.超级跳;
                    State &= ~StateType.蹬墙跳;

                    if (State.HasFlag(StateType.左墙))
                    {
                        velocityX = Feature.JumpForce;
                    }
                    else
                    {
                        velocityX = -Feature.JumpForce;
                    }

                    velocityY *= InputManager.MoveY >= 0 ? 1 : -1;
                }
            }

            if (isGrab)
            {
                Machine.Switch(Animations.Grab);
                return;
            }

            if (isFall)
            {
                Machine.Switch(Animations.Fall);
            }

            if (waitTime < Time.fixedTime)
            {
                if (isGround)
                {
                    Machine.Switch(Animations.Idle);
                    return;
                }
            }
            else
            {
                if (State.HasFlag(StateType.超级跳))
                {
                    if (Feature.ShadowIndex++ % 4 == 0)
                    {
                        owner.Sender.LoadEffectServerRpc(transform.position);
                    }

                    return;
                }

                if (State.HasFlag(StateType.蹬墙跳))
                {
                    return;
                }
            }


            Move((int)(Feature.MoveSpeed * 1.2F));
        }

        public override void OnExit()
        {
            State &= ~StateType.跳跃;
            State &= ~StateType.超级跳;
            State &= ~StateType.蹬墙跳;
        }
    }

    public class PlayerFall : PlayerState
    {
        public override void OnEnter()
        {
            State |= StateType.下落;
            owner.Sender.SyncColorServerRpc(Color.red);
        }

        public override void OnUpdate()
        {
            if (isGrab)
            {
                Machine.Switch(Animations.Grab);
                return;
            }

            if (!isFall)
            {
                Machine.Switch(Animations.Idle);
            }

            Move((int)(Feature.MoveSpeed * 1.2F));
        }

        public override void OnExit()
        {
            State &= ~StateType.下落;
        }
    }

    public class PlayerGrab : PlayerState
    {
        public override void OnEnter()
        {
            State |= StateType.抓墙;
            owner.Sender.SyncColorServerRpc(Color.cyan);
        }

        public override void OnUpdate()
        {
            if (!isGrab)
            {
                Machine.Switch(Animations.Idle);
                return;
            }

            if (isFall)
            {
                Machine.Switch(Animations.Fall);
                return;
            }

            Move(Feature.MoveSpeed);
        }

        public override void OnExit()
        {
            State &= ~StateType.抓墙;
        }
    }

    public class PlayerDash : PlayerState
    {
        private Vector2 velocity;

        public override void OnEnter()
        {
            Feature.DashCount--;
            Feature.DashTimer = Time.fixedTime + 0.24F;
            owner.Sender.SyncColorServerRpc(Color.magenta);
            State |= StateType.冲刺;
            Feature.ShadowIndex = 0;
            velocity = InputManager.Direction;
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer < Time.fixedTime)
            {
                Machine.Switch(Animations.Idle);
                return;
            }

            if (Feature.ShadowIndex++ % 4 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            var moveX = velocity.x * Feature.DashSpeed;
            var moveY = velocity.y * Feature.DashSpeed;

            if (velocity == Vector2.zero)
            {
                moveX = Direction * Feature.DashSpeed;
            }
            else if (velocity.y < 0)
            {
                Contact();
                if (State.HasFlag(StateType.地面))
                {
                    moveX = Dash();
                }
            }
            else if (velocity.y > 0)
            {
                Contact();
                if (State.HasFlag(StateType.头顶))
                {
                    Dash();
                }
            }

            velocityX = (int)moveX;
            velocityY = (int)moveY;
        }

        public override void OnExit()
        {
            velocityX = 0;
            velocityY = 0;
            State &= ~StateType.冲刺;
        }
    }

    public class PlayerCrash : PlayerState
    {
        private float waitTime;
        private int moveX;

        public override void OnEnter()
        {
            State |= StateType.冲刺跳;
            waitTime = Time.fixedTime + 0.1F;
            moveX = Direction;
            velocityX = Direction * (Feature.CrashSpeed + Feature.CrashSpeed * Feature.DashStack / 4);
            Feature.DashStack++;
        }

        public override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                if (isGround)
                {
                    Machine.Switch(Animations.Idle);
                    return;
                }

                if (moveX != InputManager.MoveX)
                {
                    Machine.Switch(Animations.Idle);
                    return;
                }
            }

            if (Feature.ShadowIndex++ % 4 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            Contact();
            Gravity();
        }

        public override void OnExit()
        {
            State &= ~StateType.冲刺跳;
        }
    }

    public class PlayerHold : PlayerState
    {
        private float waitTime;


        public override void OnEnter()
        {
            waitTime = Time.fixedTime + 0.3f;
            owner.Sender.SyncColorServerRpc(Color.red);
            State |= StateType.抓墙;
        }

        public override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                Machine.Switch(Animations.Idle);
                return;
            }

            Contact();
            if (State.HasFlag(StateType.左墙) && InputManager.MoveX < 0)
            {
                Hold();
            }
            else if (State.HasFlag(StateType.右墙) && InputManager.MoveX > 0)
            {
                Hold();
            }
        }

        public override void OnExit()
        {
            State &= ~StateType.抓墙;
        }
    }
}