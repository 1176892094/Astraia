using UnityEngine;

namespace Runtime
{
    public class DashQuad : MonoBehaviour, IOnExit
    {
        public void OnExit(Collider2D other)
        {
            if (other.TryGetComponent(out Player player) && player.isOwner)
            {
                gameObject.tag = "Collision";
                player.Feature.State &= ~State.穿梭;
            }
        }
    }
}