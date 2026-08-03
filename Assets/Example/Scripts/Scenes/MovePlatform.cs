using Astraia;
using UnityEngine;

namespace Runtime
{
    public class MovePlatform : Export, IOnEnter, IOnExit, IEvent<OnPlatformUpdate>
    {
        private const float SPEED = 6f / 60f;

        private Position position;

        public Position velocity;

        private Vector3 startPos;
        private Vector3 endPos;

        private float t;
        private bool forward = true;

        [Export] private Transform startPoint;
        [Export] private Transform endPoint;
        [Export] private BoxCollider2D collider;
        [Export] private SpriteRenderer renderer;
        

        protected override void Awake()
        {
            startPos = startPoint.position;
            endPos = endPoint.position;

            position = transform.position.ToPosition();
        }

        public void Execute(OnPlatformUpdate message)
        {
            var dir = forward ? endPos - startPos : startPos - endPos;

            velocity = dir.normalized.ToPosition() * SPEED;
            position += velocity;

            var pos = position.ToVector2();
            if (forward)
            {
                if (Vector2.Distance(pos, endPos) < SPEED)
                {
                    pos = endPos;
                    forward = false;
                }
            }
            else
            {
                if (Vector2.Distance(pos, startPos) < SPEED)
                {
                    pos = startPos;
                    forward = true;
                }
            }

            transform.position = pos;
        }

        public void OnEnter(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                player.Feature.platform = this;
            }
        }

        public void OnExit(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                player.Feature.platform = null;
                player.Machine.velocity += velocity;
            }
        }
    }
}