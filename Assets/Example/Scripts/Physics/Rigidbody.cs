using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class Rigidbody : MonoBehaviour
    {
        private static readonly Enumerable<RaycastHit2D> Hits = new Enumerable<RaycastHit2D>(8);

        public const int SUB = 4;
        public const float FIX = 200;
        public const float PIX = 1 / 16F;
        public Collider2D Collider;
        public Vector2Int Position;
        public Vector2Int Velocity;
        private Bounds bounds => new Bounds(localPos + Collider.offset, Collider.bounds.size);
        private Vector2 localPos => new Vector3(positionX, positionY) / FIX;
        private Vector2 topLeft => new Vector2(minX, maxY);
        private Vector2 topRight => new Vector2(maxX, maxY);
        private Vector2 botLeft => new Vector2(minX, minY);
        private Vector2 botRight => new Vector2(maxX, minY);

        private float minX => bounds.center.x - bounds.extents.x - 0.01F;
        private float minY => bounds.center.y - bounds.extents.y - 0.01F;
        private float maxX => bounds.center.x + bounds.extents.x + 0.01F;
        private float maxY => bounds.center.y + bounds.extents.y + 0.01F;

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
            var worldPos = localPos;
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
            Hits.Count = Physics2D.BoxCast(localPos + direction / FIX, bounds.size, 0, direction, layerMask, Hits, distance / FIX);
            return Hits;
        }

        public Enumerable<RaycastHit2D> Raycast(Vector2 direction, float distance, ContactFilter2D layerMask)
        {
            Hits.Count = Physics2D.Raycast(localPos, direction, layerMask, Hits, distance / FIX);
            return Hits;
        }

        public Enumerable<RaycastHit2D> Raycast(Vector2 origin, Vector2 direction, float distance, ContactFilter2D layerMask)
        {
            Hits.Count = Physics2D.Raycast(origin, direction, layerMask, Hits, distance / FIX);
            return Hits;
        }

        public bool Checked(int moveSpeed, out float output)
        {
            var input = new Vector2(0, moveSpeed);
            var left = Raycast(topLeft, input.normalized, input.magnitude, LayerConst.Ground).Count > 0;
            var right = Raycast(topRight, input.normalized, input.magnitude, LayerConst.Ground).Count > 0;

            if (right && !left)
            {
                if (TrySubdivide(topRight, topLeft, input, out var subOffset))
                {
                    output = -subOffset;
                    return true;
                }
            }
            else if (left && !right)
            {
                if (TrySubdivide(topLeft, topRight, input, out var subOffset))
                {
                    output = subOffset;
                    return true;
                }
            }

            output = 0;
            return false;
        }

        private bool TrySubdivide(Vector2 p1, Vector2 p2, Vector2 input, out float offset)
        {
            offset = 0;
            for (var i = SUB - 1; i >= 0; i--)
            {
                var t = (float)i / SUB;
                var samplePoint = Vector2.Lerp(p1, p2, t);
                if (Raycast(samplePoint, input.normalized, input.magnitude, LayerConst.Ground).Count > 0)
                {
                    var nextT = (float)(i + 1) / SUB;
                    offset = (t + nextT) * 0.5f;
                    return true;
                }
            }

            return false;
        }

        public bool OverlapHold(int velocityX)
        {
            var moveX = Math.Sign(velocityX);
            if (moveX != 0)
            {
                var input = new Vector2(velocityX, 0);
                var p1 = moveX > 0 ? topRight : topLeft;
                var p2 = moveX > 0 ? botRight : botLeft;
                var r1 = Raycast(p1, input.normalized, input.magnitude, LayerConst.Ground).Count > 0;
                var r2 = Raycast(p2, input.normalized, input.magnitude, LayerConst.Ground).Count > 0;
                if (r2 && !r1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}