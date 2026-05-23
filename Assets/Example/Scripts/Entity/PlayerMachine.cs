using Astraia;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
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