// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-13 19:07:04
// // # Recently: 2025-07-13 19:07:04
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Astraia
{
    [CustomPropertyDrawer(typeof(SourceAttribute))]
    public class SourceDrawer : PropertyDrawer
    {
        private static string[] sourceNames;
        private static List<Type> sourceTypes;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (sourceTypes == null)
            {
                sourceTypes = new List<Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsAbstract)
                        {
                            continue;
                        }

                        if (type.IsGenericType)
                        {
                            continue;
                        }

                        if (typeof(Source).IsAssignableFrom(type))
                        {
                            sourceTypes.Add(type);
                        }
                    }
                }


                sourceTypes.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
                sourceNames = sourceTypes.Select(t => Service.Text.Format("{0}, {1}", t.FullName, t.Assembly.GetName().Name)).ToArray();
            }

            if (property.propertyType != SerializedPropertyType.String)
            {
                return;
            }

            var currentValue = property.stringValue;

            var index = Array.FindIndex(sourceNames, name => name == currentValue);
            if (index == -1)
            {
                index = 0;
            }

            var selectIndex = EditorGUI.Popup(position, label.text, index, sourceNames);

            if (selectIndex != index)
            {
                property.stringValue = sourceNames[selectIndex];
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            EditorGUI.EndProperty();
        }
    }
}