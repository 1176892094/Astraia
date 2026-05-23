using UnityEngine;

namespace Runtime
{
    public class PlayerFeature : MonoBehaviour
    {
        public int MoveSpeed;
        public int VelocityX;
        public int VelocityY;
        public int PositionX;
        public int PositionY;

        public int JumpForce;
        public int JumpCount;
        public float JumpInput;
        public float JumpTimer;
        public float JumpCD;

        public int DashSpeed;
        public int DashCount;
        public float DashInput;
        public float DashTimer;
        public float DashCD;

        public int CrashSpeed;
        public int CrashCount;
        public Vector3 CrashPoint;

        private void Awake()
        {
            MoveSpeed = 30;
            JumpForce = MoveSpeed * 2;
            DashSpeed = MoveSpeed * 3;
            CrashSpeed = MoveSpeed * 2;
            PositionY = 100;
        }
    }
}