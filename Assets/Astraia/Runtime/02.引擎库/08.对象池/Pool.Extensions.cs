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

        public static void Inject(this Source source)
        {
            var fields = source.GetType().GetFields(Service.Find.Entity);
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
                    var component = source.transform.GetComponent(field.FieldType);
                    if (component != null)
                    {
                        field.SetValue(source, component);
                        continue;
                    }
                }

                var name = char.ToUpper(field.Name[0]) + field.Name.Substring(1);
                source.SetValue(field, name);
            }
        }

        private static void SetValue(this Source inject, FieldInfo field, string name)
        {
            var child = inject.transform.GetChild(name);
            if (child == null)
            {
                return;
            }

            var component = child.GetComponent(field.FieldType);
            if (component == null)
            {
                Debug.Log(Service.Text.Format("没有找到依赖注入的组件: {0} {1} != {2}", field.FieldType, field.FieldType.Name, name));
                return;
            }

            field.SetValue(inject, component);

            var method = inject.GetType().GetMethod(name, Service.Find.Entity);
            if (method == null)
            {
                return;
            }

            var injectType = Service.Find.Type("UnityEngine.UI.Button,UnityEngine.UI");
            if (component.TryGetComponent(injectType, out var button))
            {
                var property = injectType.GetProperty("onClick", Service.Find.Entity);
                if (property != null)
                {
                    inject.SetButton(name, (UnityEvent)property.GetValue(button));
                }

                return;
            }

            injectType = Service.Find.Type("UnityEngine.UI.Toggle,UnityEngine.UI");
            if (component.TryGetComponent(injectType, out var toggle))
            {
                var property = injectType.GetProperty("onValueChanged", Service.Find.Entity);
                if (property != null)
                {
                    inject.SetToggle(name, (UnityEvent<bool>)property.GetValue(toggle));
                }

                return;
            }

            injectType = Service.Find.Type("TMPro.TMP_InputField,Unity.TextMeshPro");
            if (component.TryGetComponent(injectType, out var inputField))
            {
                var property = injectType.GetProperty("onSubmit", Service.Find.Entity);
                if (property != null)
                {
                    inject.SetInputField(name, (UnityEvent<string>)property.GetValue(inputField));
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

        private static void SetButton(this Source inject, string name, UnityEvent button)
        {
            var panel = inject.owner.GetSource<UIPanel>();
            if (panel == null)
            {
                button.AddListener(() => inject.transform.SendMessage(name));
                return;
            }

            button.AddListener(() =>
            {
                if (panel.state != UIState.Freeze)
                {
                    inject.transform.SendMessage(name);
                }
            });
        }

        private static void SetToggle(this Source inject, string name, UnityEvent<bool> toggle)
        {
            var panel = inject.owner.GetSource<UIPanel>();
            if (panel == null)
            {
                toggle.AddListener(value => inject.transform.SendMessage(name, value));
                return;
            }

            toggle.AddListener(value =>
            {
                if (panel.state != UIState.Freeze)
                {
                    inject.transform.SendMessage(name, value);
                }
            });
        }

        private static void SetInputField(this Source inject, string name, UnityEvent<string> inputField)
        {
            var panel = inject.owner.GetSource<UIPanel>();
            if (panel == null)
            {
                inputField.AddListener(value => inject.transform.SendMessage(name, value));
                return;
            }

            inputField.AddListener(value =>
            {
                if (panel.state != UIState.Freeze)
                {
                    inject.transform.SendMessage(name, value);
                }
            });
        }
    }
}