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
    public class SpawnManager : NetworkAgent
    {
        public static SpawnManager Instance;

        public override void OnLoad()
        {
            Instance = this;
            Object.DontDestroyOnLoad(gameObject);
        }

        [ClientRpc]
        public void LoadEffectClientRpc(Vector3 position)
        {
            PoolManager.Show("Prefabs/Effect", obj =>
            {
                var sprite = obj.GetComponent<SpriteRenderer>();
                sprite.transform.position = position;
                sprite.color = new Color(0, 0, 0, 1);
                sprite.DOFade(0, 0.5f).OnComplete(() => PoolManager.Hide(obj));
            });
        }
    }
}