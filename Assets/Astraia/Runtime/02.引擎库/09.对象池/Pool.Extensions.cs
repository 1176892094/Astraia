// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 22:04:28
// // # Recently: 2025-04-09 22:04:28
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Astraia
{
    public static partial class Extensions
    {
        public static T GetOrAddComponent<T>(this Transform transform) where T : Component
        {
            var component = transform.GetComponent<T>();
            if (component == null)
            {
                component = transform.gameObject.AddComponent<T>();
            }

            return component;
        }

        public static Component GetOrAddComponent(this Transform transform, Type type)
        {
            var component = transform.GetComponent(type);
            if (component == null)
            {
                component = transform.gameObject.AddComponent(type);
            }

            return component;
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        public static Component GetOrAddComponent(this GameObject gameObject, Type type)
        {
            var component = gameObject.GetComponent(type);
            if (component == null)
            {
                component = gameObject.AddComponent(type);
            }

            return component;
        }

        public static void Inject(this Component inject, object target)
        {
            var fields = target.GetType().GetFields(Service.Ref.Instance);
            foreach (var field in fields)
            {
                if (Attribute<InjectAttribute>.GetAttribute(field) == null)
                {
                    continue;
                }

                if (!field.FieldType.IsSubclassOf(typeof(Component)))
                {
                    continue;
                }

                if (!typeof(Transform).IsAssignableFrom(field.FieldType))
                {
                    var component = inject.GetComponent(field.FieldType);
                    if (component)
                    {
                        field.SetValue(target, component);
                        continue;
                    }
                }

                var upper = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
                var child = inject.transform.GetChild(upper);
                if (child)
                {
                    var component = child.GetComponent(field.FieldType);
                    if (!component)
                    {
                        Debug.Log("没有找到依赖注入的组件: {0} {1} != {2}".Format(field.FieldType, field.FieldType.Name, upper));
                        continue;
                    }

                    field.SetValue(target, component);

                    var method = target.GetType().GetMethod(upper, Service.Ref.Instance);
                    if (method == null)
                    {
                        continue;
                    }

                    var cacheType = Service.Ref.GetType("UnityEngine.UI.Button,UnityEngine.UI");
                    if (component.TryGetComponent(cacheType, out var button))
                    {
                        var panel = inject.GetComponent<Entity>()?.FindComponent<UIPanel>();
                        if (panel != null)
                        {
                            button.GetValue<UnityEvent>("onClick").AddListener(() =>
                            {
                                if (panel.state != UIState.Freeze)
                                {
                                    target.Invoke(upper);
                                }
                            });
                            return;
                        }

                        button.GetValue<UnityEvent>("onClick").AddListener(() => target.Invoke(upper));
                        continue;
                    }

                    cacheType = Service.Ref.GetType("UnityEngine.UI.Toggle,UnityEngine.UI");
                    if (component.TryGetComponent(cacheType, out var toggle))
                    {
                        var panel = inject.GetComponent<Entity>()?.FindComponent<UIPanel>();
                        if (panel != null)
                        {
                            toggle.GetValue<UnityEvent<bool>>("onValueChanged").AddListener(value =>
                            {
                                if (panel.state != UIState.Freeze)
                                {
                                    target.Invoke(upper, value);
                                }
                            });
                            return;
                        }

                        toggle.GetValue<UnityEvent<bool>>("onValueChanged").AddListener(value => target.Invoke(upper, value));
                        continue;
                    }

                    cacheType = Service.Ref.GetType("TMPro.TMP_InputField,Unity.TextMeshPro");
                    if (component.TryGetComponent(cacheType, out var inputField))
                    {
                        var panel = inject.GetComponent<Entity>()?.FindComponent<UIPanel>();
                        if (panel != null)
                        {
                            inputField.GetValue<UnityEvent<string>>("onSubmit").AddListener(value =>
                            {
                                if (panel.state != UIState.Freeze)
                                {
                                    target.Invoke(upper, value);
                                }
                            });
                            return;
                        }

                        inputField.GetValue<UnityEvent<string>>("onSubmit").AddListener(value => target.Invoke(upper, value));
                    }
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
}