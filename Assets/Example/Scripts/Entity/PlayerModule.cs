using System.Collections.Generic;
using Astraia;
using UnityEngine;

namespace Runtime
{
    public class PlayerModule : Export
    {
        private readonly HashSet<Collider2D> previous = new();
        private readonly HashSet<Collider2D> forwards = new();

        [Export] private Rigidbody rigidbody;
        [Export] private Collider2D collision;

        protected override void OnDestroy()
        {
            previous.Clear();
            forwards.Clear();
        }

        public void Tick()
        {
            forwards.Clear();
            foreach (var hit in rigidbody.collision.Boxcast(Vector2.down, 0.2F, LayerConst.Collision))
            {
                var other = hit.collider;
                if (other && !previous.Contains(other))
                {
                    other.GetComponent<IOnEnter>()?.OnEnter(collision);
                }

                forwards.Add(other);
            }

            foreach (var other in previous)
            {
                if (other && !forwards.Contains(other))
                {
                    other.GetComponent<IOnExit>()?.OnExit(collision);
                }
            }

            previous.Clear();
            foreach (var other in forwards)
            {
                previous.Add(other);
            }
        }
    }

    public interface IOnEnter
    {
        void OnEnter(Collider2D other);
    }

    public interface IOnExit
    {
        void OnExit(Collider2D other);
    }
}