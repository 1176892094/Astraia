// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-28 03:08:00
// // # Recently: 2025-08-28 03:08:00
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia
{
    internal static partial class Hierarchy
    {
        private static readonly Dictionary<Object, ComponentData> copiedData = new();
        private static readonly object objectValue = new();

        private static void Copy(Component component)
        {
            Undo.RecordObject(component, string.Empty);
            if (copiedData.ContainsKey(component))
            {
                copiedData.Remove(component);
            }
            else
            {
                copiedData.Add(component, GetData(component));
            }

            EditorUtility.SetDirty(component);
        }

        private static void Paste(ComponentData data, Component component)
        {
            Undo.RecordObject(component, string.Empty);
            Apply(data, component);
            copiedData.Remove(data.component);
            EditorUtility.SetDirty(component);
        }
        
        private static ComponentData GetData(Component component, bool save = false)
        {
            var data = new ComponentData(component);
            if (save)
            {
                data.id = GlobalObjectId.GetGlobalObjectIdSlow(component);
            }

            var property = new SerializedObject(component).GetIterator();
            if (!property.Next(true))
            {
                return data;
            }

            do
            {
                data.properties[property.propertyPath] = GetBoxedValue(property);
            } while (property.NextVisible(true));

            return data;
        }


        private static void Apply(ComponentData data, Component component)
        {
            var copies = data.properties.Keys.ToList();
            foreach (var key in copies)
            {
                if (data.properties[key] is Object o && !o)
                {
                    data.properties[key] = EditorUtility.InstanceIDToObject(o.GetInstanceID());
                }
            }

            foreach (var kvp in data.properties)
            {
                var so = new SerializedObject(component);
                var property = so.FindProperty(kvp.Key);

                so.Update();
                SetBoxedValue(property, kvp.Value);
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(component);
            }
        }

        [Serializable]
        public class ComponentData
        {
            public Dictionary<string, object> properties = new();
            public GlobalObjectId id;
            public Component component;

            public ComponentData(Component component)
            {
                this.component = component;
            }
        }

        private static object GetBoxedValue(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Integer:
                    switch (p.numericType)
                    {
                        case SerializedPropertyNumericType.Int8:
                            return (sbyte)p.intValue;
                        case SerializedPropertyNumericType.UInt8:
                            return (byte)p.uintValue;
                        case SerializedPropertyNumericType.Int16:
                            return (short)p.intValue;
                        case SerializedPropertyNumericType.UInt16:
                            return (ushort)p.uintValue;
                        case SerializedPropertyNumericType.Int32:
                            return p.intValue;
                        case SerializedPropertyNumericType.UInt32:
                            return p.uintValue;
                        case SerializedPropertyNumericType.Int64:
                            return p.longValue;
                        case SerializedPropertyNumericType.UInt64:
                            return p.ulongValue;
                        default:
                            return p.intValue;
                    }
                case SerializedPropertyType.Boolean:
                    return p.boolValue;
                case SerializedPropertyType.Float:
                    switch (p.numericType)
                    {
                        case SerializedPropertyNumericType.Double:
                            return p.doubleValue;
                        default:
                        case SerializedPropertyNumericType.Float:
                            return p.floatValue;
                    }
                case SerializedPropertyType.String:
                    return p.stringValue;
                case SerializedPropertyType.Color:
                    return p.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return p.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (LayerMask)p.intValue;
                case SerializedPropertyType.Enum:
                    return p.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return p.vector2Value;
                case SerializedPropertyType.Vector3:
                    return p.vector3Value;
                case SerializedPropertyType.Vector4:
                    return p.vector4Value;
                case SerializedPropertyType.Rect:
                    return p.rectValue;
                case SerializedPropertyType.ArraySize:
                    return p.intValue;
                case SerializedPropertyType.Character:
                    return (ushort)p.uintValue;
                case SerializedPropertyType.AnimationCurve:
                    return p.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return p.boundsValue;
                case SerializedPropertyType.Gradient:
                    return p.gradientValue;
                case SerializedPropertyType.Quaternion:
                    return p.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return p.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    return p.intValue;
                case SerializedPropertyType.Vector2Int:
                    return p.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return p.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return p.rectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return p.boundsIntValue;
                case SerializedPropertyType.ManagedReference:
                    return p.managedReferenceValue;
                case SerializedPropertyType.Hash128:
                    return p.hash128Value;
                case SerializedPropertyType.RenderingLayerMask:
                    break;
            }

            return objectValue;
        }


        private static void SetBoxedValue(SerializedProperty p, object value)
        {
            if (value == objectValue)
            {
                return;
            }

            switch (p.propertyType)
            {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Integer:
                    if (p.numericType == SerializedPropertyNumericType.UInt64)
                    {
                        p.ulongValue = Convert.ToUInt64(value);
                        return;
                    }

                    p.longValue = Convert.ToInt64(value);
                    return;
                case SerializedPropertyType.Boolean:
                    p.boolValue = (bool)value;
                    return;
                case SerializedPropertyType.Float:
                    if (p.numericType == SerializedPropertyNumericType.Double)
                    {
                        p.doubleValue = Convert.ToDouble(value);
                        return;
                    }

                    p.floatValue = Convert.ToSingle(value);
                    return;
                case SerializedPropertyType.String:
                    p.stringValue = (string)value;
                    return;
                case SerializedPropertyType.Color:
                    p.colorValue = (Color)value;
                    return;
                case SerializedPropertyType.ObjectReference:
                    p.objectReferenceValue = (Object)value;
                    return;
                // case SerializedPropertyType.LayerMask:
                //     p.enumValueFlag = (int)value;
                //     return;
                case SerializedPropertyType.Enum:
                    p.enumValueIndex = (int)value;
                    return;
                case SerializedPropertyType.Vector2:
                    p.vector2Value = (Vector2)value;
                    return;
                case SerializedPropertyType.Vector3:
                    p.vector3Value = (Vector3)value;
                    return;
                case SerializedPropertyType.Vector4:
                    p.vector4Value = (Vector4)value;
                    return;
                case SerializedPropertyType.Rect:
                    p.rectValue = (Rect)value;
                    return;
                case SerializedPropertyType.ArraySize:
                    if (p.numericType == SerializedPropertyNumericType.UInt64)
                    {
                        p.ulongValue = Convert.ToUInt64(value);
                        return;
                    }

                    p.longValue = Convert.ToInt64(value);
                    return;
                case SerializedPropertyType.Character:
                    p.uintValue = Convert.ToUInt16(value);
                    return;
                case SerializedPropertyType.AnimationCurve:
                    p.animationCurveValue = (AnimationCurve)value;
                    return;
                case SerializedPropertyType.Bounds:
                    p.boundsValue = (Bounds)value;
                    return;
                case SerializedPropertyType.Gradient:
                    p.gradientValue = (Gradient)value;
                    return;
                case SerializedPropertyType.Quaternion:
                    p.quaternionValue = (Quaternion)value;
                    return;
                case SerializedPropertyType.ExposedReference:
                    p.exposedReferenceValue = (Object)value;
                    return;
                case SerializedPropertyType.FixedBufferSize:
                    return;
                case SerializedPropertyType.Vector2Int:
                    p.vector2IntValue = (Vector2Int)value;
                    return;
                case SerializedPropertyType.Vector3Int:
                    p.vector3IntValue = (Vector3Int)value;
                    return;
                case SerializedPropertyType.RectInt:
                    p.rectIntValue = (RectInt)value;
                    return;
                case SerializedPropertyType.BoundsInt:
                    p.boundsIntValue = (BoundsInt)value;
                    return;
                case SerializedPropertyType.ManagedReference:
                    p.managedReferenceValue = value;
                    return;
                case SerializedPropertyType.Hash128:
                    p.hash128Value = (Hash128)value;
                    return;
                case SerializedPropertyType.RenderingLayerMask:
                    break;
            }
        }
    }
}