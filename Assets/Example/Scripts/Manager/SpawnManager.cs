// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-21 14:04:48
// // # Recently: 2025-04-21 14:04:48
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia.Common;
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    public class SpawnManager : NetworkModule
    {
        public static SpawnManager Instance;

        public override void Dequeue()
        {
            Instance = this;
            Object.DontDestroyOnLoad(gameObject);
        }

        [ClientRpc]
        public async void LoadEffectClientRpc(Vector3 position)
        {
            var sprite = PoolManager.Show("Prefabs/Effect", position).GetComponent<SpriteRenderer>();
            sprite.color = new Color(0, 0, 0, 1);
            await sprite.DOFade(0, 0.5f);
            PoolManager.Hide(sprite.gameObject);
        }
    }
}