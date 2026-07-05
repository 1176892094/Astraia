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

        [SerializeField]
        private Canvas canvas;

        public static Canvas Canvas => Instance.canvas;

        public override void Enqueue()
        {
            Instance = null;
            StackData.Clear();
            LayerData.Clear();
            PanelData.Clear();
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

        public static void Destroy<T>() where T : UIPanel
        {
            Instance?.DestroyInternal<T>();
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

        public static void Destroy(Type value)
        {
            Instance?.DestroyInternal(value);
        }

        public static void Hide(int value)
        {
            Instance?.HideInternal(value);
        }

        public static void Destroy()
        {
            Instance?.DestroyInternal();
        }

        public static RectTransform GetLayer(int value)
        {
            return Instance?.GetLayerInternal(value);
        }

        private UIPanel Load(string name, Type value)
        {
            var item = GlobalSetting.PREFAB.Format(name);
            if (value.GetAttribute(out UIPathAttribute path))
            {
                item = GlobalSetting.PREFAB.Format(path.asset);
            }

            var asset = AssetManager.Load<GameObject>(item);
            asset.gameObject.SetActive(false);
            asset.gameObject.name = name;

            var agent = asset.AddComponent<Entity>();
            var panel = agent.AddComponent<UIPanel>(value);
            var child = agent.GetComponent<RectTransform>();

            if (value.GetAttribute(out UIMaskAttribute mask))
            {
                panel.Layer = mask.layer;
                panel.Group = mask.group;
            }

            SetTransform(child, GetLayer(panel.Layer));
            PanelData.Add(value, panel);
            return panel;
        }

        private T ShowInternal<T>() where T : UIPanel
        {
            if (!PanelData.TryGetValue(typeof(T), out var panel))
            {
                panel = Load(typeof(T).Name, typeof(T));
            }

            UIGroup.Show(panel);
            return (T)panel;
        }

        private T FindInternal<T>() where T : UIPanel
        {
            return (T)PanelData.GetValueOrDefault(typeof(T));
        }

        private void HideInternal<T>() where T : UIPanel
        {
            if (PanelData.TryGetValue(typeof(T), out var panel))
            {
                UIGroup.Hide(panel);
            }
        }

        private void DestroyInternal<T>()
        {
            if (PanelData.TryGetValue(typeof(T), out var panel))
            {
                UIGroup.Remove(panel);
                PanelData.Remove(typeof(T));
            }
        }

        private UIPanel ShowInternal(Type value)
        {
            if (!PanelData.TryGetValue(value, out var panel))
            {
                panel = Load(value.Name, value);
            }

            UIGroup.Show(panel);
            return panel;
        }

        public UIPanel FindInternal(Type value)
        {
            return PanelData.GetValueOrDefault(value);
        }

        public void HideInternal(Type value)
        {
            if (PanelData.TryGetValue(value, out var panel))
            {
                UIGroup.Hide(panel);
            }
        }

        public void DestroyInternal(Type value)
        {
            if (PanelData.TryGetValue(value, out var panel))
            {
                UIGroup.Remove(panel);
                PanelData.Remove(value);
            }
        }

        public void HideInternal(int index)
        {
            if (StackData.TryGetValue(index, out var panel))
            {
                panel.Hide();
            }
        }

        public void DestroyInternal()
        {
            foreach (var result in PanelData.Keys.ToList())
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

        public RectTransform GetLayerInternal(int value)
        {
            if (!LayerData.TryGetValue(value, out var parent))
            {
                var rename = "Pool - Canvas-{0}".Format(value);
                parent = new GameObject(rename).AddComponent<RectTransform>();
                parent.gameObject.layer = LayerMask.NameToLayer("UI");
                SetTransform(parent, canvas.transform);
                parent.SetSiblingIndex(LayerData.Keys.Count(key => key < value));
                LayerData.Add(value, parent);
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
            public static void Show(UIPanel panel)
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

            public static void Hide(UIPanel panel)
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

            public static void Remove(UIPanel panel)
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