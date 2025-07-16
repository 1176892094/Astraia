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
    public static partial class UIManager
    {
        private static async Task<UIPanel> Load(string path, Type type)
        {
            var obj = await AssetManager.Load<GameObject>(path);
            obj.SetActive(false);
            obj.name = path;

            var owner = obj.GetOrAddComponent<Entity>();
            var panel = (UIPanel)HeapManager.Dequeue<IAgent>(type);
            owner.transform.Inject(panel);
            owner.AddAgentInternal(panel, type);
            owner.OnFade += panel.groups.Clear;

            Surface(panel.transform, panel.layer);
            GlobalManager.panelData.Add(type, panel);
            return panel;
        }

        public static async void Show<T>(Action<T> action = null) where T : UIPanel
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.panelData.TryGetValue(typeof(T), out var panel))
            {
                panel = await Load(GlobalSetting.GetPanelPath(typeof(T).Name), typeof(T));
            }

            ShowInGroup(panel);
            action?.Invoke((T)panel);
        }

        public static void Hide<T>() where T : UIPanel
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.panelData.TryGetValue(typeof(T), out var panel))
            {
                panel.gameObject.SetActive(false);
            }
        }

        public static T Find<T>() where T : UIPanel
        {
            return (T)GlobalManager.panelData.GetValueOrDefault(typeof(T));
        }

        public static void Destroy<T>()
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.panelData.TryGetValue(typeof(T), out var panel))
            {
                panel.gameObject.SetActive(false);
                GlobalManager.panelData.Remove(typeof(T));
                Object.Destroy(panel.gameObject);
            }
        }

        public static async void Show(Type type, Action<UIPanel> action = null)
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.panelData.TryGetValue(type, out var panel))
            {
                panel = await Load(GlobalSetting.GetPanelPath(type.Name), type);
            }

            ShowInGroup(panel);
            action?.Invoke(panel);
        }

        public static void Hide(Type type)
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.panelData.TryGetValue(type, out var panel))
            {
                panel.gameObject.SetActive(false);
            }
        }

        public static UIPanel Find(Type type)
        {
            return GlobalManager.panelData.GetValueOrDefault(type);
        }

        public static void Destroy(Type type)
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.panelData.TryGetValue(type, out var panel))
            {
                panel.gameObject.SetActive(false);
                GlobalManager.panelData.Remove(type);
                Object.Destroy(panel.gameObject);
            }
        }

        public static void Clear()
        {
            var types = new List<Type>(GlobalManager.panelData.Keys);
            foreach (var type in types)
            {
                if (GlobalManager.panelData.TryGetValue(type, out var panel))
                {
                    if (panel.state == UIState.Stable)
                    {
                        panel.gameObject.SetActive(false);
                        GlobalManager.panelData.Remove(type);
                        Object.Destroy(panel.gameObject);
                    }
                }
            }
        }

        public static void Surface(Transform panel, UILayer layer)
        {
            if (!GlobalManager.Instance) return;
            var transform = panel.GetComponent<RectTransform>();
            transform.SetParent(GlobalManager.layerData[layer]);
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.offsetMin = Vector2.zero;
            transform.offsetMax = Vector2.zero;
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
        }
    }
}