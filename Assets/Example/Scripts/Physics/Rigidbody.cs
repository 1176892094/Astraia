using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class Rigidbody : Module<Player>
    {
        private static readonly Enumerable<RaycastHit2D> Hits = new Enumerable<RaycastHit2D>(8);

        public const float FIX = 200;

        public Collider2D Collider;
        public Vector2Int Position;
        public Vector2Int Velocity;
        private Bounds bounds => new Bounds(position + Collider.offset, Collider.bounds.size);
        private Vector2 position => new Vector3(positionX, positionY) / FIX;
        private Vector2 topLeft => new Vector2(minX, maxY);
        private Vector2 topRight => new Vector2(maxX, maxY);
        private Vector2 botLeft => new Vector2(minX, minY);
        private Vector2 botRight => new Vector2(maxX, minY);

        private float minX => bounds.min.x - 0.01F;
        private float minY => bounds.min.y - 0.01F;
        private float maxX => bounds.max.x + 0.01F;
        private float maxY => bounds.max.y + 0.01F;

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

        public override void Dequeue()
        {
            MovePosition(owner.transform.position);
        }

        public void MovePosition(float pixelate = 1 / 16F)
        {
            var worldPos = position;
            worldPos.x = Mathf.Round(worldPos.x / pixelate) * pixelate;
            worldPos.y = Mathf.Round(worldPos.y / pixelate) * pixelate;
            owner.transform.position = worldPos;
        }

        public void MovePosition(Vector2 worldPos, float pixelate = 1 / 16F)
        {
            positionX = Mathf.RoundToInt(worldPos.x * FIX);
            positionY = Mathf.RoundToInt(worldPos.y * FIX);
            MovePosition(pixelate);
        }

        public Enumerable<RaycastHit2D> Boxcast(Vector2 direction, float distance, ContactFilter2D layerMask)
        {
            Hits.Count = Physics2D.BoxCast(position, bounds.size, 0, direction, layerMask, Hits, distance / FIX);
            return Hits;
        }

        public Enumerable<RaycastHit2D> Raycast(Vector2 direction, float distance, ContactFilter2D layerMask)
        {
            Hits.Count = Physics2D.Raycast(position, direction, layerMask, Hits, distance / FIX);
            return Hits;
        }

        public Enumerable<RaycastHit2D> Raycast(Vector2 origin, Vector2 direction, float distance, ContactFilter2D layerMask)
        {
            Hits.Count = Physics2D.Raycast(origin, direction, layerMask, Hits, distance / FIX);
            return Hits;
        }

        public bool OverlapX(int velocityX, out int result)
        {
            var moveX = Math.Sign(velocityX);
            if (moveX != 0)
            {
                var dr = new Vector2(velocityX, 0);
                var p1 = moveX > 0 ? botRight : botLeft;
                var p2 = moveX > 0 ? topRight : topLeft;
                var r1 = Raycast(p1, dr.normalized, dr.magnitude, LayerConst.Ground).Count > 0;
                var r2 = Raycast(p2, dr.normalized, dr.magnitude, LayerConst.Ground).Count > 0;
                if (r1 && !r2)
                {
                    if (TrySubdivide(p1, p2, dr, out var offset))
                    {
                        result = Mathf.RoundToInt(offset * FIX);
                        return true;
                    }
                }
            }

            result = 0;
            return false;
        }

        public bool OverlapY(int velocityY, out int result)
        {
            var moveY = Math.Sign(velocityY);
            if (moveY != 0)
            {
                var dr = new Vector2(0, velocityY);
                var p1 = moveY > 0 ? topLeft : botLeft;
                var p2 = moveY > 0 ? topRight : botRight;
                var r1 = Raycast(p1, dr.normalized, dr.magnitude, LayerConst.Ground).Count > 0;
                var r2 = Raycast(p2, dr.normalized, dr.magnitude, LayerConst.Ground).Count > 0;
                if (r1 && !r2)
                {
                    if (TrySubdivide(p1, p2, dr, out var offset))
                    {
                        result = Mathf.RoundToInt(offset * FIX);
                        return true;
                    }
                }

                if (r2 && !r1)
                {
                    if (TrySubdivide(p2, p1, dr, out var offset))
                    {
                        result = Mathf.RoundToInt(-offset * FIX);
                        return true;
                    }
                }
            }

            result = 0;
            return false;
        }

        private bool TrySubdivide(Vector2 p1, Vector2 p2, Vector2 input, out float offset)
        {
            const int loop = 4;
            offset = 0;
            for (var i = loop - 1; i >= 0; i--)
            {
                var t = (float)i / loop;
                var samplePoint = Vector2.Lerp(p1, p2, t);
                if (Raycast(samplePoint, input.normalized, input.magnitude, LayerConst.Ground).Count > 0)
                {
                    var nextT = (float)(i + 1) / loop;
                    offset = (t + nextT) * 0.5f;
                    return true;
                }
            }

            return false;
        }
    }
}