using UnityEngine;

namespace Runtime
{
    public class DashQuad : MonoBehaviour, IOnExit
    {
        public void OnExit(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                if (gameObject.CompareTag(TagConst.Untagged))
                {
                    gameObject.tag = TagConst.DashQuad;
                    player.Feature.State &= ~State.穿梭;
                }
            }
        }
    }
}