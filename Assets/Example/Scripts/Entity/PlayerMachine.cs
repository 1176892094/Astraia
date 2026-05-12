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
using UnityEngine;

namespace Runtime
{
    public class PlayerMachine : StateMachine<int, Player>
    {
   

        public Vector3 recorder;
        public Collider2D collider;
        public SpriteRenderer renderer;
        

        public override void Dequeue()
        {
            collider = owner.GetComponent<Collider2D>();
            renderer = owner.GetComponent<SpriteRenderer>();
        }
    }
}