// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 20:04:04
// // # Recently: 2025-04-09 20:04:04
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Astraia
{
    internal sealed class InspectorIcon : Inspector
    {
        private static readonly Dictionary<int, GUIContent> contents = new Dictionary<int, GUIContent>();
        private static InspectorIcon instance;
        private static Vector2 position;
    
        private InspectorIcon()
        {
            Selection.selectionChanged += SelectionChanged;
            EditorManager.OnWindowFocused += OnWindowFocused;
        }
    
        [InitializeOnLoadMethod]
        private static void Enable()
        {
            instance ??= new InspectorIcon();
        }

        protected override void InitInspector(EditorWindow window, VisualElement parent, VisualElement element)
        {
            if (parent[0].name == "Astraia Window")
            {
                parent.RemoveAt(0);
            }
    
            if (element.childCount < 2)
            {
                return;
            }
    
            var editors = Reflection.GetEditors(element[0]);
            if (editors == null || editors.Length < 2)
            {
                return;
            }
    
            var target = editors[0].target;
            if (target is not GameObject && target.GetType() != Reflection.Importer)
            {
                return;
            }
    
            contents.Clear();
            parent.Insert(0, new IMGUIContainer(OnGUIHandler)
            {
                name = "Astraia Window",
                style =
                {
                    height = 20f,
                    backgroundColor = Color.black * 0.2f,
                }
            });
    
            void OnGUIHandler()
            {
                if (element.childCount < 1)
                {
                    return;
                }

                var elements = Reflection.GetEditors(element[0]);

                if (elements == null || editors.Length < 2)
                {
                    return;
                }
    
                var index = 0;
                position = Vector2.zero;
    
                switch (Event.current.type)
                {
                    case EventType.Repaint:
                        DrawExpand(window, editors);
                        for (var i = 0; i < element.childCount; i++)
                        {
                            index = DrawIcon(window, element, i, editors, index);
                        }
    
                        break;
                    case EventType.MouseUp:
                        DrawExpand(window, editors);
                        for (var i = 0; i < element.childCount; i++)
                        {
                            index = DrawIcon(window, element, i, editors, index);
                        }
    
                        break;
                    case EventType.MouseDown:
                        DrawExpand(window, editors);
                        break;
                    case EventType.Used:
                        DrawExpand(window, editors);
                        break;
                }
            }
        }
    
        private static void DrawExpand(EditorWindow window, Editor[] editors)
        {
            var status = false;
            var tracker = Reflection.GetTracker(window);
            for (var i = 1; i < editors.Length; ++i)
            {
                if (editors[i] is MaterialEditor)
                {
                    if (InternalEditorUtility.GetIsInspectorExpanded(editors[i].target))
                    {
                        status = true;
                        break;
                    }
                }
                else if (tracker.GetVisible(i) == 1)
                {
                    status = true;
                    break;
                }
            }
    
            var content = status ? Reflection.collapse : Reflection.expansion;
            if (Button(content) == Status.Click)
            {
                for (var i = 1; i < editors.Length; i++)
                {
                    if (editors[i] is MaterialEditor)
                    {
                        InternalEditorUtility.SetIsInspectorExpanded(editors[i].target, !status);
                    }
                    else
                    {
                        tracker.SetVisible(i, status ? 0 : 1);
                    }
                }
            }
        }
    
        private static int DrawIcon(EditorWindow window, VisualElement elements, int i, Editor[] editors, int index)
        {
            var element = elements[i];
            if (element.GetType().Name != "EditorElement")
            {
                return index;
            }
    
            if (element.childCount < 2)
            {
                return index;
            }
    
            if (editors.Length <= index)
            {
                return index;
            }
    
            var editor = editors[index];
            if (editor.target == null || editor.target is Material)
            {
                return index;
            }
    
            var display = element.style.display;
            var visible = display.keyword == StyleKeyword.Null || display == DisplayStyle.Flex;
    
            switch (Button(Content(editor.target)))
            {
                case Status.Click:
                    if (Event.current.button == 0)
                    {
                        var count = 0;
                        for (var j = 0; j < elements.childCount; ++j)
                        {
                            if (elements[j].childCount >= 2)
                            {
                                display = elements[j].style.display;
                                if (display.keyword == StyleKeyword.Null || display == DisplayStyle.Flex)
                                {
                                    count++;
                                }
                            }
                        }
    
                        if (visible && count == 1)
                        {
                            for (var j = 0; j < elements.childCount; ++j)
                            {
                                elements[j].style.display = DisplayStyle.Flex;
                            }
                        }
                        else
                        {
                            for (var j = 0; j < elements.childCount; ++j)
                            {
                                elements[j].style.display = i == j ? DisplayStyle.Flex : DisplayStyle.None;
                            }
    
                            Reflection.GetTracker(window).SetVisible(index, 1);
                        }
                    }
                    else if (Event.current.button == 1)
                    {
                        var itemRect = new Rect(Event.current.mousePosition, Vector2.zero);
                        Reflection.ShowContext(itemRect, editor.target);
                    }
    
                    Event.current.Use();
                    break;
                case Status.Hover:
                    window.Focus();
                    break;
            }
    
            return index + 1;
        }
    
        private static GUIContent Content(Object component)
        {
            var componentId = component.GetInstanceID();
            if (!contents.TryGetValue(componentId, out var content))
            {
                var icon = AssetPreview.GetMiniThumbnail(component);
                content = new GUIContent(icon, ObjectNames.NicifyVariableName(component.GetType().Name));
                if (icon.name is "cs Script Icon" or "d_cs Script Icon")
                {
                    content.image = Reflection.scriptIcon.image;
                }
    
                contents[componentId] = content;
            }
    
            return content;
        }
    
        private static Status Button(GUIContent content)
        {
            var rect = new Rect(position.x, position.y, 23, 21);
            var button = GUIUtility.GetControlID("Button".GetHashCode(), FocusType.Passive, rect);
            position.x += 23;
            switch (Event.current.type)
            {
                case EventType.Repaint when rect.Contains(Event.current.mousePosition):
                    EditorStyles.toolbarButton.Draw(rect, content, button, false, true);
                    return Status.Hover;
                case EventType.Repaint:
                    EditorStyles.toolbarButton.Draw(rect, content, button);
                    return Status.None;
                case EventType.MouseDown:
                    Event.current.Use();
                    return Status.Down;
                case EventType.MouseUp when rect.Contains(Event.current.mousePosition):
                    Event.current.Use();
                    return Status.Click;
            }
    
            return Status.None;
        }
    
        private enum Status
        {
            None,
            Down,
            Click,
            Hover,
        }
    }
}