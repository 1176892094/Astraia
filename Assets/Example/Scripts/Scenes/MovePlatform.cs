using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class MovePlatform : Export, IOnEnter, IOnExit
    {
        private const float SPEED = 2.5F / 60;

        private Player owner;
        [SerializeField] private Position velocity;
        [SerializeField] private Position position;
        [SerializeField] private Vector2 direction;

        [Export] private new BoxCollider2D collider;
        [Export] private new SpriteRenderer renderer;

        protected override void Awake()
        {
            direction = Vector2.right;
            position = transform.position.ToPosition();
        }

        private void FixedUpdate()
        {
            var positionX = position.x;
            var normalize = direction.normalized;
            var velocityX = Mathf.Lerp(velocity.x, normalize.x * SPEED, 0.2F);
            var velocityY = Mathf.Lerp(velocity.y, normalize.y * SPEED, 0.2F);
            velocity = new Position(velocityX, velocityY);
            position += velocity;

            if (owner)
            {
                owner.Machine.syncVelocity = velocity;
            }

            if (positionX < 15 && position.x > 15)
            {
                direction = Vector2.left;
            }

            if (positionX > -3 && position.x < -3)
            {
                direction = Vector2.right;
            }

            transform.position = position.ToVector2();
        }

        public void OnEnter(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                owner = player;
            }
        }

        public void OnExit(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                owner = null;
            }
        }
    }
}