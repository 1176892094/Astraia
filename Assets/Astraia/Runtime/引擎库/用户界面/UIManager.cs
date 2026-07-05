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
    [Serializable]
    public class UIManager : Singleton<UIManager>
    {
        private static readonly Dictionary<int, UIStack> StackData = new Dictionary<int, UIStack>();
        private static readonly Dictionary<int, RectTransform> LayerData = new Dictionary<int, RectTransform>();
        private static readonly Dictionary<Type, UIPanel> PanelData = new Dictionary<Type, UIPanel>();

        public static Canvas Canvas => GlobalManager.Instance.canvas;

        public override void Enqueue()
        {
            StackData.Clear();
            LayerData.Clear();
            PanelData.Clear();
        }

        private static UIPanel Load(string name, Type value)
        {
            var item = GlobalSetting.PREFAB.Format(name);
            if (value.GetAttribute(out UIPathAttribute path))
            {
                item = GlobalSetting.PREFAB.Format(path.asset);
            }

            var asset = AssetManager.Load<GameObject>(item);
            asset.gameObject.SetActive(false);
            asset.gameObject.name = name;

            var owner = asset.AddComponent<Entity>();
            var panel = owner.AddComponent<UIPanel>(value);
            var child = owner.GetComponent<RectTransform>();

            if (value.GetAttribute(out UIMaskAttribute mask))
            {
                panel.Layer = mask.layer;
                panel.Group = mask.group;
            }

            SetTransform(child, GetLayer(panel.Layer));
            PanelData.Add(value, panel);
            return panel;
        }

        public static T Show<T>() where T : UIPanel
        {
            if (!GlobalManager.Instance) return null;
            if (!PanelData.TryGetValue(typeof(T), out var panel))
            {
                panel = Load(typeof(T).Name, typeof(T));
            }

            UIGroup.Show(panel);
            return (T)panel;
        }

        public static void Hide<T>() where T : UIPanel
        {
            if (!GlobalManager.Instance) return;
            if (PanelData.TryGetValue(typeof(T), out var panel))
            {
                UIGroup.Hide(panel);
            }
        }

        public static T Find<T>() where T : UIPanel
        {
            return PanelData.TryGetValue(typeof(T), out var panel) ? (T)panel : null;
        }

        public static void Destroy<T>()
        {
            if (!GlobalManager.Instance) return;
            if (PanelData.TryGetValue(typeof(T), out var panel))
            {
                UIGroup.Remove(panel);
                PanelData.Remove(typeof(T));
            }
        }

        public static UIPanel Show(Type type)
        {
            if (!GlobalManager.Instance) return null;
            if (!PanelData.TryGetValue(type, out var panel))
            {
                panel = Load(type.Name, type);
            }

            UIGroup.Show(panel);
            return panel;
        }

        public static void Hide(Type type)
        {
            if (!GlobalManager.Instance) return;
            if (PanelData.TryGetValue(type, out var panel))
            {
                UIGroup.Hide(panel);
            }
        }

        public static UIPanel Find(Type type)
        {
            return type != null ? PanelData.GetValueOrDefault(type) : null;
        }

        public static void Destroy(Type type)
        {
            if (!GlobalManager.Instance) return;
            if (PanelData.TryGetValue(type, out var panel))
            {
                UIGroup.Remove(panel);
                PanelData.Remove(type);
            }
        }

        public static void Hide(int index)
        {
            if (StackData.TryGetValue(index, out var result))
            {
                result.Hide();
            }
        }

        public static void Destroy()
        {
            var copies = new List<Type>(PanelData.Keys);
            foreach (var result in copies)
            {
                if (PanelData.TryGetValue(result, out var panel))
                {
                    if (panel.state != UIState.Stable)
                    {
                        UIGroup.Remove(panel);
                        PanelData.Remove(result);
                    }
                }
            }
        }

        public static RectTransform GetLayer(int layer)
        {
            if (!GlobalManager.Instance) return null;
            if (!LayerData.TryGetValue(layer, out var parent))
            {
                var format = "Pool - Canvas-{0}".Format(layer);
                parent = new GameObject(format).AddComponent<RectTransform>();
                parent.gameObject.layer = LayerMask.NameToLayer("UI");
                SetTransform(parent, Canvas.transform);
                parent.SetSiblingIndex(LayerData.Keys.Count(key => key < layer));
                LayerData.Add(layer, parent);
            }

            return parent;
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

        private static class UIGroup
        {
            internal static void Show(UIPanel panel)
            {
                if (panel.Group == 0)
                {
                    Modified(panel, true);
                    return;
                }

                if (!StackData.TryGetValue(panel.Group, out var result))
                {
                    result = new UIStack();
                    StackData.Add(panel.Group, result);
                }

                result.Push(panel);
            }

            internal static void Hide(UIPanel panel)
            {
                if (panel.Group == 0)
                {
                    Modified(panel, false);
                    return;
                }

                if (StackData.TryGetValue(panel.Group, out var result))
                {
                    result.Back(panel);
                }
            }

            internal static void Remove(UIPanel panel)
            {
                Modified(panel, false);
                Object.Destroy(panel.owner.gameObject);
            }

            public static void Modified(UIPanel panel, bool state)
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

        private sealed class UIStack
        {
            private UIPanel current;
            private UIPanel reverse;

            public void Push(UIPanel panel)
            {
                if (current == panel)
                {
                    return;
                }

                if (current != null && current.owner)
                {
                    UIGroup.Modified(current, false);
                    reverse = current;
                }

                current = panel;
                UIGroup.Modified(current, true);
            }

            public void Back(UIPanel panel)
            {
                if (current != panel)
                {
                    return;
                }

                if (reverse != null && reverse.owner)
                {
                    if (current != null && current.owner)
                    {
                        UIGroup.Modified(current, false);
                    }

                    (current, reverse) = (reverse, current);
                    UIGroup.Modified(current, true);
                }
            }

            public void Hide()
            {
                if (current != null && current.owner)
                {
                    UIGroup.Modified(current, false);
                }

                reverse = null;
                current = null;
            }
        }
    }
}