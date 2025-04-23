using UnityEngine;

namespace Astraia.Net
{
    public class NetworkTransform : NetworkBehaviour
    {
        [SerializeField] private Transform target;

        [SerializeField] private TransformOption option = TransformOption.Position;

        [SerializeField, Range(0f, 1)] private float positionPerceive = 0.01f;

        [SerializeField, Range(0f, 1)] private float rotationPerceive = 0.01f;

        [SerializeField, Range(0f, 1)] private float scalePerceive = 0.01f;

        [SerializeField, Range(0f, 1)] private float positionSmooth = 0.5f;

        [SerializeField, Range(0f, 1)] private float rotationSmooth = 0.5f;

        [SerializeField, Range(0f, 1)] private float localScaleSmooth = 0.5f;

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

                cachedPosition = target.position;
                cachedRotation = target.rotation;
                cachedScale = target.localScale;
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

                cachedPosition = (option & TransformOption.Position) != 0 && positionChanged ? cachedPosition : null;
                cachedRotation = (option & TransformOption.Rotation) != 0 && rotationChanged ? cachedRotation : null;
                cachedScale = (option & TransformOption.Scale) != 0 && scaleChanged ? cachedScale : null;
                return true;
            }
        }

        private void Awake()
        {
            originPosition = target.position;
            originRotation = target.rotation;
            originScale = target.localScale;
        }

        private void Update()
        {
            if (isServer && !isVerify)
            {
                if ((option & TransformOption.Position) != 0)
                    target.position = Vector3.Distance(target.position, originPosition) < positionSmooth
                        ? Vector3.Lerp(target.position, originPosition, positionSmooth)
                        : originPosition;
                if ((option & TransformOption.Rotation) != 0)
                    target.rotation = Quaternion.Angle(target.rotation, originRotation) < rotationSmooth
                        ? Quaternion.Lerp(target.rotation, originRotation, rotationSmooth)
                        : originRotation;
                if ((option & TransformOption.Scale) != 0)
                    target.localScale = Vector3.Distance(target.localScale, originScale) < localScaleSmooth
                        ? Vector3.Lerp(target.localScale, originScale, localScaleSmooth)
                        : originScale;
            }
            else if (isClient && !isVerify && NetworkManager.Client.isReady)
            {
                if ((option & TransformOption.Position) != 0)
                    target.position = Vector3.Distance(target.position, originPosition) < positionSmooth
                        ? Vector3.Lerp(target.position, originPosition, positionSmooth)
                        : originPosition;
                if ((option & TransformOption.Rotation) != 0)
                    target.rotation = Quaternion.Angle(target.rotation, originRotation) < rotationSmooth
                        ? Quaternion.Lerp(target.rotation, originRotation, rotationSmooth)
                        : originRotation;
                if ((option & TransformOption.Scale) != 0)
                    target.localScale = Vector3.Distance(target.localScale, originScale) < localScaleSmooth
                        ? Vector3.Lerp(target.localScale, originScale, localScaleSmooth)
                        : originScale;
            }
        }

        private void LateUpdate()
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

        private void OnValidate()
        {
            if (target == null)
            {
                target = transform;
            }
        }

        public void SyncTransform(Vector3? position, Quaternion? rotation = null, Vector3? localScale = null)
        {
            target.position = originPosition = position ?? target.position;
            target.rotation = originRotation = rotation ?? target.rotation;
            target.localScale = originScale = localScale ?? target.localScale;
        }

        protected override void OnSerialize(MemorySetter setter, bool status)
        {
            if (!status) return;
            if ((option & TransformOption.Position) != 0) setter.SetVector3(target.localPosition);
            if ((option & TransformOption.Rotation) != 0) setter.SetQuaternion(target.localRotation);
            if ((option & TransformOption.Scale) != 0) setter.SetVector3(target.localScale);
        }

        protected override void OnDeserialize(MemoryGetter getter, bool status)
        {
            if (!status) return;
            if ((option & TransformOption.Position) != 0) originPosition = getter.GetVector3();
            if ((option & TransformOption.Rotation) != 0) originRotation = getter.GetQuaternion();
            if ((option & TransformOption.Scale) != 0) originScale = getter.GetVector3();
        }

        [ServerRpc]
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

        [ClientRpc]
        private void SendToClientRpc(Vector3? position, Quaternion? rotation, Vector3? localScale)
        {
            if ((syncDirection == SyncMode.Server && !isServer) || (syncDirection == SyncMode.Client && !isOwner))
            {
                originPosition = position ?? target.position;
                originRotation = rotation ?? target.rotation;
                originScale = localScale ?? target.localScale;
            }
        }
    }
}