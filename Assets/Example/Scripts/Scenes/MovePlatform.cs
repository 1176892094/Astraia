using Astraia;
using UnityEngine;

namespace Runtime
{
    public class MovePlatform : Export, IOnEnter, IOnExit
    {
        private const float SPEED = 2.5f / 60f;

        private Player owner;

        [SerializeField] private Position velocity;
        [SerializeField] private Position lastVelocity;
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
            var normalize = direction.normalized;
            velocity = new Position(Mathf.Lerp(velocity.x, normalize.x * SPEED, 0.2f), Mathf.Lerp(velocity.y, normalize.y * SPEED, 0.2f));
            position += velocity;

            if (owner)
            {
                lastVelocity = velocity;
                owner.Machine.externalVelocity = velocity;
            }

            if (position.x > 15)
            {
                direction = Vector2.left;
            }

            if (position.x < -3)
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
                lastVelocity = velocity;
                player.Machine.externalVelocity = velocity;
            }
        }

        public void OnExit(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                owner = null;
                player.Machine.velocity += lastVelocity;
                player.Machine.externalVelocity = Position.Zero;
            }
        }
    }
}