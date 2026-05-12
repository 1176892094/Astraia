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
using Astraia.Core;
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    public class Player : NetworkEntity
    {
        public StateType State = StateType.默认;
        private PlayerInput Input => Logic.GetComponent<PlayerInput>();
        public PlayerSender Sender => Logic.GetComponent<PlayerSender>();
        public PlayerMachine Machine => Logic.GetComponent<PlayerMachine>();
        public PlayerFeature Feature => Logic.GetComponent<PlayerFeature>();
        private NetworkTransform Transform => Logic.GetComponent<NetworkTransform>();

        protected override void Awake()
        {
            base.Awake();
            Logic.AddComponent<PlayerInput>();
            Logic.AddComponent<PlayerSender>();
            Logic.AddComponent<PlayerFeature>();
            Logic.AddComponent<PlayerMachine>();
            Logic.AddComponent<NetworkTransform>();
            Transform.syncDirection = SyncMode.Client;
        }

        private void Update()
        {
            if (isOwner)
            {
                Input.Update();
            }

            Transform.Update();
        }

        private void FixedUpdate()
        {
            if (isOwner)
            {
                Machine.Update();
                connection.Send(new InputMessage(objectId));
            }
        }

        private void LateUpdate()
        {
            Transform.LateUpdate();
        }
    }

    [Serializable]
    public class PlayerSender : NetworkModule, IStartAuthority
    {
        [SyncVar(nameof(OnValueChanged))] public Color32 color;

        public int Direction
        {
            get
            {
                if (transform.localScale.x > 0)
                {
                    return 1;
                }

                if (transform.localScale.x < 0)
                {
                    return -1;
                }

                return 0;
            }
            set
            {
                if (value > 0 && Direction < 0)
                {
                    if (isOwner)
                    {
                        SetDirectionServerRpc(1);
                    }
                    else if (isServer)
                    {
                        SetDirectionClientRpc(1);
                    }

                    transform.localScale = new Vector3(1, 1, 1);
                    return;
                }

                if (value < 0 && Direction > 0)
                {
                    if (isOwner)
                    {
                        SetDirectionServerRpc(-1);
                    }
                    else if (isServer)
                    {
                        SetDirectionClientRpc(-1);
                    }

                    transform.localScale = new Vector3(-1, 1, 1);
                }
            }
        }

        private PlayerMachine Machine => owner.Logic.GetComponent<PlayerMachine>();

        private void OnValueChanged(Color32 oldValue, Color32 newValue)
        {
            Machine.renderer.color = newValue;
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
            Machine.Create<PlayerIdle>(Animations.Idle);
            Machine.Create<PlayerWalk>(Animations.Walk);
            Machine.Create<PlayerJump>(Animations.Jump);
            Machine.Create<PlayerFall>(Animations.Fall);
            Machine.Create<PlayerGrab>(Animations.Grab);
            Machine.Create<PlayerDash>(Animations.Dash);
            Machine.Create<PlayerCrash>(Animations.Crash);
            Machine.Switch(Animations.Idle);
        }
    }
}