// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 23:01:36
// # Recently: 2025-01-10 20:01:58
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Reflection;
using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public abstract partial class UIPanel : Module<Entity>, IModule, IActive, IPanel
    {
        public UIState state = UIState.Common;

        public virtual void Update()
        {
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }
    }

    public abstract partial class UIPanel
    {
        [SerializeField] internal int groupMask;
        [SerializeField] internal int layerMask;

        void IModule.Create(Entity owner) => Create(owner);

        internal void Listen()
        {
            GlobalManager.panelLoop.Add(this);
        }

        internal void Remove()
        {
            GlobalManager.panelLoop.Remove(this);
        }

        internal virtual void Create(Entity owner)
        {
            this.owner = owner;
            var combineMask = 0;
            var current = GetType();
            var layerAttr = current.GetCustomAttribute<UILayerAttribute>(true);
            if (layerAttr != null)
            {
                layerMask = layerAttr.layerMask;
            }

            while (current != null)
            {
                var groupAttr = current.GetCustomAttribute<UIGroupAttribute>(false);
                if (groupAttr != null)
                {
                    combineMask |= groupAttr.groupMask;
                }

                current = current.BaseType;
            }


            if (combineMask != 0)
            {
                groupMask = combineMask;
                for (var i = 0; i < 32; i++)
                {
                    var bit = 1 << i;
                    if ((groupMask & bit) != 0)
                    {
                        UIGroup.Listen(bit, this);
                    }
                }
            }

            owner.OnFade += () =>
            {
                if (groupMask == 0) return;
                for (var i = 0; i < 32; i++)
                {
                    var bit = 1 << i;
                    if ((groupMask & bit) != 0)
                    {
                        UIGroup.Remove(bit, this);
                    }
                }

                groupMask = 0;
            };
        }
    }
}