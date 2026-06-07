using System;
using Astraia;

namespace Runtime
{
    [Serializable]
    public class PlayerMachine : Rigidbody
    {
        private readonly StateMachine<int> machine = new StateMachine<int>();

        public override void Enqueue()
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
}