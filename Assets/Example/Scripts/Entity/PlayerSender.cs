using System;
using Astraia.Core;
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
            get => Math.Sign(transform.localScale.x);
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

                    transform.localScale = new Vector3(value, 1, 1);
                }
            }
        }

        private void OnValueChanged(Color32 oldValue, Color32 newValue)
        {
            GetComponentInChildren<SpriteRenderer>().color = newValue;
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

        [ClientRpc(Channel.Reliable | Channel.IgnoreOwner)]
        private void SetDirectionClientRpc(int direction)
        {
            transform.localScale = new Vector3(direction, 1, 1);
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
            GameManager.Instance.SetPlayer(transform);
            GameManager.Instance.SetBounds(new Bounds(Vector3.zero, new Vector3(13, 6)));
            owner.Action = gameObject.AddComponent<PlayerAction>();
            var machine = GetComponent<PlayerMachine>();
            machine.Create<PlayerIdle>(Animations.Idle);
            machine.Create<PlayerWalk>(Animations.Walk);
            machine.Create<PlayerJump>(Animations.Jump);
            machine.Create<PlayerFall>(Animations.Fall);
            machine.Create<PlayerGrab>(Animations.Grab);
            machine.Create<PlayerDash>(Animations.Dash);
            machine.Create<PlayerCrash>(Animations.Crash);
            machine.Switch(Animations.Idle);
        }
    }
}