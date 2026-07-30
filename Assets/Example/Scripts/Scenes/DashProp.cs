using Astraia;
using UnityEngine;

namespace Runtime
{
    public class DashProp : Export, IOnEnter
    {
        [Export] private new BoxCollider2D collider;
        [Export] private new SpriteRenderer renderer;

        public void OnEnter(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                collider.enabled = false;
                renderer.enabled = false;
                player.Feature.DashCD = 0;
                player.Feature.DashCount = 1;
                renderer.Wait(1.5F).OnComplete(() =>
                {
                    collider.enabled = true;
                    renderer.enabled = true;
                });
            }
        }
    }
}