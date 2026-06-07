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
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Astraia.Core
{
    using static GlobalManager;

    public static class UIManager
    {
        private static bool initialized;

        private static void Initialized()
        {
            if (initialized) return;
            initialized = true;
            canvas = new GameObject(nameof(UIManager)).AddComponent<Canvas>();
            canvas.sortingOrder = 20;
            canvas.planeDistance = 20;
            canvas.gameObject.layer = LayerMask.NameToLayer("UI");
            canvas.gameObject.AddComponent<GraphicRaycaster>();
            var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5F;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.referencePixelsPerUnit = 32;
            Object.DontDestroyOnLoad(canvas);
        }

        public static void SetCamera(Camera camera)
        {
            Initialized();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
        }

        private static UIPanel Load(string path, Type type)
        {
            if (type.GetAttribute(out UIPathAttribute item))
            {
                path = GlobalSetting.PREFAB.Format(item.asset);
            }

            Initialized();
            var asset = AssetManager.Load<GameObject>(path);
            asset.gameObject.name = type.Name;
            asset.gameObject.SetActive(false);
            var owner = asset.GetOrAddComponent<Entity>();
            var panel = (UIPanel)Activator.CreateInstance(type);
            owner.modules.Add(panel);
            if (type.GetAttribute(out UIMaskAttribute mask))
            {
                panel.layer = mask.layer;
                panel.group = mask.group;
            }

            owner.gameObject.SetActive(true);
            SetLayer(panel.owner.transform, panel.layer);
            panelData.Add(type, panel);

            return panel;
        }

        public static T Show<T>() where T : UIPanel
        {
            if (!Instance) return null;
            if (!panelData.TryGetValue(typeof(T), out var panel))
            {
                panel = Load(GlobalSetting.PREFAB.Format(typeof(T).Name), typeof(T));
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
                panel = Load(GlobalSetting.PREFAB.Format(type.Name), type);
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
                SetTransform(parent, canvas.transform);
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
            initialized = false;
        }
    }
}