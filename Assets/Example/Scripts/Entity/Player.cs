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
        public PlayerAction Action => GetComponent<PlayerAction>(4);
        public PlayerSender Sender => GetComponent<PlayerSender>(1);
        public PlayerFeature Feature => GetComponent<PlayerFeature>(0);
        public PlayerMachine Machine => GetComponent<PlayerMachine>(2);

        private void Update()
        {
            if (isOwner)
            {
                Action.Tick();
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