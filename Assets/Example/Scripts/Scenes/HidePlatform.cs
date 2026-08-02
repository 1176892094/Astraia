using Astraia;
using UnityEngine;

namespace Runtime
{
    public class HidePlatform : Export, IOnEnter
    {
        [Export] private new BoxCollider2D collider;
        [Export] private new SpriteRenderer renderer;

        public void OnEnter(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                var a = player.Machine.collision.bounds;
                var b = collider.bounds;

                if (a.Intersects(b)) // 生成时玩家在里面 弹出玩家
                {
                    float y;
                    if (a.center.y > b.center.y)
                    {
                        y = b.max.y + a.size.y * 0.5f + 0.01F;
                        Execute();
                    }
                    else
                    {
                        y = b.min.y - a.size.y * 0.5f - 0.01F;
                    }

                    player.Machine.position = new Position(player.Machine.position.x, y);
                    player.Machine.MoveTransform(player.Machine.position);
                    return;
                }

                if (a.min.y > b.max.y)
                {
                    Execute();
                }
            }
        }

        private async void Execute()
        {
            if (collider.enabled)
            {
                await collider.Wait(0.5F);
                collider.enabled = false;
                renderer.enabled = false;
                await renderer.Wait(1.5F);
                collider.enabled = true;
                renderer.enabled = true;
            }
        }
    }
}