// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-16 13:07:58
// // # Recently: 2025-07-16 13:07:58
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************


using Astraia.Common;
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    public class PlayerSender : NetworkAgent, IStartAuthority
    {
        [SyncVar(nameof(OnColorValueChanged))] public Color syncColor;
        private new Player owner => (Player)base.owner;

        private void OnColorValueChanged(Color oldValue, Color newValue)
        {
            owner.Machine.renderer.color = newValue;
        }

        [ServerRpc]
        public void LoadEffectServerRpc(Vector3 position)
        {
            SpawnManager.Instance.LoadEffectClientRpc(position);
        }

        [ServerRpc]
        public void SyncColorServerRpc(Color syncColor)
        {
            this.syncColor = syncColor;
        }

        public void OnStartAuthority()
        {
            owner.Machine.AddState<PlayerIdle>(typeof(PlayerIdle));
            owner.Machine.AddState<PlayerWalk>(typeof(PlayerWalk));
            owner.Machine.AddState<PlayerJump>(typeof(PlayerJump));
            owner.Machine.AddState<PlayerGrab>(typeof(PlayerGrab));
            owner.Machine.AddState<PlayerDash>(typeof(PlayerDash));
            owner.Machine.AddState<PlayerHop>(typeof(PlayerHop));
            owner.Machine.AddState<PlayerCrash>(typeof(PlayerCrash));
            owner.Machine.ChangeState<PlayerIdle>();
            GameManager.Instance.SetCamera(owner, new Vector3(0, 3, 0), new Vector2(30, 8));
        }
    }
}