using System;
using Object = UnityEngine.Object;

namespace Astraia.Core
{
    using static GlobalManager;

    public interface ITween
    {
    }

    public static class UIGroup
    {
        public static void Hide(int value)
        {
            if (stackData.TryGetValue(value, out var stack))
            {
                stack.Clear();
            }
        }

        internal static void Show(UIPanel panel)
        {
            if (panel.group == 0)
            {
                SetActive(panel, true);
                return;
            }

            if (!stackData.TryGetValue(panel.group, out var stack))
            {
                stack = new UIQueue();
                stackData.Add(panel.group, stack);
            }

            stack.Push(panel);
        }

        internal static void Hide(UIPanel panel)
        {
            if (panel.group == 0)
            {
                SetActive(panel, false);
                return;
            }

            if (stackData.TryGetValue(panel.group, out var stack))
            {
                stack.Back(panel);
            }
        }

        internal static void Destroy(UIPanel panel, Type type)
        {
            SetActive(panel, false);
            Object.Destroy(panel.owner.gameObject);
            panelData.Remove(type);
        }

        internal static void SetActive(UIPanel panel, bool state)
        {
            var owner = panel.owner.gameObject;
            if (state != owner.activeSelf)
            {
                if (state)
                {
                    owner.SetActive(true);
                    panel.OnShow();
                    return;
                }

                if (panel is ITween)
                {
                    panel.OnHide();
                }
                else
                {
                    panel.OnHide();
                    owner.SetActive(false);
                }
            }
        }
    }
}