// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 23:04:54
// // # Recently: 2025-04-09 23:04:54
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Astraia
{
    public abstract class EditorSingleton<T> : ScriptableObject where T : EditorSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance)
                {
                    return instance;
                }

                var name = "Assets/Editor/Resources/{0}.asset".Format(typeof(T).Name);
                instance = AssetDatabase.LoadAssetAtPath<T>(name);
                if (instance)
                {
                    return instance;
                }
                
                var path = Path.GetDirectoryName(name);
                if (!Directory.Exists(path) && !string.IsNullOrEmpty(path))
                {
                    Directory.CreateDirectory(path);
                }

                instance = CreateInstance<T>();
                AssetDatabase.CreateAsset(instance, name);
                AssetDatabase.Refresh();
                return instance;
            }
        }
    }
}