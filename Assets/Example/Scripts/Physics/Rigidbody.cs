using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public partial class Rigidbody : Module<Entity>
    {
        private float pixelate = 1 / 16F;
        private Vector2 smoothStep;
        
        public Collider2D collider;
        public Position position;
        public Position velocity;
        public Position syncPosition;
        public Position syncVelocity;
        public Position cachePosition;
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
            InitPosition(owner.transform.position);
        }

        public void InitPosition(Vector3 worldPos)
        {
            position = worldPos.ToPosition();
            syncPosition = position;
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
    }
}