// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 15:09:52
// # Recently: 2026-09-03 14:20:59
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia
{
    public class UIManager : Singleton<UIManager>, IDontDestroy
    {
        private readonly Dictionary<int, UIStack> stackData = new Dictionary<int, UIStack>();
        private readonly Dictionary<int, UIPanel> panelData = new Dictionary<int, UIPanel>();

        protected override void Awake()
        {
            base.Awake();
            foreach (RectTransform layer in transform)
            {
                foreach (RectTransform child in layer)
                {
                    if (child.TryGetComponent(out UIPanel panel))
                    {
                        if (!stackData.ContainsKey(panel.state) && panel.state != 0)
                        {
                            stackData.Add(panel.state, new UIStack());
                        }

                        panelData.Add(GetHash(panel.GetType()), panel);
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            stackData.Clear();
            panelData.Clear();
            base.OnDestroy();
        }

        public static T Show<T>() where T : UIPanel
        {
            return Instance?.ShowInternal<T>();
        }

        public static T Find<T>() where T : UIPanel
        {
            return Instance?.FindInternal<T>();
        }

        public static void Hide<T>() where T : UIPanel
        {
            Instance?.HideInternal<T>();
        }

        public static UIPanel Show(Type value)
        {
            return Instance?.ShowInternal(value);
        }

        public static UIPanel Find(Type value)
        {
            return Instance?.FindInternal(value);
        }

        public static void Hide(Type value)
        {
            Instance?.HideInternal(value);
        }

        public static void Hide(int value)
        {
            Instance?.HideInternal(value);
        }

        private T ShowInternal<T>() where T : UIPanel
        {
            if (panelData.TryGetValue(GetHash(typeof(T)), out var panel))
            {
                ShowInGroup(panel);
                return (T)panel;
            }

            return null;
        }

        private T FindInternal<T>() where T : UIPanel
        {
            return (T)panelData.GetValueOrDefault(GetHash(typeof(T)));
        }

        private void HideInternal<T>() where T : UIPanel
        {
            if (panelData.TryGetValue(GetHash(typeof(T)), out var panel))
            {
                HideInGroup(panel);
            }
        }

        private UIPanel ShowInternal(Type value)
        {
            if (panelData.TryGetValue(GetHash(value), out var panel))
            {
                ShowInGroup(panel);
                return panel;
            }

            return null;
        }

        private UIPanel FindInternal(Type value)
        {
            return panelData.GetValueOrDefault(GetHash(value));
        }

        private void HideInternal(Type value)
        {
            if (panelData.TryGetValue(GetHash(value), out var panel))
            {
                HideInGroup(panel);
            }
        }

        private void HideInternal(int index)
        {
            if (stackData.TryGetValue(index, out var panel))
            {
                panel.Hide();
            }
        }

        private void ShowInGroup(UIPanel panel)
        {
            if (panel.state == 0)
            {
                Modified(panel, true);
                return;
            }

            if (!stackData.TryGetValue(panel.state, out var stack))
            {
                stack = new UIStack();
                stackData.Add(panel.state, stack);
            }

            stack.Push(panel);
        }

        private void HideInGroup(UIPanel panel)
        {
            if (panel.state == 0)
            {
                Modified(panel, false);
                return;
            }

            if (stackData.TryGetValue(panel.state, out var stack))
            {
                stack.Back(panel);
            }
        }

        private static int GetHash(Type value)
        {
            var result = 23;
            foreach (var c in value.FullName!)
            {
                result = result * 31 + c;
            }

            return result;
        }

        private static void Modified(UIPanel panel, bool state)
        {
            var owner = panel.gameObject;
            if (state != owner.activeSelf)
            {
                if (state)
                {
                    owner.SetActive(true);
                    panel.ShowInternal();
                }
                else if (panel is ITween)
                {
                    panel.HideInternal();
                }
                else
                {
                    panel.HideInternal();
                    owner.SetActive(false);
                }
            }
        }

        [Serializable]
        private sealed class UIStack
        {
            public UIPanel current;
            public UIPanel reverse;

            public void Push(UIPanel panel)
            {
                if (current == panel)
                {
                    return;
                }

                if (current)
                {
                    Modified(current, false);
                    reverse = current;
                }

                current = panel;
                Modified(current, true);
            }

            public void Back(UIPanel panel)
            {
                if (current != panel)
                {
                    return;
                }

                if (reverse)
                {
                    if (current)
                    {
                        Modified(current, false);
                    }

                    (current, reverse) = (reverse, current);
                    Modified(current, true);
                }
            }

            public void Hide()
            {
                if (current)
                {
                    Modified(current, false);
                }

                reverse = null;
                current = null;
            }
        }
    }
}