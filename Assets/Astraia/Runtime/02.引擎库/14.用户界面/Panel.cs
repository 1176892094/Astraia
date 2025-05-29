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
            var data = await AssetManager.Load<GameObject>(path);
            var item = data.GetComponent(type);
            if (item == null)
            {
                item = data.AddComponent(type);
            }

            data.name = path;
            var panel = (UIPanel)item;
            GlobalManager.panelData.Add(type, panel);
            Surface(panel.transform, panel.layer);
            return panel;
        }

        public static async void Show<T>(Action<T> action = null) where T : UIPanel
        {
            if (!GlobalManager.Instance) return;
            var assetPath = GlobalSetting.GetPanelPath(typeof(T).Name);
            if (!GlobalManager.panelData.TryGetValue(typeof(T), out var panel))
            {
                panel = await Load(assetPath, typeof(T));
                ShowInGroup(panel);
                panel.Show();
            }
            else if (ShowInGroup(panel))
            {
                panel.Show();
            }

            action?.Invoke((T)panel);
        }

        public static void Hide<T>() where T : UIPanel
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.panelData.TryGetValue(typeof(T), out var panel))
            {
                if (panel.gameObject.activeInHierarchy)
                {
                    panel.Hide();
                }
            }
        }

        public static T Find<T>() where T : UIPanel
        {
            if (!GlobalManager.Instance) return null;
            if (GlobalManager.panelData.TryGetValue(typeof(T), out var panel))
            {
                return (T)panel;
            }

            return null;
        }

        public static void Destroy<T>()
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.panelData.TryGetValue(typeof(T), out var panel))
            {
                panel.Hide();
                GlobalManager.panelData.Remove(typeof(T));
                Object.Destroy(panel.gameObject);
            }
        }

        public static async void Show(Type type, Action<UIPanel> action = null)
        {
            if (!GlobalManager.Instance) return;
            var path = GlobalSetting.GetPanelPath(type.Name);
            if (!GlobalManager.panelData.TryGetValue(type, out var panel))
            {
                panel = await Load(path, type);
                ShowInGroup(panel);
                panel.Show();
            }
            else if (ShowInGroup(panel))
            {
                panel.Show();
            }

            action?.Invoke(panel);
        }

        public static void Hide(Type type)
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.panelData.TryGetValue(type, out var panel))
            {
                if (panel.gameObject.activeInHierarchy)
                {
                    panel.Hide();
                }
            }
        }

        public static UIPanel Find(Type type)
        {
            if (!GlobalManager.Instance) return null;
            if (GlobalManager.panelData.TryGetValue(type, out var panel))
            {
                return panel;
            }

            return null;
        }

        public static void Destroy(Type type)
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.panelData.TryGetValue(type, out var panel))
            {
                panel.Hide();
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
                    if (panel.state != UIState.Stable)
                    {
                        panel.Hide();
                        GlobalManager.panelData.Remove(type);
                        Object.Destroy(panel.gameObject);
                    }
                }
            }
        }

        public static void Surface(Transform panel, UILayer layer)
        {
            if (!GlobalManager.Instance) return;
            var index = (int)layer;
            if (!GlobalManager.layerData.TryGetValue(index, out var pool))
            {
                for (var i = UILayer.Lowest; i <= UILayer.Highest; i++)
                {
                    var name = Service.Text.Format("Pool - Canvas/{0}", i);
                    var item = new GameObject(name, typeof(RectTransform));
                    item.transform.SetParent(GlobalManager.Instance.canvas.transform);
                    item.gameObject.layer = LayerMask.NameToLayer("UI");
                    pool = item.GetComponent<RectTransform>();
                    pool.anchorMin = Vector2.zero;
                    pool.anchorMax = Vector2.one;
                    pool.offsetMin = Vector2.zero;
                    pool.offsetMax = Vector2.zero;
                    pool.localScale = Vector3.one;
                    pool.localPosition = Vector3.zero;
                    GlobalManager.layerData.Add((int)i, pool);
                }

                pool = GlobalManager.layerData[index];
            }

            var rect = (RectTransform)panel;
            rect.SetParent(pool);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
        }
    }
}