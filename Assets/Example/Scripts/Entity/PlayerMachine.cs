using Astraia;

namespace Runtime
{
    public class PlayerMachine : Rigidbody
    {
        private readonly StateMachine<int> machine = new StateMachine<int>();
        private Player owner;

        protected override void Awake()
        {
            base.Awake();
            owner = GetComponent<Player>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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
}