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
using Astraia.Core;

namespace Astraia
{
    [Serializable]
    public abstract class UIPanel : Module<Entity>, IModule, ISystem
    {
        public UIState state = UIState.Common;
        internal int group;
        internal int layer;

        void IModule.Acquire(Entity owner)
        {
            this.owner = owner;
            var panel = Service.Ref<UIMaskAttribute>.GetAttribute(GetType());
            if (panel != null)
            {
                layer = panel.layer;
                group = panel.group;
            }
        }

        int ISystem.index => 10;

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

    internal sealed class UIStack
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        private UIPanel current;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        private UIPanel reverse;

        public void Push(UIPanel panel)
        {
            if (current == panel)
            {
                return;
            }

            if (current)
            {
                UIGroup.SetActive(current, false);
                reverse = current;
            }

            current = panel;
            UIGroup.SetActive(current, true);
        }

        public void Back(UIPanel panel)
        {
            if (reverse == null)
            {
                return;
            }

            if (current != panel)
            {
                return;
            }

            var forward = current;
            if (current)
            {
                UIGroup.SetActive(current, false);
            }

            current = reverse;
            reverse = forward;
            UIGroup.SetActive(current, true);
        }

        public void Clear()
        {
            if (current)
            {
                UIGroup.SetActive(current, false);
            }

            reverse = null;
            current = null;
        }
    }
}