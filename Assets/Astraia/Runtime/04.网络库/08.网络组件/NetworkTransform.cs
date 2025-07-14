using System;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    public class NetworkTransform : NetworkSource
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

        private double keepTime = double.MinValue;

        private bool isModify
        {
            get
            {
                if (!NetworkSystem.Tick(NetworkManager.Instance.sendRate, ref sendTime))
                {
                    return false;
                }

                cachedPosition = target.position;
                cachedRotation = target.rotation;
                cachedScale = target.localScale;
                positionChanged = Vector3.SqrMagnitude(originPosition - target.position) > positionPerceive * positionPerceive;
                rotationChanged = Quaternion.Angle(originRotation, target.rotation) > rotationPerceive;
                scaleChanged = Vector3.SqrMagnitude(originScale - target.localScale) > scalePerceive * scalePerceive;

                var unchanged = !positionChanged && !rotationChanged && !scaleChanged;
                if (unchanged && sendUnchanged)
                {
                    if (keepTime > Time.unscaledTime)
                    {
                        return true;
                    }

                    return false;
                }

                sendUnchanged = unchanged;
                if (!sendUnchanged)
                {
                    keepTime = Time.unscaledTime + 0.1;
                    originPosition = target.position;
                    originRotation = target.rotation;
                    originScale = target.localScale;
                }

                cachedPosition = (option & TransformOption.Position) != 0 && positionChanged ? cachedPosition : null;
                cachedRotation = (option & TransformOption.Rotation) != 0 && rotationChanged ? cachedRotation : null;
                cachedScale = (option & TransformOption.Scale) != 0 && scaleChanged ? cachedScale : null;
                return true;
            }
        }

        public override void OnAwake()
        {
            target = transform;
            originPosition = target.position;
            originRotation = target.rotation;
            originScale = target.localScale;
        }

        public override void OnDestroy()
        {
            sendTime = double.MinValue;
        }

        public override void OnUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (isServer && !isVerify)
            {
                SyncPosition();
            }
            else if (isClient && !isVerify && NetworkManager.Client.isReady)
            {
                SyncPosition();
            }
        }

        public override void OnLateUpdate()
        {
            if (target == null)
            {
                return;
            }

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
                var isLerp = Vector3.Distance(target.position, originPosition) > 0.02f;
                target.position = isLerp ? Vector3.Lerp(target.position, originPosition, positionSmooth) : originPosition;
            }

            if ((option & TransformOption.Rotation) != 0)
            {
                var isLerp = Quaternion.Angle(target.rotation, originRotation) > 0.02f;
                target.rotation = isLerp ? Quaternion.Lerp(target.rotation, originRotation, rotationSmooth) : originRotation;
            }

            if ((option & TransformOption.Scale) != 0)
            {
                var isLerp = Vector3.Distance(target.localScale, originScale) > 0.02f;
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
            if ((option & TransformOption.Position) != 0) writer.SetVector3(target.localPosition);
            if ((option & TransformOption.Rotation) != 0) writer.SetQuaternion(target.localRotation);
            if ((option & TransformOption.Scale) != 0) writer.SetVector3(target.localScale);
        }

        protected override void OnDeserialize(MemoryReader reader, bool initialize)
        {
            if (!initialize) return;
            if ((option & TransformOption.Position) != 0) originPosition = reader.GetVector3();
            if ((option & TransformOption.Rotation) != 0) originRotation = reader.GetQuaternion();
            if ((option & TransformOption.Scale) != 0) originScale = reader.GetVector3();
        }

        [ServerRpc(Channel.Unreliable)]
        private void SendToServerRpc(Vector3? position, Quaternion? rotation, Vector3? localScale)
        {
            if (syncDirection == SyncMode.Client && !isClient)
            {
                originPosition = position ?? target.position;
                originRotation = rotation ?? target.rotation;
                originScale = localScale ?? target.localScale;
            }

            SendToClientRpc(position, rotation, localScale);
        }

        [ClientRpc(Channel.Unreliable)]
        private void SendToClientRpc(Vector3? position, Quaternion? rotation, Vector3? localScale)
        {
            if ((syncDirection == SyncMode.Server && !isServer) || (syncDirection == SyncMode.Client && !isOwner))
            {
                Debug.LogWarning(position);
                originPosition = position ?? target.position;
                originRotation = rotation ?? target.rotation;
                originScale = localScale ?? target.localScale;
            }
        }
    }
}