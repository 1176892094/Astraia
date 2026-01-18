using System;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    public class NetworkTransform : NetworkModule
    {
        [SerializeField] private Transform target;

        [SerializeField] private Option option = Option.Position;

        [SerializeField, Range(0f, 1)] private float positionPerceive = 0.01f;

        [SerializeField, Range(0f, 1)] private float rotationPerceive = 0.01f;

        [SerializeField, Range(0f, 1)] private float mutationPerceive = 0.01f;

        [SerializeField, Range(0f, 1)] private float positionSmooth = 0.5f;

        [SerializeField, Range(0f, 1)] private float rotationSmooth = 0.5f;

        [SerializeField, Range(0f, 1)] private float mutationSmooth = 0.5f;

        private Vector3 position;

        private Quaternion rotation;

        private Vector3 mutation;

        private Vector3? positionCache;

        private Quaternion? rotationCache;

        private Vector3? mutationCache;

        private bool positionChanged;

        private bool rotationChanged;

        private bool mutationChanged;

        private bool unchangedCache;

        private double sendTime = double.MinValue;

        private bool isModify
        {
            get
            {
                if (!NetworkSystem.Tick(ref sendTime))
                {
                    return false;
                }

                positionChanged = Vector3.SqrMagnitude(position - target.position) > positionPerceive * positionPerceive;
                rotationChanged = Quaternion.Angle(rotation, target.rotation) > rotationPerceive;
                mutationChanged = Vector3.SqrMagnitude(mutation - target.localScale) > mutationPerceive * mutationPerceive;

                var unchanged = !positionChanged && !rotationChanged && !mutationChanged;
                if (unchanged && unchangedCache)
                {
                    return false;
                }

                unchangedCache = unchanged;
                if (!unchangedCache)
                {
                    position = target.position;
                    rotation = target.rotation;
                    mutation = target.localScale;
                }

                positionCache = (option & Option.Position) != 0 && positionChanged ? target.position : null;
                rotationCache = (option & Option.Rotation) != 0 && rotationChanged ? target.rotation : null;
                mutationCache = (option & Option.Mutation) != 0 && mutationChanged ? target.localScale : null;
                return true;
            }
        }

        public override void Dequeue()
        {
            target = transform;
            position = target.position;
            rotation = target.rotation;
            mutation = target.localScale;
        }

        public override void Enqueue()
        {
            sendTime = double.MinValue;
        }

        public void Update()
        {
            if (isServer && !isVerify)
            {
                SyncPosition();
            }
            else if (isClient && !isVerify && NetworkManager.Client.isReady)
            {
                SyncPosition();
            }
        }

        public void LateUpdate()
        {
            if (isServer && isVerify && NetworkManager.isServer && isModify)
            {
                SendToClientRpc(positionCache, rotationCache, mutationCache);
            }
            else if (isClient && isVerify && NetworkManager.Client.isReady && isModify)
            {
                SendToServerRpc(positionCache, rotationCache, mutationCache);
            }
        }

        private void SyncPosition()
        {
            if ((option & Option.Position) != 0)
            {
                var isLerp = Vector3.Distance(target.position, position) < 0.5f;
                target.position = isLerp ? Vector3.Lerp(target.position, position, positionSmooth) : position;
            }

            if ((option & Option.Rotation) != 0)
            {
                var isLerp = Quaternion.Angle(target.rotation, rotation) < 0.5f;
                target.rotation = isLerp ? Quaternion.Lerp(target.rotation, rotation, rotationSmooth) : rotation;
            }

            if ((option & Option.Mutation) != 0)
            {
                var isLerp = Vector3.Distance(target.localScale, mutation) < 0.5f;
                target.localScale = isLerp ? Vector3.Lerp(target.localScale, mutation, mutationSmooth) : mutation;
            }
        }

        public void SyncTransform(Vector3? position, Quaternion? rotation = null, Vector3? mutation = null)
        {
            target.position = this.position = position ?? target.position;
            target.rotation = this.rotation = rotation ?? target.rotation;
            target.localScale = this.mutation = mutation ?? target.localScale;
        }

        protected override void OnSerialize(MemoryWriter writer, bool initialize)
        {
            if (!initialize) return;
            if ((option & Option.Position) != 0) writer.WriteVector3(target.localPosition);
            if ((option & Option.Rotation) != 0) writer.WriteQuaternion(target.localRotation);
            if ((option & Option.Mutation) != 0) writer.WriteVector3(target.localScale);
        }

        protected override void OnDeserialize(MemoryReader reader, bool initialize)
        {
            if (!initialize) return;
            if ((option & Option.Position) != 0) position = reader.ReadVector3();
            if ((option & Option.Rotation) != 0) rotation = reader.ReadQuaternion();
            if ((option & Option.Mutation) != 0) mutation = reader.ReadVector3();
        }

        [ServerRpc(Channel.Unreliable)]
        private void SendToServerRpc(Vector3? position, Quaternion? rotation, Vector3? mutation)
        {
            if (syncDirection == SyncMode.Client && !isClient)
            {
                if (position != null) this.position = position.Value;
                if (rotation != null) this.rotation = rotation.Value;
                if (mutation != null) this.mutation = mutation.Value;
            }

            SendToClientRpc(position, rotation, mutation);
        }

        [ClientRpc(Channel.Unreliable)]
        private void SendToClientRpc(Vector3? position, Quaternion? rotation, Vector3? mutation)
        {
            if ((syncDirection == SyncMode.Server && !isServer) || (syncDirection == SyncMode.Client && !isOwner))
            {
                if (position != null) this.position = position.Value;
                if (rotation != null) this.rotation = rotation.Value;
                if (mutation != null) this.mutation = mutation.Value;
            }
        }

        [Flags]
        public enum Option : byte
        {
            None,
            Position = 1 << 0,
            Rotation = 1 << 1,
            Mutation = 1 << 2,
        }
        // NetworkTransform
        // 总长：1B   ulong(压缩)
        // 类型：2B   ushort
        // 对象：1B   uint(压缩)
        // 组件：1B   byte
        // 方法：2B   ushort
        // 片段：1B   Segment(压缩) 13+17+13 = 43 不超过 127字节 长度为 1
        // 位置：12+1 Vector3? 
        // 旋转：16+1 Quaternion? 
        // 缩放：12+1 Vector3? 
        // TODO：同步位置：1+2+1+1+2+1+13+1+1 = 23B
        // TODO：每秒同步：23*30 = 690B

        // SyncVar Color
        // 总长：1B   ulong(压缩)
        // 类型：2B   ushort
        // 遮罩：1B   ulong(压缩)  判断组件变动组件 最多 64 NetworkModule
        // 索引：1B   ulong(压缩)  网络变量最大数量 最多 64 SyncVar
        // 校验：1B   byte  
        // 对象：1B   uint(压缩)
        // 片段：1B   Segment(压缩) 16 不超过 127字节 长度为 1
        // 变量：16B   Color
        // TODO：同步颜色：1+2+1+1+1+1+1+16 = 24B

        //网络变量大小<127B： 8 + size
        //远程调用大小<127B： 8 + size
    }
}