using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Astraia.Core
{
    public static class InjectManager
    {
        public static T Inject<T>(this Component self, string name) where T : Component
        {
            var child = self.transform.GetChild(name);
            if (child)
            {
                var component = child.GetComponent<T>();
                if (Button(self, component, name))
                {
                    return component;
                }

                if (Toggle(self, component, name))
                {
                    return component;
                }

                if (Slider(self, component, name))
                {
                    return component;
                }

                InputField(self, component, name);
                return component;
            }

            return self.GetComponent<T>();
        }

        private static bool Button<T>(Component self, T component, string name) where T : Component
        {
            if (component.TryGetComponent(out Button button))
            {
                if (self is UIPanel panel)
                {
                    button.onClick.AddListener(() =>
                    {
                        if (panel.Interactive())
                        {
                            self.SendMessage(name);
                        }
                    });
                }
                else
                {
                    button.onClick.AddListener(() => self.SendMessage(name));
                }

                return true;
            }

            return false;
        }

        private static bool Toggle<T>(Component self, T component, string name) where T : Component
        {
            if (component.TryGetComponent(out Toggle toggle))
            {
                if (self is UIPanel panel)
                {
                    toggle.onValueChanged.AddListener(value =>
                    {
                        if (panel.Interactive())
                        {
                            self.SendMessage(name, value);
                        }
                    });
                }
                else
                {
                    toggle.onValueChanged.AddListener(value => self.SendMessage(name, value));
                }

                return true;
            }

            return false;
        }

        private static bool Slider<T>(Component self, T component, string name) where T : Component
        {
            if (component.TryGetComponent(out Slider slider))
            {
                if (self is UIPanel panel)
                {
                    slider.onValueChanged.AddListener(value =>
                    {
                        if (panel.Interactive())
                        {
                            self.SendMessage(name, value);
                        }
                    });
                }
                else
                {
                    slider.onValueChanged.AddListener(value => self.SendMessage(name, value));
                }

                return true;
            }

            return false;
        }

        private static bool InputField<T>(Component self, T component, string name) where T : Component
        {
            var cacheType = Search.GetType("TMPro.TMP_InputField,Unity.TextMeshPro");
            if (component.TryGetComponent(cacheType, out var inputField))
            {
                if (self is UIPanel panel)
                {
                    inputField.GetValue<UnityEvent<string>>("onSubmit").AddListener(value =>
                    {
                        if (panel.Interactive())
                        {
                            self.SendMessage(name, value);
                        }
                    });
                }
                else
                {
                    inputField.GetValue<UnityEvent<string>>("onSubmit").AddListener(value => self.SendMessage(name, value));
                }

                return true;
            }

            return false;
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