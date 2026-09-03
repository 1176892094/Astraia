// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 21:09:03
// # Recently: 2026-09-02 21:22:59
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

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

                var name = "Assets/Editor/Resources/Settings/{0}.asset".Format(typeof(T).Name);
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