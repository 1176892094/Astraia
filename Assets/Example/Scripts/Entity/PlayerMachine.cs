// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:11
// // # Recently: 2025-04-20 19:04:11
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia;
using Astraia.Core;
using UnityEngine;

namespace Runtime
{
    public class PlayerMachine : StateMachine<Player>
    {
        public Rigidbody2D rigidbody;
        public SpriteRenderer renderer;

        public float velocityX
        {
#if UNITY_6000_0_OR_NEWER
            get => rigidbody.linearVelocityX;
            set => rigidbody.linearVelocityX = value;
#else
            get => rigidbody.velocityX;
            set => rigidbody.velocityX = value;
#endif
        }

        public float velocityY
        {
#if UNITY_6000_0_OR_NEWER
            get => rigidbody.linearVelocityY;
            set => rigidbody.linearVelocityY = value;
#else
            get => rigidbody.velocityY;
            set => rigidbody.velocityY = value;
#endif
        }

        public Vector2 velocity
        {
#if UNITY_6000_0_OR_NEWER
            get => rigidbody.linearVelocity;
            set => rigidbody.linearVelocity = value;
#else
            get => rigidbody.velocity;
            set => rigidbody.velocity = value;
#endif
        }

        public override void Dequeue()
        {
            rigidbody = owner.GetComponent<Rigidbody2D>();
            renderer = owner.GetComponent<SpriteRenderer>();
        }
    }
}