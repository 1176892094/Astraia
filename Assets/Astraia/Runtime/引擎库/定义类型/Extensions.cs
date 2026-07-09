// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-04-09 22:04:28
// # Recently: 2025-04-09 22:04:28
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Astraia.Core
{
    public static partial class Extensions
    {
        public static T Inject<T>(this Component owner, object obj, string name) where T : Component
        {
            var child = owner.transform.GetChild(name);
            if (child)
            {
                var component = child.GetComponent<T>();
                var method = obj.GetType().GetMethod(name, Search.Instance);
                if (method != null)
                {
                    if (Button(obj, component, method) || Toggle(obj, component, method) || Slider(obj, component, method))
                    {
                        return component;
                    }

                    InputField(obj, component, method);
                }

                return component;
            }

            return owner.GetComponent<T>();
        }

        private static bool Button<T>(object obj, T component, MethodInfo method) where T : Component
        {
            if (component.TryGetComponent(out Button button))
            {
                var action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), obj, method);
                if (obj is UIPanel panel)
                {
                    button.onClick.AddListener(() =>
                    {
                        if (panel.state != UIState.Freeze)
                        {
                            action.Invoke();
                        }
                    });
                }
                else
                {
                    button.onClick.AddListener(action);
                }

                return true;
            }

            return false;
        }

        private static bool Toggle<T>(object obj, T component, MethodInfo method) where T : Component
        {
            if (component.TryGetComponent(out Toggle toggle))
            {
                var action = (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>), obj, method);
                if (obj is UIPanel panel)
                {
                    toggle.onValueChanged.AddListener(value =>
                    {
                        if (panel.state != UIState.Freeze)
                        {
                            action.Invoke(value);
                        }
                    });
                }
                else
                {
                    toggle.onValueChanged.AddListener(action);
                }

                return true;
            }

            return false;
        }

        private static bool Slider<T>(object obj, T component, MethodInfo method) where T : Component
        {
            if (component.TryGetComponent(out Slider slider))
            {
                var action = (UnityAction<float>)Delegate.CreateDelegate(typeof(UnityAction<float>), obj, method);
                if (obj is UIPanel panel)
                {
                    slider.onValueChanged.AddListener(value =>
                    {
                        if (panel.state != UIState.Freeze)
                        {
                            action.Invoke(value);
                        }
                    });
                }
                else
                {
                    slider.onValueChanged.AddListener(action);
                }

                return true;
            }

            return false;
        }

        private static void InputField<T>(object obj, T component, MethodInfo method) where T : Component
        {
            var cacheType = Search.GetType("TMPro.TMP_InputField,Unity.TextMeshPro");
            if (component.TryGetComponent(cacheType, out var inputField))
            {
                var action = (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>), obj, method);
                if (obj is UIPanel panel)
                {
                    inputField.GetValue<UnityEvent<string>>("onSubmit").AddListener(value =>
                    {
                        if (panel.state != UIState.Freeze)
                        {
                            action.Invoke(value);
                        }
                    });
                }
                else
                {
                    inputField.GetValue<UnityEvent<string>>("onSubmit").AddListener(action);
                }
            }
        }

        private static Transform GetChild(this Transform parent, string name)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }

                var result = child.GetChild(name);
                if (result)
                {
                    return result;
                }
            }

            return null;
        }
    }

    public static partial class Extensions
    {
        private static readonly Dictionary<char, string> colors = new Dictionary<char, string>();

        static Extensions()
        {
            colors['R'] = "#FF0000";
            colors['G'] = "#00FF00";
            colors['B'] = "#AAAAFF";
            colors['Y'] = "#FFFF00";
            colors['O'] = "#FFAA00";
            colors['S'] = "#00FFFF";
            colors['P'] = "#FFAAFF";
            colors['W'] = "#FFFFFF";
        }

        public static string Color(this string result, string format)
        {
            return "<color={0}>{1}</color>".Format(colors.GetValueOrDefault(format[0], format), result);
        }

        public static T GetOrAddComponent<T>(this Component self) where T : Component
        {
            var component = self.GetComponent<T>();
            return component ? component : self.gameObject.AddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            var component = self.GetComponent<T>();
            return component ? component : self.AddComponent<T>();
        }

        private readonly struct AsyncAdaptor : IAsync
        {
            private readonly Component owner;
            public AsyncAdaptor(Component owner) => this.owner = owner;
            public bool isActive => owner && owner.gameObject && owner.gameObject.activeInHierarchy;
        }

        public static Timer Wait(this Component current, float duration = 0)
        {
            return Timer.Create(new AsyncAdaptor(current), duration);
        }

        public static Tween Play(this Component current, float duration)
        {
            return Tween.Create(new AsyncAdaptor(current), duration);
        }

        public static Tween DOMoveX(this Component component, float endValue, float duration)
        {
            var position = component.transform.position;
            return component.transform.Play(duration).OnUpdate(progress =>
            {
                var positionX = Mathf.Lerp(position.x, endValue, progress);
                component.transform.position = new Vector3(positionX, position.y, position.z);
            });
        }

        public static Tween DOMoveY(this Component component, float endValue, float duration)
        {
            var position = component.transform.position;
            return component.Play(duration).OnUpdate(progress =>
            {
                var positionY = Mathf.Lerp(position.y, endValue, progress);
                component.transform.position = new Vector3(position.x, positionY, position.z);
            });
        }

        public static Tween DOMoveZ(this Component component, float endValue, float duration)
        {
            var position = component.transform.position;
            return component.Play(duration).OnUpdate(progress =>
            {
                var positionZ = Mathf.Lerp(position.z, endValue, progress);
                component.transform.position = new Vector3(position.x, position.y, positionZ);
            });
        }

        public static Tween DOMove(this Component component, Vector3 endValue, float duration)
        {
            var position = component.transform.position;
            return component.Play(duration).OnUpdate(progress =>
            {
                var positionX = Mathf.Lerp(position.x, endValue.x, progress);
                var positionY = Mathf.Lerp(position.y, endValue.y, progress);
                var positionZ = Mathf.Lerp(position.z, endValue.z, progress);
                component.transform.position = new Vector3(positionX, positionY, positionZ);
            });
        }

        public static Tween DORotateX(this Component component, float endValue, float duration)
        {
            var rotation = component.transform.rotation.eulerAngles;
            return component.Play(duration).OnUpdate(progress =>
            {
                var rotationX = Mathf.LerpAngle(rotation.x, endValue, progress);
                component.transform.rotation = Quaternion.Euler(rotationX, rotation.y, rotation.z);
            });
        }

        public static Tween DORotateY(this Component component, float endValue, float duration)
        {
            var rotation = component.transform.rotation.eulerAngles;
            return component.Play(duration).OnUpdate(progress =>
            {
                var rotationY = Mathf.LerpAngle(rotation.y, endValue, progress);
                component.transform.rotation = Quaternion.Euler(rotation.x, rotationY, rotation.z);
            });
        }

        public static Tween DORotateZ(this Component component, float endValue, float duration)
        {
            var rotation = component.transform.rotation.eulerAngles;
            return component.Play(duration).OnUpdate(progress =>
            {
                var rotationZ = Mathf.LerpAngle(rotation.z, endValue, progress);
                component.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotationZ);
            });
        }

        public static Tween DORotate(this Component component, Vector3 endValue, float duration)
        {
            var rotation = component.transform.rotation.eulerAngles;
            return component.Play(duration).OnUpdate(progress =>
            {
                var rotationX = Mathf.Lerp(rotation.x, endValue.x, progress);
                var rotationY = Mathf.Lerp(rotation.y, endValue.y, progress);
                var rotationZ = Mathf.Lerp(rotation.z, endValue.z, progress);
                component.transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
            });
        }

        public static Tween DOScaleX(this Component component, float endValue, float duration)
        {
            var localScale = component.transform.localScale;
            return component.Play(duration).OnUpdate(progress =>
            {
                var localScaleX = Mathf.Lerp(localScale.x, endValue, progress);
                component.transform.localScale = new Vector3(localScaleX, localScale.y, localScale.z);
            });
        }

        public static Tween DOScaleY(this Component component, float endValue, float duration)
        {
            var localScale = component.transform.localScale;
            return component.Play(duration).OnUpdate(progress =>
            {
                var localScaleY = Mathf.Lerp(localScale.y, endValue, progress);
                component.transform.localScale = new Vector3(localScale.x, localScaleY, localScale.z);
            });
        }

        public static Tween DOScaleZ(this Component component, float endValue, float duration)
        {
            var localScale = component.transform.localScale;
            return component.Play(duration).OnUpdate(progress =>
            {
                var localScaleZ = Mathf.Lerp(localScale.z, endValue, progress);
                component.transform.localScale = new Vector3(localScale.x, localScale.y, localScaleZ);
            });
        }

        public static Tween DOScale(this Component component, Vector3 endValue, float duration)
        {
            var localScale = component.transform.localScale;
            return component.Play(duration).OnUpdate(progress =>
            {
                var localScaleX = Mathf.Lerp(localScale.x, endValue.x, progress);
                var localScaleY = Mathf.Lerp(localScale.y, endValue.y, progress);
                var localScaleZ = Mathf.Lerp(localScale.z, endValue.z, progress);
                component.transform.localScale = new Vector3(localScaleX, localScaleY, localScaleZ);
            });
        }
    }
}