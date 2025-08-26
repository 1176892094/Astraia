// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-26 23:08:50
// // # Recently: 2025-08-26 23:08:50
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************


using UnityEditor;
using UnityEngine;

namespace Astraia
{
    internal static class EditorEvent
    {
        private static readonly Event current;
        private static readonly Event Event = current ??= typeof(Event).GetValue<Event>("s_Current");

        public static bool isRepaint => Event.type == EventType.Repaint;
        public static bool isLayout => Event.type == EventType.Layout;
        public static KeyCode keyCode => Event.keyCode;
        public static bool isKeyDown => Event.type == EventType.KeyDown;
        public static bool isMouseUp => Event.type == EventType.MouseUp;
        public static bool isMouseDown => Event.type == EventType.MouseDown;
        public static bool isMouseMove => Event.type == EventType.MouseMove;
        public static bool isMouseDrag => Event.type == EventType.MouseDrag;
        public static EventModifiers modifiers => Event.modifiers;
        public static bool isModifierKey => modifiers != EventModifiers.None;
        public static int mouseButton => Event.button;
        public static Vector2 mousePosition => Event.mousePosition;
        public static bool isAlt => Event.alt;
        public static bool isShift => Event.shift;
        public static bool isCtrl => Event.control || Event.command;
        public static bool E => isKeyDown && keyCode == KeyCode.E && !isModifierKey;

        private static bool wasAlt;
        private static bool wasShift;
        private static int pressedId;

        public static void Use()
        {
            Event?.Use();
        }

        public static void RepaintKeyboard()
        {
            var keyboard = typeof(Event).GetValue<Event>("s_Current");
            var window = EditorWindow.mouseOverWindow;
            if (wasAlt && !keyboard.alt)
            {
                if (window && window.GetType() == Reflection.Browser)
                {
                    EditorApplication.RepaintProjectWindow();
                }
            }

            if (wasShift && !keyboard.shift)
            {
                if (window && window.GetType() == Reflection.Hierarchy)
                {
                    EditorApplication.RepaintHierarchyWindow();
                }
            }

            wasAlt = keyboard.alt;
            wasShift = keyboard.shift;
        }

        public static bool Button(Rect rect, string iconName, float iconSize = 0, Color color = default, Color colorHovered = default, Color colorPressed = default)
        {
            var id = GUIUtility.GUIToScreenRect(rect).GetHashCode();
            var isPressed = id == pressedId;

            var isActive = false;

            Reflection.MarkInteractive(rect);

            if (isRepaint)
            {
                if (color == default)
                {
                    color = Color.white;
                }

                if (colorHovered == default)
                {
                    colorHovered = Color.white;
                }

                if (colorPressed == default)
                {
                    colorPressed = new Color(1, 1, 1, 0.5f);
                }

                if (rect.Contains(mousePosition))
                {
                    color = colorHovered;
                }

                if (isPressed)
                {
                    color = colorPressed;
                }

                if (iconSize == 0)
                {
                    iconSize = Mathf.Min(rect.width, rect.height);
                }

                var iconRect = new Rect(rect.center.x - iconSize / 2, rect.center.y - iconSize / 2, iconSize, iconSize);

                var oldColor = GUI.color;
                GUI.color *= color;
                GUI.DrawTexture(iconRect, EditorIcon.GetIcon(iconName));
                GUI.color = oldColor;
            }

            if (isMouseDown)
            {
                if (rect.Contains(mousePosition))
                {
                    pressedId = id;
                    Use();
                }
            }

            if (isMouseUp)
            {
                if (isPressed)
                {
                    pressedId = 0;
                    if (rect.Contains(mousePosition))
                    {
                        isActive = true;
                    }

                    Use();
                }
            }

            if (isMouseDrag)
            {
                if (isPressed)
                {
                    Use();
                }
            }

            return isActive;
        }
    }
}