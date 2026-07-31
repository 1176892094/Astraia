using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class MovePlatform : Export, IOnEnter, IOnExit
    {
        private const float SPEED = 1.5F / 60;

        [SerializeField] private float velocityX;
        [SerializeField] private float velocityY;
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
            velocityX = Mathf.Lerp(velocityX, normalize.x * SPEED, 0.2F);
            velocityY = Mathf.Lerp(velocityY, normalize.y * SPEED, 0.2F);
            var velocity = new Position(velocityX, velocityY);
            position += velocity;

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
                //   owner.Machine.syncVelocity = new Position(velocityX, velocityY);
                owner = null;
            }
        }
    }
}