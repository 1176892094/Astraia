using Astraia;
using UnityEngine;

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
}