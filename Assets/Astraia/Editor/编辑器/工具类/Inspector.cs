// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 19:09:16
// # Recently: 2026-09-02 21:23:00
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astraia
{
    internal static partial class Inspector
    {
        private static IEnumerable<EditorWindow> inspectors;
        private static VisualElement inspector;

        public static void SelectionChanged()
        {
            if (inspectors == null)
            {
                var windows = EditorReference.Inspector.GetValue<IList>("m_AllInspectors");
                inspectors = windows.Cast<EditorWindow>().Where(r => r.GetType() == EditorReference.Inspector);
            }

            foreach (var window in inspectors)
            {
                InitWindow(window);
            }
        }

        public static void InitWindow(EditorWindow window)
        {
            var parent = FindElement(window.rootVisualElement, "unity-inspector-main-container");
            var element = FindElement(window.rootVisualElement, "unity-inspector-editors-list");
            if (parent != null && parent.childCount >= 2)
            {
                InitInspector(element);
            }
        }

        private static VisualElement FindElement(VisualElement parent, string className)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var element = parent[i];
                if (element.ClassListContains(className))
                {
                    return element;
                }

                element = FindElement(element, className);
                if (element != null)
                {
                    return element;
                }
            }

            return null;
        }

        private static void InitInspector(VisualElement element)
        {
            if (element.parent[0].name == "Astraia Settings")
            {
                element.parent.RemoveAt(0);
            }

            if (element.childCount != 0)
            {
                return;
            }

            inspector ??= InitInspector();
            element.parent.Insert(0, inspector);
        }

        private static VisualElement InitInspector()
        {
            var element = new VisualElement { name = "Astraia Settings", };
            InitTitle(element);
            InitTools(element);
            InitPackages(element);
            return element;
        }

        private static void InitTitle(VisualElement element)
        {
            element.Add(new Label(element.name)
            {
                style =
                {
                    height = 21f,
                    fontSize = 14f,
                    borderBottomWidth = 1f,
                    borderBottomColor = Color.black * 0.6f,
                    backgroundColor = Color.gray * 0.6f,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            });
        }

        private static void InitTools(VisualElement parent)
        {
            VisualElement element = null;
            foreach (var submenu in Unsupported.GetSubmenus("Tools"))
            {
                var index = submenu.LastIndexOf('/');
                if (submenu.Contains("Astraia"))
                {
                    if (element == null)
                    {
                        SetLabel(parent, "Astraia");
                        element = SetElement(parent);
                    }

                    SetButton(element, submenu, submenu.Substring(index + 1));
                }
            }
        }

        private static void InitPackages(VisualElement parent)
        {
            SetLabel(parent, "Settings");
            var element = SetElement(parent);
            SetButton(element, "File/Build Profiles", "Build Settings");
            SetButton(element, "Edit/Project Settings...", "Project Settings");
            SetButton(element, "Unity/Settings…", "Preferences");

            var window = string.Empty;
            var started = false;

            foreach (var submenu in Unsupported.GetSubmenus("Window"))
            {
                var upper = submenu.ToUpper();

                if (!started)
                {
                    if (upper != "WINDOW/GENERAL/SCENE")
                    {
                        continue;
                    }

                    started = true;
                }

                if (upper == "WINDOW/RENDERING/LIGHTING")
                {
                    break;
                }

                // 处理当前项
                var index = submenu.LastIndexOf('/');
                var itemText = submenu.Substring(0, index);
                var nameText = submenu.Substring(index + 1);
                index = itemText.LastIndexOf('/');

                if (index == -1)
                {
                    SetButton(element, submenu, nameText);
                }
                else
                {
                    if (window != itemText)
                    {
                        window = itemText;
                        SetLabel(parent, itemText.Substring(index + 1));
                        element = SetElement(parent);
                    }

                    SetButton(element, submenu, nameText);
                }
            }
        }

        private static void SetLabel(VisualElement parent, string text)
        {
            parent.Add(new Label(text)
            {
                style =
                {
                    fontSize = 12f,
                    marginTop = 3f,
                    marginLeft = 3f,
                    marginRight = 3f,
                    paddingLeft = 5f
                }
            });
        }

        private static void SetButton(VisualElement parent, string submenu, string text)
        {
            var button = new EditorToolbarButton(() => EditorApplication.ExecuteMenuItem(submenu))
            {
                style =
                {
                    height = 20,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    left = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0
                }
            };

            typeof(Button).GetProperty("text", Search.Instance)?.SetValue(button, text);
            parent.Add(button);
        }

        private static VisualElement SetElement(VisualElement parent)
        {
            var element = new VisualElement
            {
                style =
                {
                    borderTopWidth = 1f,
                    borderBottomWidth = 1f,
                    borderTopColor = Color.black * 0.6f,
                    borderBottomColor = Color.black * 0.6f,
                    marginTop = 2f
                }
            };
            parent.Add(element);
            return element;
        }
    }
}