// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 23:04:12
// // # Recently: 2025-04-09 23:04:12
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using UnityEditor;
using UnityEngine;

namespace Astraia
{
    [CustomPropertyDrawer(typeof(Xor.Int))]
    internal class XorIntDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var color = GUI.color;
            GUI.color = Color.green;
            var content = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            GUI.color = color;

            var origin = property.FindPropertyRelative("origin");
            var offset = property.FindPropertyRelative("offset");
            var source = origin.intValue ^ offset.intValue;

            GUI.enabled = false;
            EditorGUI.IntField(content, source);
            GUI.enabled = true;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(Xor.Long))]
    internal class XorLongDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var color = GUI.color;
            GUI.color = Color.green;
            var content = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            GUI.color = color;

            var origin = property.FindPropertyRelative("origin");
            var offset = property.FindPropertyRelative("offset");
            var source = origin.longValue ^ offset.longValue;

            GUI.enabled = false;
            EditorGUI.LongField(content, source);
            GUI.enabled = true;

            EditorGUI.EndProperty();
        }
    }
}