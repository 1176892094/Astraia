using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public partial class Rigidbody : Module<Entity>
    {
        private const float PIXELATE = 1 / 16F;

        private static readonly Enumerable<Collider2D> Hits = new Enumerable<Collider2D>(16);
        private Vector2 smoothStep;

        public Collider2D collider;
        public Position position;
        public Position velocity;
        public Position syncPosition;
      
        public Position dashPosition;
        public Collision collision => new Collision(position.ToVector2(), collider.bounds.extents, collider.offset);

        public Fixation positionX
        {
            get => position.x;
            set => position = new Position(value, position.y);
        }

        public Fixation positionY
        {
            get => position.y;
            set => position = new Position(position.x, value);
        }

        public Fixation velocityX
        {
            get => velocity.x;
            set => velocity = new Position(value, velocity.y);
        }

        public Fixation velocityY
        {
            get => velocity.y;
            set => velocity = new Position(velocity.x, value);
        }

        protected override void Dequeue()
        {
            collider = owner.GetComponent<Collider2D>();
            MovePosition(owner.transform.position);
        }

        public void MovePosition(Vector3 worldPos)
        {
            position = worldPos.ToPosition();
            MovePosition(position);
        }

        public void MovePosition(Position position)
        {
            syncPosition = position;
            var worldPos = position.ToVector2();
            // worldPos.x = Mathf.Round(worldPos.x / PIXELATE) * PIXELATE;
            // worldPos.y = Mathf.Round(worldPos.y / PIXELATE) * PIXELATE;
            owner.transform.position = worldPos;
        }

        public void SyncPosition()
        {
            var worldPos = syncPosition.ToVector2();
            worldPos = Vector2.SmoothDamp(owner.transform.position, worldPos, ref smoothStep, Time.fixedDeltaTime);
            // worldPos.x = Mathf.Round(worldPos.x / PIXELATE) * PIXELATE;
            // worldPos.y = Mathf.Round(worldPos.y / PIXELATE) * PIXELATE;
            owner.transform.position = worldPos;
        }
        public bool Contains(Bounds b)
        {
            var a = collision.bounds;
            if (a.Intersects(b))
            {
                var dx = a.center.x - b.center.x;
                var px = a.extents.x + b.extents.x - Mathf.Abs(dx);

                var dy = a.center.y - b.center.y;
                var py = a.extents.y + b.extents.y - Mathf.Abs(dy);

                if (px < py)
                {
                    positionX = a.center.x > b.center.x ? b.max.x + a.extents.x + 0.01f : b.min.x - a.extents.x - 0.01f;
                }
                else
                {
                    positionY = a.center.y > b.center.y ? b.max.y + a.extents.y + 0.01f : b.min.y - a.extents.y - 0.01f;
                }

                return true;
            }

            return false;
        }

        public Enumerable<Collider2D> Overlap(ContactFilter2D filter)
        {
            var bounds = collision.bounds;
            Hits.Count = Physics2D.OverlapBox(bounds.center, bounds.size, 0, filter, Hits);
            return Hits;
        }
    }
}