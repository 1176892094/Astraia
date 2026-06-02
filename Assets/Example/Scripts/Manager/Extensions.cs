using Astraia;
using Astraia.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime
{
    public static class Extensions
    {
        private static readonly Enumerable<RaycastHit2D> Hits = new Enumerable<RaycastHit2D>(8);

        public static Enumerable<RaycastHit2D> BoxCast(this Collider2D collider, Vector2 direction, float distance)
        {
            Hits.Count = collider.Cast(direction, LayerConst.Ground, Hits, distance);
            return Hits;
        }

        public static Enumerable<RaycastHit2D> Raycast(this Collider2D collider, Vector2 direction, float distance)
        {
            Debug.DrawLine(collider.bounds.center, collider.bounds.center + (Vector3)(direction.normalized * distance), Color.blue);
            Hits.Count = collider.Raycast(direction, LayerConst.Ground, Hits, distance);
            return Hits;
        }

        public static int DashCast(this Collider2D collider)
        {
            var bounds = collider.bounds;
            var origin1 = new Vector2(bounds.min.x, bounds.max.y);
            var origin2 = new Vector2(bounds.max.x, bounds.max.y);
            var state = 0;
            if (Physics2D.OverlapPoint(origin1 + Vector2.up * 0.5F))
            {
                state |= 1 << 0;
            }

            if (Physics2D.OverlapPoint(origin2 + Vector2.up * 0.5F))
            {
                state |= 1 << 1;
            }

            return state;
        }

        public static int HoldCast(this Collider2D collider, int direction)
        {
            var bounds = collider.bounds;
            Vector2 origin1;
            Vector2 origin2;
            if (direction > 0)
            {
                origin1 = new Vector2(bounds.max.x, bounds.min.y + 0.02F);
                origin2 = new Vector2(bounds.max.x, bounds.max.y - 0.02F);
            }
            else
            {
                origin1 = new Vector2(bounds.min.x, bounds.min.y + 0.02F);
                origin2 = new Vector2(bounds.min.x, bounds.max.y - 0.02F);
            }

            var state = 0;
            if (Physics2D.OverlapPoint(origin1 + Vector2.right * 0.5F * direction))
            {
                state |= 1 << 0;
            }

            if (Physics2D.OverlapPoint(origin2 + Vector2.right * 0.5F * direction))
            {
                state |= 1 << 1;
            }

            return state;
        }

        public static Tween DOFade(this SpriteRenderer component, float endValue, float duration)
        {
            var color = component.color;
            return component.Play(duration).OnUpdate(progress =>
            {
                var colorA = Mathf.Lerp(color.a, endValue, progress);
                component.color = new Color(color.r, color.g, color.b, colorA);
            });
        }

        public static Tween DOFade(this Graphic component, float endValue, float duration)
        {
            var color = component.color;
            return component.Play(duration).OnUpdate(progress =>
            {
                var colorA = Mathf.Lerp(color.a, endValue, progress);
                component.color = new Color(color.r, color.g, color.b, colorA);
            });
        }
    }
}