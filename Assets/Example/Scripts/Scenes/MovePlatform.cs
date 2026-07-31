using Astraia;
using UnityEngine;

namespace Runtime
{
    public class MovePlatform : Export, IOnEnter, IOnExit, IEvent<OnPlatformUpdate>
    {
        private const float SPEED = 10f / 60f;

        [SerializeField] private Vector2 direction;
        [SerializeField] public Position velocity;
        [SerializeField] public Position position;

        [Export] private new BoxCollider2D collider;
        [Export] private new SpriteRenderer renderer;
        private Player owner;

        protected override void Awake()
        {
            direction = Vector2.right;
            position = transform.position.ToPosition();
        }

        public void Execute(OnPlatformUpdate message)
        {
            var normalize = direction.normalized;

            velocity = new Position(normalize.x, normalize.y) * SPEED;
            position += velocity;

            if (owner)
            {
                owner.Machine.position += velocity;
                owner.Machine.MovePosition(owner.Machine.position);
            }

            if (position.x > 14)
            {
                direction = Vector2.left;
            }

            if (position.x < -2)
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
                owner.Machine.position += velocity;
                owner.Machine.MovePosition(owner.Machine.position);
                Debug.Log("OnEnter");
            }
        }

        public void OnExit(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                owner = null;Debug.Log("OnExit");
                player.Machine.velocity += velocity;
            }
        }
    }
}