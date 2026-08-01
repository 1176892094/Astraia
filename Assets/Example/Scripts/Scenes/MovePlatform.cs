using Astraia;
using UnityEngine;

namespace Runtime
{
    public class MovePlatform : Export, IOnEnter, IOnExit, IEvent<OnPlatformUpdate>
    {
        private const float SPEED = 3f / 60f;

        private Position position;
        private Vector3 startPos;
        private Vector3 endPos;

        private float t;
        private bool forward = true;

        [Export] private Transform startPoint;
        [Export] private Transform endPoint;

        [Export] private new BoxCollider2D collider;
        [Export] private new SpriteRenderer renderer;

        private Player owner;

        private Vector2 lastPosition;

        protected override void Awake()
        {
            startPos = startPoint.position;
            endPos = endPoint.position;

            position = transform.position.ToPosition();

            lastPosition = transform.position;
        }

        public void Execute(OnPlatformUpdate message)
        {
            MovePlatforms();
            var delta = transform.position - (Vector3)lastPosition;
            if (owner)
            {
                owner.Machine.Contains(collider.bounds);
                owner.Machine.position += delta.ToPosition();
                owner.Machine.MovePosition(owner.Machine.position);
            }

            lastPosition = transform.position;
        }

        private void MovePlatforms()
        {
            var dir = forward ? endPos - startPos : startPos - endPos;

            var velocity = dir.normalized.ToPosition() * SPEED;

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

            position = new Position(pos.x, pos.y);
            transform.position = pos;
        }

        public void OnEnter(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                owner = player;
                player.Feature.State |= State.加速;
            }
        }

        public void OnExit(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                owner = null;
                player.Feature.State &= ~State.加速;
            }
        }
    }
}