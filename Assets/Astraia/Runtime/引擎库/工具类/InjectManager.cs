using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Astraia.Core
{
    public static class InjectManager
    {
        public static T Inject<T>(Component self, string name) where T : Component
        {
            var child = self.transform.GetChild(name);
            if (child)
            {
                var component = child.GetComponent<T>();
                switch (component)
                {
                    case Button button:
                        button.onClick.AddListener(() => self.SendMessage(name));
                        break;
                    case Toggle toggle:
                        toggle.onValueChanged.AddListener(value => self.SendMessage(name, value));
                        break;
                    case Slider slider:
                        slider.onValueChanged.AddListener(value => self.SendMessage(name, value));
                        break;
                    case InputField inputField:
                        inputField.onSubmit.AddListener(value => self.SendMessage(name, value));
                        break;
                    case ScrollRect scrollRect:
                        scrollRect.onValueChanged.AddListener(value => self.SendMessage(name, value));
                        break;
                    case UIBehaviour:
                        var cacheType = Search.GetType("TMPro.TMP_InputField,Unity.TextMeshPro");
                        if (component.TryGetComponent(cacheType, out var result))
                        {
                            result.GetValue<UnityEvent<string>>("onSubmit").AddListener(value => self.SendMessage(name, value));
                        }

                        break;
                }

                return component;
            }

            return self.GetComponent<T>();
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