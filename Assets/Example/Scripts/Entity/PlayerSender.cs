using System;
using Astraia;
using Astraia.Core;
using Astraia.Net;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
    [Serializable]
    public class PlayerSender : NetworkModule, IStartAuthority
    {
        public new Player owner => (Player)base.owner;

        [SyncVar(nameof(OnValueChanged))] public Color32 color;

        public int Direction
        {
            get
            {
                if (transform.localScale.x > 0)
                {
                    return 1;
                }

                if (transform.localScale.x < 0)
                {
                    return -1;
                }

                return 0;
            }
            set
            {
                if (value > 0 && Direction < 0)
                {
                    if (isOwner)
                    {
                        SetDirectionServerRpc(1);
                    }
                    else if (isServer)
                    {
                        SetDirectionClientRpc(1);
                    }

                    transform.localScale = new Vector3(1, 1, 1);
                    return;
                }

                if (value < 0 && Direction > 0)
                {
                    if (isOwner)
                    {
                        SetDirectionServerRpc(-1);
                    }
                    else if (isServer)
                    {
                        SetDirectionClientRpc(-1);
                    }

                    transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }

        private void OnValueChanged(Color32 oldValue, Color32 newValue)
        {
            GetComponent<PlayerMachine>().renderer.color = newValue;
        }

        [ServerRpc]
        public void SyncColorServerRpc(Color32 color)
        {
            this.color = color;
        }

        [ServerRpc]
        private void SetDirectionServerRpc(int direction)
        {
            SetDirectionClientRpc(direction);
        }

        [ClientRpc(Channel.Reliable | Channel.IgnoreOwner)]
        private void SetDirectionClientRpc(int direction)
        {
            transform.localScale = new Vector3(direction, 1, 1);
        }

        [ServerRpc]
        public void LoadEffectServerRpc(Vector3 position)
        {
            LoadEffectClientRpc(position);
        }

        [ClientRpc]
        public async void LoadEffectClientRpc(Vector3 position)
        {
            var sprite = PoolManager.Show<SpriteRenderer>("Prefabs/Shadow", position);
            sprite.color = new Color(0, 0, 0, 1);
            await sprite.DOFade(0, 0.5f);
            PoolManager.Hide(sprite);
        }

        public void OnStartAuthority()
        {
            GameManager.Instance.SetPlayer(transform);
            GameManager.Instance.SetBounds(new Bounds(Vector3.zero, new Vector3(13, 6)));
            owner.Input = gameObject.AddComponent<PlayerInput>();
            var machine = GetComponent<PlayerMachine>();
            machine.Create<PlayerIdle>(Animations.Idle);
            machine.Create<PlayerWalk>(Animations.Walk);
            machine.Create<PlayerJump>(Animations.Jump);
            machine.Create<PlayerFall>(Animations.Fall);
            machine.Create<PlayerGrab>(Animations.Grab);
            machine.Create<PlayerDash>(Animations.Dash);
            machine.Create<PlayerCrash>(Animations.Crash);
            machine.Switch(Animations.Idle);
        }
    }

    public class PlayerMachine : MonoBehaviour
    {
        private readonly StateMachine<int> machine = new StateMachine<int>();
        private Player owner;
        public new Collider2D collider;
        public new SpriteRenderer renderer;

        private void Awake()
        {
            owner = GetComponent<Player>();
            collider = GetComponent<Collider2D>();
            renderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void OnDestroy()
        {
            machine.Clear();
        }

        public void Tick()
        {
            machine.Update();
        }

        public void Create<T>(int key) where T : IState
        {
            machine.Create<T>(owner, key);
        }

        public void Switch(int key)
        {
            machine.Switch(key);
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

        public void OnDrawGizmos()
        {
            var other = GetComponent<Collider2D>();
            var bounds = other.bounds;
            var velocity = new Vector2(VelocityX, VelocityY);
            Vector2 origin = bounds.center;
            Vector2 size = bounds.size;
            var direction = velocity.normalized;
            var distance = velocity.magnitude / 100f;
            var endPosition = origin + direction * distance;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(endPosition, size);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, endPosition);
        }
    }

    public class PlayerInput : MonoBehaviour
    {
        private Player owner;
        private PlayerFeature Feature => owner.Feature;
        private PlayerMachine Machine => owner.Machine;

        private State State
        {
            get => owner.State;
            set => owner.State = value;
        }

        private void Awake()
        {
            owner = GetComponent<Player>();
        }

        private void OnEnable()
        {
            InputManager.Dash.performed += DashButton;
            InputManager.Jump.started += JumpButton;
            InputManager.Jump.canceled += FallButton;
        }

        private void OnDisable()
        {
            InputManager.Dash.performed -= DashButton;
            InputManager.Jump.started -= JumpButton;
            InputManager.Jump.canceled -= FallButton;
        }

        private void DashButton(InputAction.CallbackContext obj)
        {
            Feature.DashInput = Time.time + 0.2f;
        }

        private void JumpButton(InputAction.CallbackContext obj)
        {
            State |= State.缓冲;
            Feature.JumpInput = Time.time + 0.2f;
        }

        private void FallButton(InputAction.CallbackContext obj)
        {
            State &= ~State.缓冲;
        }

        public void Tick()
        {
            DashUpdate();
            JumpUpdate();
        }

        private void DashUpdate()
        {
            if (Feature.DashCount <= 0)
            {
                return;
            }

            if (Feature.DashInput < Time.time)
            {
                return;
            }

            if (Feature.DashCD > Time.time)
            {
                return;
            }

            if (State.HasFlag(State.冲刺))
            {
                return;
            }

            Machine.Switch(Animations.Dash);
            Feature.DashCD = Time.time + 0.4F;
        }

        private void JumpUpdate()
        {
            if (State.HasFlag(State.地面))
            {
                Feature.JumpTimer = Time.time + 0.2F;
            }

            if (State.HasFlag(State.左墙) || State.HasFlag(State.右墙))
            {
                Feature.JumpTimer = Time.time + 0.2F;
            }

            if (Feature.JumpTimer < Time.time)
            {
                return;
            }

            if (Feature.JumpCount <= 0)
            {
                return;
            }

            if (Feature.JumpInput < Time.time)
            {
                return;
            }

            if (Feature.JumpCD > Time.time)
            {
                return;
            }

            if (State.HasFlag(State.跳跃))
            {
                return;
            }

            Machine.Switch(Animations.Jump);
            Feature.JumpCD = Time.time + 0.3F;
        }
    }
}