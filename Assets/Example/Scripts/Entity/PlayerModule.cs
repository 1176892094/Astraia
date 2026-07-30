using System.Collections.Generic;
using Astraia;
using UnityEngine;

namespace Runtime
{
    public class PlayerModule : Module<Player>
    {
        private readonly HashSet<Collider2D> previous = new();
        private readonly HashSet<Collider2D> forwards = new();
        private Rigidbody rigidbody => owner.Machine;
        private Collider2D collision => rigidbody.collider;

        protected override void Enqueue()
        {
            previous.Clear();
            forwards.Clear();
        }

        public void Tick()
        {
            forwards.Clear();
            var velocity = rigidbody.velocity;
            var moveX = Fixation.Sign(velocity.x);
            var moveY = Fixation.Sign(velocity.y);
            if (moveY == 0)
            {
                moveY = -1;
            }

            foreach (var hit in rigidbody.collision.Boxcast(new Vector2(moveX, moveY), 0.01F, LayerConst.Collision))
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