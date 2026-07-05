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
        private Dictionary<int, UIStack> stackData = new Dictionary<int, UIStack>();
        private Dictionary<int, RectTransform> layerData = new Dictionary<int, RectTransform>();
        private Dictionary<Type, UIPanel> panelData = new Dictionary<Type, UIPanel>();
        [SerializeField] private Canvas canvas;

        public static Canvas Canvas => Instance.canvas;

        public override void Enqueue()
        {
            Instance = null;
            stackData.Clear();
            layerData.Clear();
            panelData.Clear();
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
            panelData.Add(value, panel);
            return panel;
        }

        private T ShowInternal<T>() where T : UIPanel
        {
            if (!panelData.TryGetValue(typeof(T), out var panel))
            {
                panel = Load(typeof(T).Name, typeof(T));
            }

            ShowInGroup(panel);
            return (T)panel;
        }

        private T FindInternal<T>() where T : UIPanel
        {
            return (T)panelData.GetValueOrDefault(typeof(T));
        }

        private void HideInternal<T>() where T : UIPanel
        {
            if (panelData.TryGetValue(typeof(T), out var panel))
            {
                HideInGroup(panel);
            }
        }

        private void DestroyInternal<T>()
        {
            if (panelData.TryGetValue(typeof(T), out var panel))
            {
                DestroyInGroup(panel);
                panelData.Remove(typeof(T));
            }
        }

        private UIPanel ShowInternal(Type value)
        {
            if (!panelData.TryGetValue(value, out var panel))
            {
                panel = Load(value.Name, value);
            }

            ShowInGroup(panel);
            return panel;
        }

        public UIPanel FindInternal(Type value)
        {
            return panelData.GetValueOrDefault(value);
        }

        public void HideInternal(Type value)
        {
            if (panelData.TryGetValue(value, out var panel))
            {
                HideInGroup(panel);
            }
        }

        public void DestroyInternal(Type value)
        {
            if (panelData.TryGetValue(value, out var panel))
            {
                DestroyInGroup(panel);
                panelData.Remove(value);
            }
        }

        public void HideInternal(int index)
        {
            if (stackData.TryGetValue(index, out var panel))
            {
                panel.Hide();
            }
        }

        public void DestroyInternal()
        {
            foreach (var result in panelData.Keys.ToList())
            {
                if (panelData.TryGetValue(result, out var panel))
                {
                    if (panel.state != UIState.Stable)
                    {
                        DestroyInGroup(panel);
                        panelData.Remove(result);
                    }
                }
            }
        }

        public RectTransform GetLayerInternal(int value)
        {
            if (!layerData.TryGetValue(value, out var parent))
            {
                var rename = "Pool - Canvas-{0}".Format(value);
                parent = new GameObject(rename).AddComponent<RectTransform>();
                parent.gameObject.layer = LayerMask.NameToLayer("UI");
                SetTransform(parent, canvas.transform);
                parent.SetSiblingIndex(layerData.Keys.Count(key => key < value));
                layerData.Add(value, parent);
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

        private void ShowInGroup(UIPanel panel)
        {
            if (panel.Group == 0)
            {
                Modified(panel, true);
                return;
            }

            if (!stackData.TryGetValue(panel.Group, out var result))
            {
                result = new UIStack();
                stackData.Add(panel.Group, result);
            }

            result.Push(panel);
        }

        private void HideInGroup(UIPanel panel)
        {
            if (panel.Group == 0)
            {
                Modified(panel, false);
                return;
            }

            if (stackData.TryGetValue(panel.Group, out var result))
            {
                result.Back(panel);
            }
        }

        private void DestroyInGroup(UIPanel panel)
        {
            Modified(panel, false);
            Object.Destroy(panel.owner.gameObject);
        }

        private static void Modified(UIPanel panel, bool state)
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

                if (reverse != null && reverse.owner)
                {
                    if (current != null && current.owner)
                    {
                        Modified(current, false);
                    }

                    (current, reverse) = (reverse, current);
                    Modified(current, true);
                }
            }

            public void Hide()
            {
                if (current != null && current.owner)
                {
                    Modified(current, false);
                }

                reverse = null;
                current = null;
            }
        }
    }
}