using System;
using Astraia;
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class PlayerSender : NetworkModule, IStartAuthority
    {
        public new Player owner => (Player)base.owner;

        [SyncVar(nameof(OnValueChanged))] public Color32 color;

        public int Direction
        {
            get => Math.Sign(owner.transform.localScale.x);
            set
            {
                if ((value > 0 && Direction < 0) || (value < 0 && Direction > 0))
                {
                    if (isOwner)
                    {
                        SetDirectionServerRpc(value);
                    }
                    else if (isServer)
                    {
                        SetDirectionClientRpc(value);
                    }

                    owner.transform.localScale = new Vector3(value, 1, 1);
                }
            }
        }

        private void OnValueChanged(Color32 oldValue, Color32 newValue)
        {
            owner.GetComponentInChildren<SpriteRenderer>().color = newValue;
        }

        [ServerRpc]
        public void SyncColorServerRpc(Color32 color)
        {
            this.color = color;
        }

        [ServerRpc]
        private void SetDirectionServerRpc(int direction)
        {
            SetDirectionClientRpc(direction);
        }

        [ClientRpc(Pass.KCP | Pass.ANY)]
        private void SetDirectionClientRpc(int direction)
        {
            owner.transform.localScale = new Vector3(direction, 1, 1);
        }

        [ServerRpc]
        public void LoadEffectServerRpc(Vector3 position)
        {
            LoadEffectClientRpc(position);
        }

        [ClientRpc]
        public async void LoadEffectClientRpc(Vector3 position)
        {
            var sprite = PoolManager.Show<SpriteRenderer>("Prefabs/Shadow", position);
            sprite.color = new Color(0, 0, 0, 1);
            await sprite.DOFade(0, 0.5f);
            PoolManager.Hide(sprite);
        }

        public void OnStartAuthority()
        {
            GameManager.Instance.SetPlayer(owner.transform);
            GameManager.Instance.SetBounds(new Bounds(Vector3.zero, new Vector3(13, 6)));
            owner.AddComponent<PlayerAction>().Dequeue();
            owner.Machine.Create<PlayerIdle>(Animations.Idle);
            owner.Machine.Create<PlayerWalk>(Animations.Walk);
            owner.Machine.Create<PlayerJump>(Animations.Jump);
            owner.Machine.Create<PlayerFall>(Animations.Fall);
            owner.Machine.Create<PlayerGrab>(Animations.Grab);
            owner.Machine.Create<PlayerDash>(Animations.Dash);
            owner.Machine.Create<PlayerCrash>(Animations.Crash);
            owner.Machine.Switch(Animations.Idle);
        }
    }
}