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

using Astraia.Net;

namespace Runtime
{
    public class Player : NetworkEntity
    {
        public State State = State.默认;
        public PlayerInput Input;
        public PlayerSender Sender;
        public PlayerMachine Machine;
        public PlayerFeature Feature;

        protected override void Awake()
        {
            base.Awake();
            Sender = gameObject.GetComponent<PlayerSender>();
            Feature = gameObject.AddComponent<PlayerFeature>();
            Machine = gameObject.AddComponent<PlayerMachine>();
        }

        private void Update()
        {
            if (isOwner)
            {
                Input.Tick();
            }
        }

        private void FixedUpdate()
        {
            if (isOwner)
            {
                Machine.Tick();
            }
        }
    }
}