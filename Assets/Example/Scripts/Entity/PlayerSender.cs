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

        [ServerRpc]
        public void SendInputServerRpc(PlayerInput input)
        {
            owner.Machine.velocityX = input.moveX;
            owner.Feature.lastFrame = owner.Feature.nextFrame;
            TargetUpdatePosition(connection, transform.position, owner.Feature.lastFrame);
        }

        [TargetRpc]
        void TargetUpdatePosition(NetworkClient target, Vector3 position, int processedSeq)
        {
            if (!isOwner) return;

            // // 服务器权威位置覆盖
            // serverPosition = position;
            // transform.position = serverPosition;
            //
            // // 移除已确认输入
            // while (pendingInputs.Count > 0 && pendingInputs.Peek().seq <= processedSeq)
            // {
            //     pendingInputs.Dequeue();
            // }
            //
            // // 重放未确认输入，修正预测
            // foreach (var input in pendingInputs)
            // {
            //     ApplyInput(input);
            // }
        }

        public void OnStartAuthority()
        {
            owner.Machine.AddState(StateConst.Hop, typeof(PlayerHop));
            owner.Machine.AddState(StateConst.Idle, typeof(PlayerIdle));
            owner.Machine.AddState(StateConst.Walk, typeof(PlayerWalk));
            owner.Machine.AddState(StateConst.Jump, typeof(PlayerJump));
            owner.Machine.AddState(StateConst.Grab, typeof(PlayerGrab));
            owner.Machine.AddState(StateConst.Dash, typeof(PlayerDash));
            owner.Machine.AddState(StateConst.Crash, typeof(PlayerCrash));
            owner.Machine.ChangeState(StateConst.Idle);
            GameManager.Instance.SetCamera(owner, new Vector3(0, 3, 0), new Vector2(30, 8));
        }
    }
}