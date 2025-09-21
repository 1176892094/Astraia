using System;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    public class NetworkTransform : NetworkModule
    {
        [SerializeField] private Transform target;

        [SerializeField] private TransformOption option = TransformOption.Position;

        [SerializeField, Range(0f, 1)] private float positionPerceive = 0.01f;

        [SerializeField, Range(0f, 1)] private float rotationPerceive = 0.01f;

        [SerializeField, Range(0f, 1)] private float scalePerceive = 0.01f;

        [SerializeField, Range(0f, 1)] private float positionSmooth = 0.5f;

        [SerializeField, Range(0f, 1)] private float rotationSmooth = 0.5f;

        [SerializeField, Range(0f, 1)] private float scaleSmooth = 0.5f;

        private Vector3? cachedPosition;

        private Quaternion? cachedRotation;

        private Vector3? cachedScale;

        private Vector3 originPosition;

        private Quaternion originRotation;

        private Vector3 originScale;

        private bool positionChanged;

        private bool rotationChanged;

        private bool scaleChanged;

        private bool sendUnchanged;

        private double sendTime = double.MinValue;

        private bool isModify
        {
            get
            {
                if (!NetworkSystem.Tick(NetworkManager.Instance.sendRate, ref sendTime))
                {
                    return false;
                }

                positionChanged = Vector3.SqrMagnitude(originPosition - target.position) > positionPerceive * positionPerceive;
                rotationChanged = Quaternion.Angle(originRotation, target.rotation) > rotationPerceive;
                scaleChanged = Vector3.SqrMagnitude(originScale - target.localScale) > scalePerceive * scalePerceive;

                var unchanged = !positionChanged && !rotationChanged && !scaleChanged;
                if (unchanged && sendUnchanged)
                {
                    return false;
                }

                sendUnchanged = unchanged;
                if (!sendUnchanged)
                {
                    originPosition = target.position;
                    originRotation = target.rotation;
                    originScale = target.localScale;
                }

                cachedPosition = (option & TransformOption.Position) != 0 && positionChanged ? target.position : null;
                cachedRotation = (option & TransformOption.Rotation) != 0 && rotationChanged ? target.rotation : null;
                cachedScale = (option & TransformOption.Scale) != 0 && scaleChanged ? target.localScale : null;
                return true;
            }
        }

        public override void Dequeue()
        {
            target = transform;
            originPosition = target.position;
            originRotation = target.rotation;
            originScale = target.localScale;
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
            if (isServer && isVerify && isModify)
            {
                SendToClientRpc(cachedPosition, cachedRotation, cachedScale);
            }
            else if (isClient && isVerify && NetworkManager.Client.isReady && isModify)
            {
                SendToServerRpc(cachedPosition, cachedRotation, cachedScale);
            }
        }

        private void SyncPosition()
        {
            if ((option & TransformOption.Position) != 0)
            {
                var isLerp = Vector3.Distance(target.position, originPosition) < 0.5f;
                target.position = isLerp ? Vector3.Lerp(target.position, originPosition, positionSmooth) : originPosition;
            }

            if ((option & TransformOption.Rotation) != 0)
            {
                var isLerp = Quaternion.Angle(target.rotation, originRotation) < 0.5f;
                target.rotation = isLerp ? Quaternion.Lerp(target.rotation, originRotation, rotationSmooth) : originRotation;
            }

            if ((option & TransformOption.Scale) != 0)
            {
                var isLerp = Vector3.Distance(target.localScale, originScale) < 0.5f;
                target.localScale = isLerp ? Vector3.Lerp(target.localScale, originScale, scaleSmooth) : originScale;
            }
        }

        public void SyncTransform(Vector3? position, Quaternion? rotation = null, Vector3? localScale = null)
        {
            target.position = originPosition = position ?? target.position;
            target.rotation = originRotation = rotation ?? target.rotation;
            target.localScale = originScale = localScale ?? target.localScale;
        }

        protected override void OnSerialize(MemoryWriter writer, bool initialize)
        {
            if (!initialize) return;
            if ((option & TransformOption.Position) != 0) writer.WriteVector3(target.localPosition);
            if ((option & TransformOption.Rotation) != 0) writer.WriteQuaternion(target.localRotation);
            if ((option & TransformOption.Scale) != 0) writer.WriteVector3(target.localScale);
        }

        protected override void OnDeserialize(MemoryReader reader, bool initialize)
        {
            if (!initialize) return;
            if ((option & TransformOption.Position) != 0) originPosition = reader.ReadVector3();
            if ((option & TransformOption.Rotation) != 0) originRotation = reader.ReadQuaternion();
            if ((option & TransformOption.Scale) != 0) originScale = reader.ReadVector3();
        }

        [ServerRpc(Channel.Unreliable)]
        private void SendToServerRpc(Vector3? position, Quaternion? rotation, Vector3? scale)
        {
            if (syncDirection == SyncMode.Client && !isClient)
            {
                if (position != null) originPosition = position.Value;
                if (rotation != null) originRotation = rotation.Value;
                if (scale != null) originScale = scale.Value;
            }

            SendToClientRpc(position, rotation, scale);
        }

        [ClientRpc(Channel.Unreliable)]
        private void SendToClientRpc(Vector3? position, Quaternion? rotation, Vector3? scale)
        {
            if ((syncDirection == SyncMode.Server && !isServer) || (syncDirection == SyncMode.Client && !isOwner))
            {
                if (position != null) originPosition = position.Value;
                if (rotation != null) originRotation = rotation.Value;
                if (scale != null) originScale = scale.Value;
            }
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