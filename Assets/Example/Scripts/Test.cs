using System;
using System.Collections.Generic;
using UnityEngine;

namespace Example.Scripts
{
    public class Entity : MonoBehaviour
    {
        [SerializeReference] public List<IModule> Modules = new List<IModule>();
    }

    public interface IAcquire
    {
        void Acquire(object owner);
    }

    public interface IModule : IAcquire
    {
        void Dequeue();
        void Enqueue();
    }

    public abstract class Module<T> : IModule
    {
        public T owner;

        void IAcquire.Acquire(object owner)
        {
            this.owner = (T)owner;
        }

        public virtual void Enqueue()
        {
        }

        public virtual void Dequeue()
        {
        }
    }
    
    public class Player
    {
        public int Id;
    }

    [Serializable]
    public class PlayerModule : Module<Player>
    {
        public string Name;
    }
}