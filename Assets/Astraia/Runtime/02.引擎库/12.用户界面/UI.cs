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
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia.Common
{
    using static GlobalManager;

    public static class PageManager
    {
        private static async Task<UIPanel> Load(string path, Type type)
        {
            var obj = await AssetManager.Load<GameObject>(path);
            obj.SetActive(false);
            obj.name = path;
            
            var owner = obj.GetOrAddComponent<Entity>();
            var panel = (UIPanel)HeapManager.Dequeue<IAgent>(type);
            var group = new HashSet<int>();
            panelData.Add(panel, group);
            
            owner.transform.Inject(panel);
            owner.AddAgent(panel, typeof(UIPanel));
            owner.OnFade += Enqueue;
            
            SetLayer(panel.transform, panel.layer);
            panelType.Add(type, panel);
            return panel;

            void Enqueue()
            {
                group.Clear();
                panelType.Remove(type);
                panelData.Remove(panel);
            }
        }

        public static async void Show<T>(Action<T> action = null) where T : UIPanel
        {
            if (!Instance) return;
            if (!panelType.TryGetValue(typeof(T), out var panel))
            {
                panel = await Load(GlobalSetting.GetPanelPath(typeof(T).Name), typeof(T));
            }

            UIGroup.Show(panel);
            action?.Invoke((T)panel);
        }

        public static void Hide<T>() where T : UIPanel
        {
            if (!Instance) return;
            if (panelType.TryGetValue(typeof(T), out var panel))
            {
                panel.gameObject.SetActive(false);
            }
        }

        public static T Find<T>() where T : UIPanel
        {
            return panelType.TryGetValue(typeof(T), out var panel) ? (T)panel : null;
        }

        public static void Destroy<T>()
        {
            if (!Instance) return;
            if (panelType.TryGetValue(typeof(T), out var panel))
            {
                panel.gameObject.SetActive(false);
                Object.Destroy(panel.gameObject);
            }
        }

        public static async void Show(Type type, Action<UIPanel> action = null)
        {
            if (!Instance) return;
            if (!panelType.TryGetValue(type, out var panel))
            {
                panel = await Load(GlobalSetting.GetPanelPath(type.Name), type);
            }

            UIGroup.Show(panel);
            action?.Invoke(panel);
        }

        public static void Hide(Type type)
        {
            if (!Instance) return;
            if (panelType.TryGetValue(type, out var panel))
            {
                panel.gameObject.SetActive(false);
            }
        }

        public static UIPanel Find(Type type)
        {
            return panelType.TryGetValue(type, out var panel) ? panel : null;
        }

        public static void Destroy(Type type)
        {
            if (!Instance) return;
            if (panelType.TryGetValue(type, out var panel))
            {
                panel.gameObject.SetActive(false);
                Object.Destroy(panel.gameObject);
            }
        }

        public static void Clear()
        {
            foreach (var panel in panelType.Values)
            {
                if (panel.state != UIState.Stable)
                {
                    panel.gameObject.SetActive(false);
                    Object.Destroy(panel.gameObject);
                }
            }
        }

        public static void SetLayer(Transform panel, UILayer layer)
        {
            if (!Instance) return;
            var transform = panel.GetComponent<RectTransform>();
            transform.SetParent(layerData[layer]);
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.offsetMin = Vector2.zero;
            transform.offsetMax = Vector2.zero;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
        }

        internal static void Dispose()
        {
            foreach (var panel in panelData.Values)
            {
                panel.Clear();
            }

            foreach (var group in groupData.Values)
            {
                group.Clear();
            }

            panelData.Clear();
            groupData.Clear();
            layerData.Clear();
            panelType.Clear();
        }
    }
}