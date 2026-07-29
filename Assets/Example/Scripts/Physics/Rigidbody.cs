using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class Rigidbody : Module<Player>
    {
        public readonly struct Collision
        {
            private const float EXPAND = 0.01F;

            private static readonly Enumerable<RaycastHit2D> Hits = new(16);
            private readonly Vector2 extent;
            private readonly Vector2 center;
            private readonly Vector2 screen;

            private float minX => center.x - extent.x - EXPAND;
            private float minY => center.y - extent.y - EXPAND;
            private float maxX => center.x + extent.x + EXPAND;
            private float maxY => center.y + extent.y + EXPAND;
            private Vector2 LT => new(minX, maxY); // 左上
            private Vector2 LB => new(minX, minY); // 左下
            private Vector2 RT => new(maxX, maxY); // 右上
            private Vector2 RB => new(maxX, minY); // 右下

            public Collision(Vector2 position, Vector2 extents, Vector2 offset)
            {
                center = offset + position;
                extent = extents;
                screen = extents * 2;
            }

            public Enumerable<RaycastHit2D> Boxcast(float distance, ContactFilter2D filter) // 平台检测
            {
                var origin = new Rect(center.x, minY, screen.x, EXPAND);
                Hits.Count = Physics2D.BoxCast(origin.position, origin.size, 0, Vector2.down, filter, Hits, distance);
                return Hits;
            }

            public Enumerable<RaycastHit2D> Boxcast(Vector2 direction, float distance, ContactFilter2D filter) // 移动检测
            {
                var origin = center + direction * EXPAND;
                Hits.Count = Physics2D.BoxCast(origin, screen, 0, direction, filter, Hits, distance);
                return Hits;
            }

            private static Enumerable<RaycastHit2D> Raycast(Vector2 origin, Vector2 direction, ContactFilter2D filter, float distance) // 射线检测
            {
                Hits.Count = Physics2D.Raycast(origin, direction, filter, Hits, distance);
                return Hits;
            }

            public bool RaycastX(float velocityX, ContactFilter2D filter) // 上下检测
            {
                var moveX = Math.Sign(velocityX);
                if (moveX != 0)
                {
                    var dr = new Vector2(velocityX, 0);
                    var p1 = moveX > 0 ? RB : LB;
                    var p2 = moveX > 0 ? RT : LT;
                    var r1 = Raycast(p1, dr.normalized, filter, dr.magnitude).Count > 0;
                    var r2 = Raycast(p2, dr.normalized, filter, dr.magnitude).Count > 0;
                    if (r1 && !r2)
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool RaycastX(float velocityX, ContactFilter2D filter, out float result) // 上下检测 细分射线
            {
                var moveX = Math.Sign(velocityX);
                if (moveX != 0)
                {
                    var dr = new Vector2(velocityX, 0);
                    var p1 = moveX > 0 ? RB : LB;
                    var p2 = moveX > 0 ? RT : LT;
                    var r1 = Raycast(p1, dr.normalized, filter, dr.magnitude).Count > 0;
                    var r2 = Raycast(p2, dr.normalized, filter, dr.magnitude).Count > 0;
                    if (r1 && !r2)
                    {
                        if (Subdivide(p1, p2, dr, filter, out var offset))
                        {
                            result = offset;
                            return true;
                        }
                    }

                    if (r2 && !r1)
                    {
                        if (Subdivide(p2, p1, dr, filter, out var offset))
                        {
                            result = -offset;
                            return true;
                        }
                    }
                }

                result = 0;
                return false;
            }

            public bool RaycastY(float velocityY, ContactFilter2D filter, out float result) //左右检测 细分射线
            {
                var moveY = Math.Sign(velocityY);
                if (moveY != 0)
                {
                    var dr = new Vector2(0, velocityY);
                    var p1 = moveY > 0 ? LT : LB;
                    var p2 = moveY > 0 ? RT : RB;
                    var r1 = Raycast(p1, dr.normalized, filter, dr.magnitude).Count > 0;
                    var r2 = Raycast(p2, dr.normalized, filter, dr.magnitude).Count > 0;
                    if (r1 && !r2)
                    {
                        if (Subdivide(p1, p2, dr, filter, out var offset))
                        {
                            result = offset;
                            return true;
                        }
                    }

                    if (r2 && !r1)
                    {
                        if (Subdivide(p2, p1, dr, filter, out var offset))
                        {
                            result = -offset;
                            return true;
                        }
                    }
                }
                

                result = 0;
                return false;
            }

            private static bool Subdivide(Vector2 p1, Vector2 p2, Vector2 dr, ContactFilter2D filter, out float offset, int count = 4)
            {
                offset = 0;
                for (var i = count - 1; i >= 0; i--)
                {
                    var step = (float)i / count;
                    var lerp = Vector2.Lerp(p1, p2, step);
                    if (Raycast(lerp, dr.normalized, filter, dr.magnitude).Count > 0)
                    {
                        var next = (float)(i + 1) / count;
                        offset = (step + next) * 0.5f;
                        return true;
                    }
                }

                return false;
            }
        }

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

        protected override void Dequeue()
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