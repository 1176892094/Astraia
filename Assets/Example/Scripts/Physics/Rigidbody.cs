using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class Rigidbody : MonoBehaviour
    {
        private static readonly Enumerable<RaycastHit2D> Hits = new Enumerable<RaycastHit2D>(8);
        public const float FIX = 250;
        public const float PIX = 1 / 16F;
        public Vector2Int Position;
        public Vector2Int Velocity;
        public Collider2D Collider;
        private Bounds bounds => new Bounds(position + Collider.offset, Collider.bounds.size);
        private Vector2 position => new Vector3(positionX, positionY) / FIX;
        private Vector2 topLeft => new Vector2(minX, maxY);
        private Vector2 topRight => new Vector2(maxX, maxY);
        private Vector2 botLeft => new Vector2(minX, minY);
        private Vector2 botRight => new Vector2(maxX, minY);
        private float minX => bounds.center.x - bounds.extents.x;
        private float minY => bounds.center.y - bounds.extents.y;
        private float maxX => bounds.center.x + bounds.extents.x;
        private float maxY => bounds.center.y + bounds.extents.y;

        public int positionX
        {
            get => Position.x;
            set => Position.x = value;
        }

        public int positionY
        {
            get => Position.y;
            set => Position.y = value;
        }

        public int velocityX
        {
            get => Velocity.x;
            set => Velocity.x = value;
        }

        public int velocityY
        {
            get => Velocity.y;
            set => Velocity.y = value;
        }

        protected virtual void Awake()
        {
            MovePosition(transform.position);
        }

        public void MovePosition()
        {
            var worldPos = position;
            worldPos.x = Mathf.Round(worldPos.x / PIX) * PIX;
            worldPos.y = Mathf.Round(worldPos.y / PIX) * PIX;
            transform.position = worldPos;
        }

        public void MovePosition(Vector2 worldPos)
        {
            positionX = Mathf.RoundToInt(worldPos.x * FIX);
            positionY = Mathf.RoundToInt(worldPos.y * FIX);
            MovePosition();
        }

        public Enumerable<RaycastHit2D> Boxcast(Vector2 direction, float distance, ContactFilter2D layerMask)
        {
            Hits.Count = Physics2D.BoxCast(position + direction / FIX, bounds.size, 0, direction, layerMask, Hits, distance / FIX);
            return Hits;
        }

        public Enumerable<RaycastHit2D> Raycast(Vector2 direction, float distance, ContactFilter2D layerMask)
        {
            Hits.Count = Physics2D.Raycast(position, direction, layerMask, Hits, distance / FIX);
            return Hits;
        }

        public int OverlapUp()
        {
            return Overlap(topLeft, topRight, new Vector2(0, 0.1F));
        }

        public int OverlapDown()
        {
            return Overlap(botLeft, botRight, new Vector2(0, -0.1F));
        }

        public int OverlapLeft()
        {
            return Overlap(topLeft, botLeft, new Vector2(-0.1F, 0));
        }

        public int OverlapRight()
        {
            return Overlap(topRight, botRight, new Vector2(0.1F, 0));
        }

        private static int Overlap(Vector2 p1, Vector2 p2, Vector2 offset)
        {
            int state = 0;
            if (Physics2D.OverlapPoint(p1 + offset))
            {
                state |= 1;
            }

            if (Physics2D.OverlapPoint(p2 + offset))
            {
                state |= 2;
            }

            return state;
        }
    }
}