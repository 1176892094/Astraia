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

namespace Astraia
{
    [Serializable]
    public abstract partial class UIPanel : Module<Entity>, IModule, IActive
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

    public abstract partial class UIPanel : ISystem
    {
        internal int layerMask;
        internal int groupMask;

        void IModule.Create(Entity owner) => Create(owner);
        void ISystem.AddSystem() => GlobalManager.panelLoop.Add(this);
        void ISystem.SubSystem() => GlobalManager.panelLoop.Remove(this);

        internal virtual void Create(Entity owner)
        {
            this.owner = owner;
            var current = GetType();
            var attribute = current.GetCustomAttribute<UIMaskAttribute>(true);
            if (attribute != null)
            {
                layerMask = attribute.layerMask;
            }

            groupMask = 0;
            while (current != null)
            {
                attribute = current.GetCustomAttribute<UIMaskAttribute>(false);
                if (attribute != null)
                {
                    groupMask |= attribute.groupMask;
                }

                current = current.BaseType;
            }

            if (groupMask != 0)
            {
                Register();
                owner.OnFade += UnRegister;
            }
        }

        private void Register()
        {
            for (var i = 0; i < 32; i++)
            {
                var bit = 1 << i;
                if ((groupMask & bit) != 0)
                {
                    UIGroup.Listen(bit, this);
                }
            }
        }

        private void UnRegister()
        {
            for (var i = 0; i < 32; i++)
            {
                var bit = 1 << i;
                if ((groupMask & bit) != 0)
                {
                    UIGroup.Remove(bit, this);
                }
            }
        }
    }
}