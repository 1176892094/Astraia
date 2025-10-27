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
using System.Collections.Generic;
using Astraia.Common;
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
    }

    public static partial class Extensions
    {
        public static TaskNode GetNode(this string root, Func<TaskNode.Node, TaskNode> func)
        {
            return LoadNode(GetRoot(root), func);
        }

        public static void Enqueue(this TaskNode root)
        {
            if (root == null) return;
            foreach (var child in root.nodes)
            {
                Enqueue(child);
            }

            root.nodes = Array.Empty<TaskNode>();
            HeapManager.Enqueue(root, root.GetType());
        }

        private static TaskNode LoadNode(TaskNode.Node root, Func<TaskNode.Node, TaskNode> func)
        {
            if (root.name == null)
            {
                return null;
            }

            var result = func.Invoke(root);
            if (result == null)
            {
                return null;
            }

            result.nodes = new TaskNode[root.items.Count];
            for (int i = 0; i < root.items.Count; i++)
            {
                var node = LoadNode(root.items[i], func);
                if (node != null)
                {
                    result.nodes[i] = node;
                }
            }

            return result;
        }

        private static TaskNode.Node GetRoot(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return default;
            }

            var index = reason.IndexOf('(');
            if (index < 0)
            {
                return new TaskNode.Node(reason);
            }

            var result = reason.Substring(0, index).Trim();
            var braced = Parse(reason, index);

            var node = new TaskNode.Node(result);
            foreach (var child in Split(braced))
            {
                node.items.Add(GetRoot(child));
            }

            return node;
        }

        private static string Parse(string reason, int index)
        {
            var depth = 0;
            var count = index;
            while (count < reason.Length)
            {
                if (reason[count] == '(')
                {
                    depth++;
                }
                else if (reason[count] == ')')
                {
                    depth--;
                }

                if (depth == 0)
                {
                    break;
                }

                count++;
            }

            return reason.Substring(index + 1, count - index - 1);
        }

        private static List<string> Split(string reason)
        {
            var result = new List<string>();
            var depth = 0;
            var index = 0;

            for (var i = 0; i < reason.Length; i++)
            {
                if (reason[i] == '(')
                {
                    depth++;
                }
                else if (reason[i] == ')')
                {
                    depth--;
                }
                else if ((reason[i] == ',' || reason[i] == '，') && depth == 0)
                {
                    result.Add(reason.Substring(index, i - index).Trim());
                    index = i + 1;
                }
            }

            result.Add(reason.Substring(index).Trim());
            return result;
        }

        public static string Color(this string result, string format)
        {
            return format switch
            {
                "R" => "<color=#FF0000>{0}</color>".Format(result), // 红
                "G" => "<color=#00FF00>{0}</color>".Format(result), // 绿
                "B" => "<color=#0000FF>{0}</color>".Format(result), // 蓝
                "Y" => "<color=#FFFF00>{0}</color>".Format(result), // 黄
                "O" => "<color=#FFAA00>{0}</color>".Format(result), // 橙
                "S" => "<color=#00FFFF>{0}</color>".Format(result), // 青
                "P" => "<color=#FFAAFF>{0}</color>".Format(result), // 紫
                "W" => "<color=#FFFFFF>{0}</color>".Format(result), // 白
                _ => "<color=#{0}>{1}</color>".Format(format, result)
            };
        }
    }

    public static partial class Extensions
    {
        public static void Inject(this Component inject, object target)
        {
            var fields = target.GetType().GetFields(Service.Ref.Instance);
            foreach (var field in fields)
            {
                if (Service.Ref<InjectAttribute>.GetAttribute(field) == null)
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
                        Service.Log.Info("没有找到依赖注入的组件: {0} {1} != {2}".Format(field.FieldType, field.FieldType.Name, upper));
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

    public static partial class Extensions
    {
        public static Timer Wait(this Component current, float duration = 0)
        {
            return Timer.Create(current, duration);
        }

        public static Tween Play(this Component current, float duration)
        {
            return Tween.Create(current, duration);
        }

        public static Tween DOMoveX(this Transform transform, float endValue, float duration)
        {
            var position = transform.position;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var positionX = Mathf.Lerp(position.x, endValue, progress);
                transform.position = new Vector3(positionX, position.y, position.z);
            });
        }

        public static Tween DOMoveY(this Transform transform, float endValue, float duration)
        {
            var position = transform.position;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var positionY = Mathf.Lerp(position.y, endValue, progress);
                transform.position = new Vector3(position.x, positionY, position.z);
            });
        }

        public static Tween DOMoveZ(this Transform transform, float endValue, float duration)
        {
            var position = transform.position;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var positionZ = Mathf.Lerp(position.z, endValue, progress);
                transform.position = new Vector3(position.x, position.y, positionZ);
            });
        }

        public static Tween DOMove(this Transform transform, Vector3 endValue, float duration)
        {
            var position = transform.position;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var positionX = Mathf.Lerp(position.x, endValue.x, progress);
                var positionY = Mathf.Lerp(position.y, endValue.y, progress);
                var positionZ = Mathf.Lerp(position.z, endValue.z, progress);
                transform.position = new Vector3(positionX, positionY, positionZ);
            });
        }

        public static Tween DORotateX(this Transform transform, float endValue, float duration)
        {
            var rotation = transform.rotation.eulerAngles;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var rotationX = Mathf.LerpAngle(rotation.x, endValue, progress);
                transform.rotation = Quaternion.Euler(rotationX, rotation.y, rotation.z);
            });
        }

        public static Tween DORotateY(this Transform transform, float endValue, float duration)
        {
            var rotation = transform.rotation.eulerAngles;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var rotationY = Mathf.LerpAngle(rotation.y, endValue, progress);
                transform.rotation = Quaternion.Euler(rotation.x, rotationY, rotation.z);
            });
        }

        public static Tween DORotateZ(this Transform transform, float endValue, float duration)
        {
            var rotation = transform.rotation.eulerAngles;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var rotationZ = Mathf.LerpAngle(rotation.z, endValue, progress);
                transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotationZ);
            });
        }

        public static Tween DORotate(this Transform transform, Vector3 endValue, float duration)
        {
            var rotation = transform.rotation.eulerAngles;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var rotationX = Mathf.Lerp(rotation.x, endValue.x, progress);
                var rotationY = Mathf.Lerp(rotation.y, endValue.y, progress);
                var rotationZ = Mathf.Lerp(rotation.z, endValue.z, progress);
                transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
            });
        }

        public static Tween DOScaleX(this Transform transform, float endValue, float duration)
        {
            var localScale = transform.localScale;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var localScaleX = Mathf.Lerp(localScale.x, endValue, progress);
                transform.localScale = new Vector3(localScaleX, localScale.y, localScale.z);
            });
        }

        public static Tween DOScaleY(this Transform transform, float endValue, float duration)
        {
            var localScale = transform.localScale;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var localScaleY = Mathf.Lerp(localScale.y, endValue, progress);
                transform.localScale = new Vector3(localScale.x, localScaleY, localScale.z);
            });
        }

        public static Tween DOScaleZ(this Transform transform, float endValue, float duration)
        {
            var localScale = transform.localScale;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var localScaleZ = Mathf.Lerp(localScale.z, endValue, progress);
                transform.localScale = new Vector3(localScale.x, localScale.y, localScaleZ);
            });
        }

        public static Tween DOScale(this Transform transform, Vector3 endValue, float duration)
        {
            var localScale = transform.localScale;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var localScaleX = Mathf.Lerp(localScale.x, endValue.x, progress);
                var localScaleY = Mathf.Lerp(localScale.y, endValue.y, progress);
                var localScaleZ = Mathf.Lerp(localScale.z, endValue.z, progress);
                transform.localScale = new Vector3(localScaleX, localScaleY, localScaleZ);
            });
        }
    }
}