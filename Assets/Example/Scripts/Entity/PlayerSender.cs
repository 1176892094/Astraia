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


using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    public class PlayerSender : NetworkAgent
    {
        [SyncVar(nameof(OnColorValueChanged))] public Color syncColor;

        private void OnColorValueChanged(Color oldValue, Color newValue)
        {
            owner.GetAgent<PlayerMachine>().renderer.color = newValue;
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
    }
}