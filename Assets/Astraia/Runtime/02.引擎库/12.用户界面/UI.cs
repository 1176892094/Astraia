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

namespace Astraia.Common
{
    using static GlobalManager;

    internal interface IPanel
    {
        void Update();

        void Listen();

        void Remove();
    }

    public static class UIManager
    {
        private static UIPanel Load(string path, Type type)
        {
            var asset = AssetManager.Load<GameObject>(path);
            asset.SetActive(false);
            asset.name = path;

            var panel = (UIPanel)HeapManager.Dequeue<IModule>(type);
            var owner = asset.GetOrAddComponent<Entity>();
            owner.AddComponent(panel, typeof(UIPanel));
            owner.OnShow += ((IPanel)panel).Listen;
            owner.OnHide += ((IPanel)panel).Remove;

            SetLayer(panel.transform, panel.layerMask);
            panelData.Add(type, panel);
            return panel;
        }

        public static T Show<T>() where T : UIPanel
        {
            if (!Instance) return null;
            if (!panelData.TryGetValue(typeof(T), out var panel))
            {
                panel = Load(GlobalSetting.GetPanelPath(typeof(T).Name), typeof(T));
            }

            UIGroup.Show(panel);
            return (T)panel;
        }

        public static void Hide<T>() where T : UIPanel
        {
            if (!Instance) return;
            if (panelData.TryGetValue(typeof(T), out var panel))
            {
                panel.gameObject.SetActive(false);
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
                panel.gameObject.SetActive(false);
                Object.Destroy(panel.gameObject);
                panelData.Remove(typeof(T));
            }
        }

        public static UIPanel Show(Type type)
        {
            if (!Instance) return null;
            if (!panelData.TryGetValue(type, out var panel))
            {
                panel = Load(GlobalSetting.GetPanelPath(type.Name), type);
            }

            UIGroup.Show(panel);
            return panel;
        }

        public static void Hide(Type type)
        {
            if (!Instance) return;
            if (panelData.TryGetValue(type, out var panel))
            {
                panel.gameObject.SetActive(false);
            }
        }

        public static UIPanel Find(Type type)
        {
            return panelData.TryGetValue(type, out var panel) ? panel : null;
        }

        public static void Destroy(Type type)
        {
            if (!Instance) return;
            if (panelData.TryGetValue(type, out var panel))
            {
                panel.gameObject.SetActive(false);
                Object.Destroy(panel.gameObject);
                panelData.Remove(type);
            }
        }

        public static void Destroy()
        {
            var copies = new List<Type>(panelData.Keys);
            foreach (var panel in copies)
            {
                var result = panelData[panel];
                if (result.state != UIState.Stable)
                {
                    result.gameObject.SetActive(false);
                    Object.Destroy(result.gameObject);
                    panelData.Remove(panel);
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

        private static void SetTransform(RectTransform transform, Transform parent)
        {
            transform.SetParent(parent);
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.offsetMin = Vector2.zero;
            transform.offsetMax = Vector2.zero;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
        }

        internal static void Dispose()
        {
            foreach (var group in groupData.Values)
            {
                group.Clear();
            }

            groupData.Clear();
            layerData.Clear();
            panelData.Clear();
        }
    }
}