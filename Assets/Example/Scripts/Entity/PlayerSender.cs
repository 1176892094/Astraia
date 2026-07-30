using System;
using Astraia;
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class PlayerSender : NetworkModule, IStartAuthority
    {
        [SyncVar(nameof(OnValueChanged))] public Color32 color;

        private void OnValueChanged(Color32 oldValue, Color32 newValue)
        {
            owner.GetComponentInChildren<SpriteRenderer>().color = newValue;
        }

        [ServerRpc]
        public void SetPositionServerRpc(Position position)
        {
            SyncManager.Instance.SetPosition(objectId, position);
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
            owner.transform.localScale = new Vector3(direction, 1, 1);
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

        public void OnStartAuthority()
        {
            var player = (Player)owner;
            CameraManager.Instance.SetPlayer(player.transform);
            CameraManager.Instance.SetBounds(default);
            player.AddComponent<PlayerAction>().Dequeue();
            player.Machine.InitPosition(player.transform.position);
            player.Machine.Create<PlayerIdle>(Animations.Idle);
            player.Machine.Create<PlayerWait>(Animations.Wait);
            player.Machine.Create<PlayerWalk>(Animations.Walk);
            player.Machine.Create<PlayerJump>(Animations.Jump);
            player.Machine.Create<PlayerFall>(Animations.Fall);
            player.Machine.Create<PlayerGrab>(Animations.Grab);
            player.Machine.Create<PlayerHold>(Animations.Hold);
            player.Machine.Create<PlayerDash>(Animations.Dash);
            player.Machine.Create<PlayerRush>(Animations.Rush);
            player.Machine.Create<PlayerShuttle>(Animations.Shuttle);
            player.Machine.Switch(Animations.Idle);
        }
    }
}