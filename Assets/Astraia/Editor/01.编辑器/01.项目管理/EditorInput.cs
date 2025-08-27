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


using UnityEngine;

namespace Astraia
{
    internal static class EditorInput
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
        public static bool Q => isKeyDown && keyCode == KeyCode.Q && !isModifierKey;
        public static bool W => isKeyDown && keyCode == KeyCode.W && !isModifierKey;
        public static bool E => isKeyDown && keyCode == KeyCode.E && !isModifierKey;
        public static bool R => isKeyDown && keyCode == KeyCode.R && !isModifierKey;
        public static bool isShiftE => isKeyDown && keyCode == KeyCode.E && isShift;
        public static bool isShiftR => isKeyDown && keyCode == KeyCode.R && isShift;
        public static bool isEscape => isKeyDown && keyCode == KeyCode.Escape && !isModifierKey;

        public static void Use()
        {
            Event?.Use();
        }
    }
}