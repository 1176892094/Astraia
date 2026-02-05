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
using System.Collections.Generic;
using Astraia.Core;
using Sirenix.OdinInspector;

namespace Astraia
{
    [Serializable]
    public abstract class UIPanel : Module<Entity>, IModule, ISystem
    {
        public UIState state = UIState.Common;
        internal int layer;
        internal int group;

        int ISystem.index => 10;

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

    [Serializable]
    internal sealed class UIStack
    {
        [ShowInInspector, HideLabel] private Stack<UIPanel> Stack = new Stack<UIPanel>();
        private UIPanel Current => Stack.Count > 0 ? Stack.Peek() : null;

        public void Push(UIPanel panel)
        {
            if (Current == panel)
            {
                return;
            }

            if (Stack.Contains(panel))
            {
                while (Stack.Count > 0)
                {
                    var other = Stack.Peek();
                    if (other == panel)
                    {
                        break;
                    }

                    Stack.Pop();
                    UIGroup.SetActive(other, false);
                }

                UIGroup.SetActive(Current, true);
                return;
            }

            if (Current != null)
            {
                UIGroup.SetActive(Current, false);
            }

            Stack.Push(panel);
            UIGroup.SetActive(panel, true);
        }

        public void Pop()
        {
            if (Stack.Count == 0)
            {
                return;
            }

            var panel = Stack.Pop();
            UIGroup.SetActive(panel, false);

            if (Current != null)
            {
                UIGroup.SetActive(Current, true);
            }
        }

        public void Remove(UIPanel panel)
        {
            var copies = new Stack<UIPanel>();

            while (Stack.Count > 0)
            {
                var other = Stack.Pop();
                if (other == panel)
                {
                    break;
                }

                copies.Push(other);
            }

            while (copies.Count > 0)
            {
                var other = copies.Pop();
                Stack.Push(other);
            }
        }

        public void Clear()
        {
            while (Stack.Count > 0)
            {
                var panel = Stack.Pop();
                if (panel)
                {
                    UIGroup.SetActive(panel, false);
                }
            }
        }
    }
}