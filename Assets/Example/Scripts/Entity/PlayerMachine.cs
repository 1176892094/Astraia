using System;
using Astraia;

namespace Runtime
{
    [Serializable]
    public class PlayerMachine : Rigidbody
    {
        private readonly StateMachine machine = new StateMachine();

        protected override void Enqueue()
        {
            machine.Clear();
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