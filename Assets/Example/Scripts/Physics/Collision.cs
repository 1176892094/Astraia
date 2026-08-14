using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
    public partial class Rigidbody
    {
        public readonly struct Collision
        {
            private static readonly List<RaycastHit2D> Hits = new List<RaycastHit2D>(16);

            public readonly Bounds bounds;
            private Vector2 size => bounds.size;
            private Vector2 center => bounds.center;
            private Vector2 extents => bounds.extents;
            private float minX => center.x - extents.x - 0.01F;
            private float minY => center.y - extents.y - 0.01F;
            private float maxX => center.x + extents.x + 0.01F;
            private float maxY => center.y + extents.y + 0.01F;
            private Vector2 LT => new(minX, maxY); // 左上
            private Vector2 LB => new(minX, minY); // 左下
            private Vector2 RT => new(maxX, maxY); // 右上
            private Vector2 RB => new(maxX, minY); // 右下

            public Collision(Vector2 center, Vector2 extent, Vector2 offset)
            {
                bounds = new Bounds(center + offset, extent * 2);
            }

            public List<RaycastHit2D> Raycast(Vector2 direction, float distance, ContactFilter2D filter) // 冲刺检测
            {
                Hits.Clear();
                Physics2D.Raycast(center, direction, filter, Hits, distance);
                return Hits;
            }

            public List<RaycastHit2D> Boxcast(Vector2 direction, float distance, ContactFilter2D filter) // 冲刺检测
            {
                Hits.Clear();
                Physics2D.BoxCast(center, size, 0, direction, filter, Hits, distance);
                return Hits;
            }

            public List<RaycastHit2D> Boxcast(float distance, ContactFilter2D filter) // 平台检测
            {
                var origin = new Rect(center.x, center.y - extents.y, size.x, 0.01F);
                Hits.Clear();
                Physics2D.BoxCast(origin.position, origin.size, 0, Vector2.down, filter, Hits, distance);
                return Hits;
            }

            public List<RaycastHit2D> BoxcastX(int moveX, float distance, ContactFilter2D filter) // 碰撞检测X
            {
                var direction = new Vector2(moveX, 0);
                var position = center + direction * 0.01F;
                Hits.Clear();
                Physics2D.BoxCast(position, size, 0, direction, filter, Hits, distance);
                return Hits;
            }

            public List<RaycastHit2D> BoxcastY(int moveY, float distance, ContactFilter2D filter) // 碰撞检测Y
            {
                var direction = new Vector2(0, moveY);
                var position = center + direction * 0.01F;
                Hits.Clear();
                Physics2D.BoxCast(position, size, 0, direction, filter, Hits, distance);
                return Hits;
            }

            public bool RaycastX(float velocityX, ContactFilter2D filter)
            {
                var moveX = Math.Sign(velocityX);
                if (moveX != 0)
                {
                    var dr = new Vector2(velocityX, 0);
                    var p1 = moveX > 0 ? RB : LB;
                    var p2 = moveX > 0 ? RT : LT;
                    var r1 = Raycast(p1, dr.normalized, filter, dr.magnitude).Count > 0;
                    var r2 = Raycast(p2, dr.normalized, filter, dr.magnitude).Count > 0;
                    if (r1 && !r2) // 悬挂墙壁 底部碰撞 顶部没撞
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool RaycastX(float velocityX, ContactFilter2D filter, out float result)
            {
                var moveX = Math.Sign(velocityX);
                if (moveX != 0)
                {
                    var dr = new Vector2(velocityX, 0);
                    var p1 = moveX > 0 ? RB : LB;
                    var p2 = moveX > 0 ? RT : LT;
                    var r1 = Raycast(p1, dr.normalized, filter, dr.magnitude).Count > 0;
                    var r2 = Raycast(p2, dr.normalized, filter, dr.magnitude).Count > 0;
                    if (r1 && !r2) // 横向冲刺 底部碰撞 顶部没撞 向上偏移
                    {
                        if (Subdivide(p1, p2, dr, filter, out var offset))
                        {
                            result = offset;
                            return true;
                        }
                    }

                    if (r2 && !r1) // 横向冲刺 顶部碰撞 底部没装 向下偏移
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

            public bool RaycastY(float velocityY, ContactFilter2D filter, out float result)
            {
                var moveY = Math.Sign(velocityY);
                if (moveY != 0)
                {
                    var dr = new Vector2(0, velocityY);
                    var p1 = moveY > 0 ? LT : LB;
                    var p2 = moveY > 0 ? RT : RB;
                    var r1 = Raycast(p1, dr.normalized, filter, dr.magnitude).Count > 0;
                    var r2 = Raycast(p2, dr.normalized, filter, dr.magnitude).Count > 0;
                    if (r1 && !r2) // 竖向冲刺 左边碰撞 右边没撞 向右偏移
                    {
                        if (Subdivide(p1, p2, dr, filter, out var offset))
                        {
                            result = offset;
                            return true;
                        }
                    }

                    if (r2 && !r1) // 竖向冲刺 右边碰撞 左边没撞 向左偏移
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

            private static bool Subdivide(Vector2 p1, Vector2 p2, Vector2 dr, ContactFilter2D filter, out float offset)
            {
                offset = 0;

                float low = 0;
                float high = 1;

                for (var i = 0; i < 5; i++)
                {
                    var mid = (low + high) * 0.5f;
                    var point = Vector2.Lerp(p1, p2, mid);
                    var hit = Raycast(point, dr.normalized, filter, dr.magnitude).Count > 0;
                    if (hit)
                    {
                        low = mid;
                    }
                    else
                    {
                        high = mid;
                    }
                }

                var t = (low + high) * 0.5f;
                offset = Vector2.Distance(p1, Vector2.Lerp(p1, p2, t));
                return true;
            }

            private static List<RaycastHit2D> Raycast(Vector2 origin, Vector2 direction, ContactFilter2D filter, float distance)
            {
                Hits.Clear();
                Physics2D.Raycast(origin, direction, filter, Hits, distance);
                return Hits;
            }
        }
    }
}