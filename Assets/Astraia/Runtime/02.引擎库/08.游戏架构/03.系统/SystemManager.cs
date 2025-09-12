// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-10 22:09:08
// // # Recently: 2025-09-10 22:09:08
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Astraia.Common
{
    using static GlobalManager;

    internal sealed class SystemManager : MonoBehaviour
    {
        private void Awake()
        {
            foreach (var module in systemList)
            {
                var result = Service.Ref.GetType(module);
                if (result != null)
                {
                    systemData[result] = (ISystem)Activator.CreateInstance(result);
                }
            }
        }

        public static void OnUpdate()
        {
            foreach (var panel in panelData.Values)
            {
                if (panel)
                {
                    panel.Update();
                }
            }

            foreach (var system in systemData.Values)
            {
                system.Update();
            }

            for (int i = asyncData.Count - 1; i >= 0; i--)
            {
                asyncData[i].Update();
            }
        }

        internal static void Dispose()
        {
            asyncData.Clear();
            systemData.Clear();
        }
#if UNITY_EDITOR && ODIN_INSPECTOR
        private static List<string> Systems = GlobalSetting.systems;

        [HideInEditorMode, ShowInInspector]
        private IEnumerable<ISystem> systems
        {
            get => systemData.Values.ToList();
            set => Debug.LogError(value);
        }

        [HideInPlayMode, ValueDropdown("Systems")]
#endif
        [SerializeField]
        private List<string> systemList = new List<string>();
    }
}