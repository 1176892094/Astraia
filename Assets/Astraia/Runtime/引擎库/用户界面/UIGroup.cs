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
            if (QueueData.TryGetValue(value, out var queue))
            {
                queue.Hide();
            }
        }

        internal static void Show(UIPanel panel)
        {
            if (panel.group == 0)
            {
                Modified(panel, true);
                return;
            }

            if (!QueueData.TryGetValue(panel.group, out var queue))
            {
                queue = new UIQueue();
                QueueData.Add(panel.group, queue);
            }

            queue.Push(panel);
        }

        internal static void Hide(UIPanel panel)
        {
            if (panel.group == 0)
            {
                Modified(panel, false);
                return;
            }

            if (QueueData.TryGetValue(panel.group, out var queue))
            {
                queue.Back(panel);
            }
        }

        internal static void Destroy(UIPanel panel, Type type)
        {
            Modified(panel, false);
            Object.Destroy(panel.owner.gameObject);
            PanelData.Remove(type);
        }

        internal static void Modified(UIPanel panel, bool state)
        {
            var owner = panel.owner.gameObject;
            if (state != owner.activeSelf)
            {
                if (state)
                {
                    owner.SetActive(true);
                    panel.OnShow();
                }
                else if (panel is ITween)
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