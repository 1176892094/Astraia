using Astraia;
using UnityEngine;

namespace Runtime
{
    public class DashProp : Export, IOnEnter
    {
        [Export] private new BoxCollider2D collider;
        [Export] private new SpriteRenderer renderer;

        public async void OnEnter(Collider2D other)
        {
            if (other.TryGetComponent(out Player player))
            {
                collider.enabled = false;
                renderer.enabled = false;
                if (player.isOwner)
                {
                    player.Feature.DashCD = 0;
                    player.Feature.DashCount = 1;
                }

                await renderer.Wait(1.5F);
                collider.enabled = true;
                renderer.enabled = true;
            }
        }
    }
}