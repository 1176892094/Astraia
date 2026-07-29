using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public partial class Rigidbody : Module<Entity>
    {
        private static readonly Collider2D[] overlaps = new Collider2D[16];

        private float pixelate;
        private Vector2 smoothStep;
        private Collider2D collider;

        public Position position;
        public Position velocity;
        public Position syncPosition;
        public Position syncVelocity;
        public Position startPosition;
        public Collision collision => new Collision(position.ToVector2(), collider.bounds.size, collider.offset);

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
        }

        public void SetPixelate(Camera camera)
        {
            pixelate = 1 / (camera.targetTexture.height / (camera.orthographicSize * 2));
        }

        public void InitPosition(Vector3 worldPos)
        {
            position = worldPos.ToPosition();
            syncPosition = position;
            startPosition = position;
            MovePosition(position);
        }

        public void MovePosition(Vector3 worldPos)
        {
            position = worldPos.ToPosition();
            MovePosition(position);
        }

        public void MovePosition(Position position)
        {
            var worldPos = position.ToVector2();
            worldPos.x = Mathf.Round(worldPos.x / pixelate) * pixelate;
            worldPos.y = Mathf.Round(worldPos.y / pixelate) * pixelate;
            owner.transform.position = worldPos;
        }

        public void SyncPosition()
        {
            var worldPos = syncPosition.ToVector2();
            worldPos = Vector2.SmoothDamp(owner.transform.position, worldPos, ref smoothStep, Time.fixedDeltaTime);
            worldPos.x = Mathf.Round(worldPos.x / pixelate) * pixelate;
            worldPos.y = Mathf.Round(worldPos.y / pixelate) * pixelate;
            owner.transform.position = worldPos;
        }

        public void ResolveOverlap(ContactFilter2D filter)
        {
            for (var i = 0; i < 4; i++)
            {
                if (!collision.Decelerate(collider, filter, out var offset))
                {
                    return;
                }

                position += offset.ToPosition();
            }
        }
    }
}