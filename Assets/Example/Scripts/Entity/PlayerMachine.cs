using System;
using Astraia;

namespace Runtime
{
    [Serializable]
    public class PlayerMachine : Rigidbody
    {
        private readonly StateMachine<int> machine = new StateMachine<int>();

        protected override void Enqueue()
        {
            machine.Dispose();
        }

        public void Tick()
        {
            machine.Update();
        }

        public void Create<T>(int key) 
        {
            machine.Create<T>(owner, key);
        }

        public void Switch(int key)
        {
            machine.Switch(key);
        }
    }
}