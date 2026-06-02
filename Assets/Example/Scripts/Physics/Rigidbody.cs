using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Runtime
{
    [Serializable]
    public class Rigidbody : MonoBehaviour
    {
        public const int FIX = 200;

        public Vector2 size = Vector2.one;
        public Vector2Int offset;
        public Vector2Int position;
        public Vector2Int velocity;
        public List<Collision> collisions = new List<Collision>();

        public int PositionX
        {
            get => position.x;
            set => position.x = value;
        }

        public int PositionY
        {
            get => position.y;
            set => position.y = value;
        }

        public int VelocityX
        {
            get => velocity.x;
            set => velocity.x = value;
        }

        public int VelocityY
        {
            get => velocity.y;
            set => velocity.y = value;
        }

        public Collision Bounds
        {
            get
            {
                var sizeX = Mathf.RoundToInt(size.x * FIX);
                var sizeY = Mathf.RoundToInt(size.y * FIX);
                return new Collision(position + offset, new Vector2Int(sizeX, sizeY));
            }
        }

        protected virtual void Awake()
        {
            MovePosition(transform.position);
        }

        protected virtual void OnDestroy()
        {
            collisions.Clear();
        }

        public void OnDrawGizmosSelected()
        {
            Vector2 pos = transform.position * FIX;
            Gizmos.DrawWireCube((pos + offset) / FIX, size);
        }

        public bool Contains(Vector2 point)
        {
            return Bounds.Contains(point);
        }

        public bool Overlap(Collision other)
        {
            return Bounds.Overlap(other);
        }

        public bool Overlap(Vector2 center, Vector2 size)
        {
            return Bounds.Overlap(center, size);
        }

        public int BoxCast(Collision target, Vector2 direction, int distance)
        {
            return Bounds.BoxCast(target, direction, distance);
        }

        public void MovePosition()
        {
            transform.position = new Vector2(PositionX, PositionY) / FIX;
        }

        public void MovePosition(Vector2 position)
        {
            PositionX = Mathf.RoundToInt(position.x * FIX);
            PositionY = Mathf.RoundToInt(position.y * FIX);
            transform.position = new Vector2(PositionX, PositionY) / FIX;
        }

        public List<Collision> GetContacts(Tilemap map,int velocityX,int velocityY)
        {
            const float factor = FIX;

            var minX = Mathf.FloorToInt((Bounds.MinX - velocityX) / factor);
            var minY = Mathf.FloorToInt((Bounds.MinY - velocityY) / factor);
            var maxX = Mathf.CeilToInt((Bounds.MaxX + velocityX) / factor);
            var maxY = Mathf.CeilToInt((Bounds.MaxY + velocityY) / factor);

            collisions.Clear();
            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    var point = new Vector3Int(x, y, 0);
                    if (map.HasTile(point))
                    {
                        var center = map.GetCellCenterWorld(point);
                        var centerX = Mathf.RoundToInt(center.x * factor);
                        var centerY = Mathf.RoundToInt(center.y * factor);
                        collisions.Add(new Collision(new Vector2Int(centerX, centerY), Vector2Int.one * FIX));
                    }
                }
            }

            return collisions;
        }

        public static implicit operator Collision(Rigidbody rigidbody)
        {
            return rigidbody.Bounds;
        }

        [Serializable]
        public struct Collision
        {
            public int CenterX;
            public int CenterY;
            public int ExtentX;
            public int ExtentY;
            public int MinX => CenterX - ExtentX;
            public int MinY => CenterY - ExtentY;
            public int MaxX => CenterX + ExtentX;
            public int MaxY => CenterY + ExtentY;
            public Vector2Int Center => new Vector2Int(CenterX, CenterY);
            public Vector2Int Extent => new Vector2Int(ExtentX, ExtentY);
            public Vector2Int TopLeft => new Vector2Int(MinX, MaxY);
            public Vector2Int TopRight => new Vector2Int(MaxX, MaxY);
            public Vector2Int BottomLeft => new Vector2Int(MinX, MinY);
            public Vector2Int BottomRight => new Vector2Int(MaxX, MinY);

            public Collision(Vector2Int center, Vector2Int size)
            {
                CenterX = center.x;
                CenterY = center.y;
                ExtentX = size.x / 2;
                ExtentY = size.y / 2;
            }

            public bool Contains(Vector2 point)
            {
                return point.x >= MinX && point.x <= MaxX && point.y >= MinY && point.y <= MaxY;
            }

            public bool Overlap(Collision other)
            {
                return MinX <= other.MaxX && MaxX >= other.MinX && MinY <= other.MaxY && MaxY >= other.MinY;
            }

            public bool Overlap(Vector2 center, Vector2 size)
            {
                var extentX = size.x / 2;
                var extentY = size.y / 2;

                var minX = center.x - extentX;
                var minY = center.y - extentY;
                var maxX = center.x + extentX;
                var maxY = center.y + extentY;

                return MinX <= maxX && MaxX >= minX && MinY <= maxY && MaxY >= minY;
            }

            public int BoxCast(Collision target, Vector2 direction, int distance)
            {
                var signX = Math.Sign(direction.x);
                var signY = Math.Sign(direction.y);
                var stepX = BoxCastX(target, signX);
                var stepY = BoxCastY(target, signY);

                if (stepX != -1 && stepY != -1)
                {
                    int steps = Math.Max(stepX, stepY);
                    if (steps <= distance)
                    {
                        var offset = new Vector2Int(signX, signY) * steps;
                        var origin = new Collision(Center + offset, Extent * 2);
                        if (origin.Overlap(target))
                        {
                            return steps;
                        }
                    }
                }

                return -1;
            }

            private int BoxCastX(Collision target, int signX)
            {
                switch (signX)
                {
                    case >= 0 when MaxX < target.MinX: // 向右，自身在目标左侧
                        return target.MinX - MaxX;
                    case <= 0 when MinX > target.MaxX: // 向左，自身在目标右侧
                        return MinX - target.MaxX;
                    case >= 0 when MinX >= target.MaxX: // 向右，自身已在目标右侧
                        return -1;
                    case <= 0 when MaxX <= target.MinX: // 向左，自身已在目标左侧
                        return -1;
                    default:
                        return 0;
                }
            }

            private int BoxCastY(Collision target, int signY)
            {
                switch (signY)
                {
                    case > 0 when MaxY < target.MinY: // 向上，自身在目标下方
                        return target.MinY - MaxY;
                    case < 0 when MinY > target.MaxY: // 向下，自身在目标上方
                        return MinY - target.MaxY;
                    case >= 0 when MinY >= target.MaxY: // 向上，自身已在目标上侧
                        return -1;
                    case <= 0 when MaxY <= target.MinY: // 向下，自身已在目标下侧
                        return -1;
                    default:
                        return 0;
                }
            }

            public static bool Raycast(Collision collision, Vector2 origin, Vector2 direction, float maxDistance)
            {
                direction = direction.normalized;

                var tMin = float.NegativeInfinity;
                var tMax = float.PositiveInfinity;

                if (Mathf.Approximately(direction.x, 0))
                {
                    if (origin.x < collision.MinX || origin.x > collision.MaxX)
                    {
                        return false;
                    }
                }
                else
                {
                    var tx1 = (collision.MinX - origin.x) / direction.x;
                    var tx2 = (collision.MaxX - origin.x) / direction.x;

                    tMin = Mathf.Max(tMin, Mathf.Min(tx1, tx2));
                    tMax = Mathf.Min(tMax, Mathf.Max(tx1, tx2));
                }

                if (Mathf.Approximately(direction.y, 0))
                {
                    if (origin.y < collision.MinY || origin.y > collision.MaxY)
                    {
                        return false;
                    }
                }
                else
                {
                    var ty1 = (collision.MinY - origin.y) / direction.y;
                    var ty2 = (collision.MaxY - origin.y) / direction.y;

                    tMin = Mathf.Max(tMin, Mathf.Min(ty1, ty2));
                    tMax = Mathf.Min(tMax, Mathf.Max(ty1, ty2));
                }

                if (tMax < 0)
                {
                    return false;
                }

                if (tMin > tMax)
                {
                    return false;
                }

                if (tMin < 0)
                {
                    tMin = 0;
                }

                return tMin <= maxDistance;
            }
        }
    }
}