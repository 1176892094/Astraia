// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 16:01:50
// # Recently: 2025-01-10 20:01:57
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia.Core
{
    using static GlobalManager;

    public interface ITween
    {
    }

    public static class UIManager
    {
        private static Entity Load(string path, string name)
        {
            var asset = AssetManager.Load<GameObject>(path);
            asset.SetActive(false);
            asset.gameObject.name = name;
            return asset.GetOrAddComponent<Entity>();
        }

        private static UIPanel Load(string path, Type type)
        {
            var item = Service.Ref<UIPathAttribute>.GetAttribute(type);
            if (item != null)
            {
                path = GlobalSetting.Prefab.Format(item.asset);
            }

            var panel = (UIPanel)Load(path, type.Name).LoadComponent(type, type);
            SetLayer(panel.transform, panel.layer);
            panelData.Add(type, panel);
            return panel;
        }

        public static T Show<T>() where T : UIPanel
        {
            if (!Instance) return null;
            if (!panelData.TryGetValue(typeof(T), out var panel))
            {
                panel = Load(GlobalSetting.Prefab.Format(typeof(T).Name), typeof(T));
            }

            UIGroup.Show(panel);
            return (T)panel;
        }

        public static void Hide<T>() where T : UIPanel
        {
            if (!Instance) return;
            if (panelData.TryGetValue(typeof(T), out var panel))
            {
                UIGroup.Hide(panel);
            }
        }

        public static T Find<T>() where T : UIPanel
        {
            return panelData.TryGetValue(typeof(T), out var panel) ? (T)panel : null;
        }

        public static void Destroy<T>()
        {
            if (!Instance) return;
            if (panelData.TryGetValue(typeof(T), out var panel))
            {
                UIGroup.Destroy(panel, typeof(T));
            }
        }

        public static UIPanel Show(Type type)
        {
            if (!Instance) return null;
            if (!panelData.TryGetValue(type, out var panel))
            {
                panel = Load(GlobalSetting.Prefab.Format(type.Name), type);
            }

            UIGroup.Show(panel);
            return panel;
        }

        public static void Hide(Type type)
        {
            if (!Instance) return;
            if (panelData.TryGetValue(type, out var panel))
            {
                UIGroup.Hide(panel);
            }
        }

        public static UIPanel Find(Type type)
        {
            return type != null ? panelData.TryGetValue(type, out var panel) ? panel : null : null;
        }

        public static void Destroy(Type type)
        {
            if (!Instance) return;
            if (panelData.TryGetValue(type, out var panel))
            {
                UIGroup.Destroy(panel, type);
            }
        }

        public static void Destroy()
        {
            var copies = new List<Type>(panelData.Keys);
            foreach (var result in copies)
            {
                if (panelData.TryGetValue(result, out var panel))
                {
                    if (panel.state != UIState.Stable)
                    {
                        UIGroup.Destroy(panel, result);
                    }
                }
            }
        }

        public static void SetLayer(Transform panel, int layer)
        {
            if (!Instance) return;
            if (!layerData.TryGetValue(layer, out var parent))
            {
                var format = "Pool - Canvas-{0}".Format(layer);
                parent = new GameObject(format).AddComponent<RectTransform>();
                parent.gameObject.layer = LayerMask.NameToLayer("UI");
                SetTransform(parent, Instance.canvas.transform);
                parent.SetSiblingIndex(layerData.Keys.Count(key => key < layer));
                layerData.Add(layer, parent);
            }

            SetTransform(panel.GetComponent<RectTransform>(), parent);
        }

        private static void SetTransform(RectTransform rect, Transform parent)
        {
            rect.SetParent(parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
        }

        internal static void Dispose()
        {
            foreach (var stack in stackData.Values)
            {
                stack.Clear();
            }

            stackData.Clear();
            layerData.Clear();
            panelData.Clear();
        }
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
            panel.gameObject.SetActive(false);
            Object.Destroy(panel.gameObject);
            panelData.Remove(type);
        }

        internal static void SetActive(UIPanel panel, bool state)
        {
            if (state != panel.gameObject.activeSelf)
            {
                if (state)
                {
                    panel.gameObject.SetActive(true);
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
                    panel.gameObject.SetActive(false);
                }
            }
        }
    }
}