// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:03
// // # Recently: 2025-04-20 19:04:03
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using Astraia;
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class Player : NetworkModule, IEvent<OnPlayerUpdate>, IStartAuthority
    {
        [Export] public PlayerAction Action;
        [Export] public PlayerModule Module;
        [Export] public PlayerFeature Feature;
        [Export] public PlayerMachine Machine;

        [SyncVar(nameof(OnValueChanged))]
        public Color32 color;

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

                    transform.localScale = new Vector3(value, 1, 1);
                }
            }
        }

        public void Execute(OnPlayerUpdate message)
        {
            Module.Tick();

            if (isOwner)
            {
                Action?.Tick();
                Machine.Tick();
            }
            else
            {
                Machine.SyncTransform();
            }
        }

        public void OnStartAuthority()
        {
            Action = transform.GetOrAddComponent<PlayerAction>();
            Machine.MovePosition(transform.position);
            Machine.Create<PlayerIdle>(Animations.Idle);
            Machine.Create<PlayerWait>(Animations.Wait);
            Machine.Create<PlayerWalk>(Animations.Walk);
            Machine.Create<PlayerJump>(Animations.Jump);
            Machine.Create<PlayerFall>(Animations.Fall);
            Machine.Create<PlayerGrab>(Animations.Grab);
            Machine.Create<PlayerHold>(Animations.Hold);
            Machine.Create<PlayerDash>(Animations.Dash);
            Machine.Create<PlayerRush>(Animations.Rush);
            Machine.Create<PlayerShuttle>(Animations.Shuttle);
            Machine.Switch(Animations.Idle);
            SendReadyServerRpc();
        }

        private void OnValueChanged(Color32 oldValue, Color32 newValue)
        {
            GetComponentInChildren<SpriteRenderer>().color = newValue;
        }

        [ServerRpc]
        public void SetPositionServerRpc(Position position)
        {
            SyncManager.Instance.SetPosition(this, position);
        }

        [ServerRpc]
        public void SyncColorServerRpc(Color32 color)
        {
            this.color = color;
        }

        [ServerRpc]
        public void SetDirectionServerRpc(int direction)
        {
            SetDirectionClientRpc(direction);
        }

        [ClientRpc(Pass.KCP | Pass.ANY)]
        private void SetDirectionClientRpc(int direction)
        {
            transform.localScale = new Vector3(direction, 1, 1);
        }

        [ServerRpc]
        public void LoadEffectServerRpc(Position position)
        {
            LoadEffectClientRpc(position);
        }

        [ClientRpc]
        public async void LoadEffectClientRpc(Position position)
        {
            var sprite = PoolManager.Show<SpriteRenderer>("Prefabs/10002", position.ToVector2());
            sprite.color = new Color(0, 0, 0, 1);
            await sprite.DOFade(0, 0.5f);
            PoolManager.Hide(sprite);
        }

        [ServerRpc]
        public void LoadEffectServerRpc(Position position, Position velocity)
        {
            LoadEffectClientRpc(position, velocity);
        }

        [ClientRpc]
        public void LoadEffectClientRpc(Position position, Position velocity)
        {
            var sprite = PoolManager.Show<SpriteRenderer>("Prefabs/10003", position.ToVector2());
            sprite.color = new Color(1, 1, 1, 1);
            sprite.transform.localScale = Vector3.one * 0.5F;
            var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            sprite.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            sprite.DOFade(0, 0.5F);
            sprite.DOScale(Vector3.one, 0.5F).OnComplete(() => PoolManager.Hide(sprite));
        }

        [ServerRpc]
        public void SendReadyServerRpc()
        {
            SyncManager.Instance.playerReady++;
        }
    }
}