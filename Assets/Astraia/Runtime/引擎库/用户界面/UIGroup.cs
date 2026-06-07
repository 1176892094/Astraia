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
                stack = new UIStack();
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
            panel.owner.gameObject.SetActive(false);
            Object.Destroy(panel.owner.gameObject);
            panelData.Remove(type);
        }

        internal static void SetActive(UIPanel panel, bool state)
        {
            if (state != panel.owner.gameObject.activeSelf)
            {
                if (state)
                {
                    panel.owner.gameObject.SetActive(true);
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
                    panel.owner.gameObject.SetActive(false);
                }
            }
        }
    }
}