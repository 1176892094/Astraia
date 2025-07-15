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
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Astraia
{
    public static partial class Extensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        public static void Inject(this Transform inject, object target)
        {
            var fields = target.GetType().GetFields(Service.Find.Entity);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<InjectAttribute>(true) == null)
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
                    if (component != null)
                    {
                        field.SetValue(target, component);
                        continue;
                    }
                }

                var name = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
                var child = inject.GetChild(name);
                if (child != null)
                {
                    var component = child.GetComponent(field.FieldType);
                    if (component == null)
                    {
                        Debug.Log(Service.Text.Format("没有找到依赖注入的组件: {0} {1} != {2}", field.FieldType, field.FieldType.Name, name));
                        continue;
                    }

                    field.SetValue(target, component);

                    var method = target.GetType().GetMethod(name, Service.Find.Entity);
                    if (method == null)
                    {
                        continue;
                    }

                    var cacheType = Service.Find.Type("UnityEngine.UI.Button,UnityEngine.UI");
                    if (component.TryGetComponent(cacheType, out var button))
                    {
                        var property = cacheType.GetProperty("onClick", Service.Find.Entity);
                        if (property != null)
                        {
                            var func = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, method);
                            var panel = inject.GetComponent<Entity>()?.GetAgent<UIPanel>();
                            if (panel == null)
                            {
                                ((UnityEvent)property.GetValue(button)).AddListener(func);
                            }
                            else
                            {
                                ((UnityEvent)property.GetValue(button)).AddListener(() =>
                                {
                                    if (panel.state != UIState.Freeze)
                                    {
                                        func.Invoke();
                                    }
                                });
                            }
                        }

                        continue;
                    }

                    cacheType = Service.Find.Type("UnityEngine.UI.Toggle,UnityEngine.UI");
                    if (component.TryGetComponent(cacheType, out var toggle))
                    {
                        var property = cacheType.GetProperty("onValueChanged", Service.Find.Entity);
                        if (property != null)
                        {
                            var func = (UnityAction<bool>)Delegate.CreateDelegate(typeof(UnityAction<bool>), target, method);
                            var panel = inject.GetComponent<Entity>()?.GetAgent<UIPanel>();
                            if (panel == null)
                            {
                                ((UnityEvent<bool>)property.GetValue(toggle)).AddListener(func);
                            }
                            else
                            {
                                ((UnityEvent<bool>)property.GetValue(toggle)).AddListener(value =>
                                {
                                    if (panel.state != UIState.Freeze)
                                    {
                                        func.Invoke(value);
                                    }
                                });
                            }
                        }

                        continue;
                    }

                    cacheType = Service.Find.Type("TMPro.TMP_InputField,Unity.TextMeshPro");
                    if (component.TryGetComponent(cacheType, out var inputField))
                    {
                        var property = cacheType.GetProperty("onSubmit", Service.Find.Entity);
                        if (property != null)
                        {
                            var func = (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>), target, method);
                            var panel = inject.GetComponent<Entity>()?.GetAgent<UIPanel>();
                            if (panel == null)
                            {
                                ((UnityEvent<string>)property.GetValue(inputField)).AddListener(func);
                            }
                            else
                            {
                                ((UnityEvent<string>)property.GetValue(inputField)).AddListener(value =>
                                {
                                    if (panel.state != UIState.Freeze)
                                    {
                                        func.Invoke(value);
                                    }
                                });
                            }
                        }
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
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}