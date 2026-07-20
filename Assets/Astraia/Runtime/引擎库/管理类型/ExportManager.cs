using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Astraia
{
    public static class ExportManager
    {
        public static T Export<T>(Component owner, object obj, string name) where T : Component
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
                var result = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), obj, method);
                if (obj is UIPanel panel)
                {
                    button.onClick.AddListener(() =>
                    {
                        if (panel.state != UIState.Freeze)
                        {
                            result.Invoke();
                        }
                    });
                }
                else
                {
                    button.onClick.AddListener(result);
                }

                return true;
            }

            return false;
        }

        private static bool Toggle<T>(object obj, T component, MethodInfo method) where T : Component
        {
            if (component.TryGetComponent(out Toggle toggle))
            {
                var result = (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>), obj, method);
                if (obj is UIPanel panel)
                {
                    toggle.onValueChanged.AddListener(value =>
                    {
                        if (panel.state != UIState.Freeze)
                        {
                            result.Invoke(value);
                        }
                    });
                }
                else
                {
                    toggle.onValueChanged.AddListener(result);
                }

                return true;
            }

            return false;
        }

        private static bool Slider<T>(object obj, T component, MethodInfo method) where T : Component
        {
            if (component.TryGetComponent(out Slider slider))
            {
                var result = (UnityAction<float>)Delegate.CreateDelegate(typeof(UnityAction<float>), obj, method);
                if (obj is UIPanel panel)
                {
                    slider.onValueChanged.AddListener(value =>
                    {
                        if (panel.state != UIState.Freeze)
                        {
                            result.Invoke(value);
                        }
                    });
                }
                else
                {
                    slider.onValueChanged.AddListener(result);
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
                var result = (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>), obj, method);
                var reason = (UnityEvent<string>)cacheType.GetProperty("onSubmit", Search.Instance)!.GetValue(inputField);
                if (obj is UIPanel panel)
                {
                    reason.AddListener(value =>
                    {
                        if (panel.state != UIState.Freeze)
                        {
                            result.Invoke(value);
                        }
                    });
                }
                else
                {
                    reason.AddListener(result);
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

        internal static bool HasAttribute<T>(this MemberInfo member) where T : Attribute
        {
            var attribute = member.GetCustomAttribute<T>(true);
            return attribute != null;
        }

        internal static bool GetAttribute<T>(this MemberInfo member, out T attribute) where T : Attribute
        {
            attribute = member.GetCustomAttribute<T>(true);
            return attribute != null;
        }
    }
}