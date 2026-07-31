using Astraia;
using UnityEngine;

namespace Runtime
{
    public class MovePlatform : Export, IOnEnter, IOnExit, IEvent<OnPlatformUpdate>
    {
        private const float SPEED = 5f / 60f;

        private Player owner;

        [SerializeField] private Position velocity;
        [SerializeField] private Position position;
        [SerializeField] private Position carryVelocity;
        [SerializeField] private Vector2 direction;

        [Export] private new BoxCollider2D collider;
        [Export] private new SpriteRenderer renderer;

        protected override void Awake()
        {
            direction = Vector2.right;
            position = transform.position.ToPosition();
        }

        public void Execute(OnPlatformUpdate message)
        {
            var normalize = direction.normalized;

            velocity = new Position(Mathf.Lerp(velocity.x, normalize.x * SPEED, 0.2f), Mathf.Lerp(velocity.y, normalize.y * SPEED, 0.2f));

            var delta = velocity;

            if (owner)
            {
                carryVelocity = velocity;

                owner.Apply(delta);
            }

            position += velocity;

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
                carryVelocity = velocity;
            }
        }

        public void OnExit(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                player.Machine.velocity += carryVelocity;
                carryVelocity = Position.Zero;
                owner = null;
            }
        }
    }
}