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
            foreach (var system in systemList)
            {
                var result = Service.Ref.GetType(system);
                if (result != null)
                {
                    systemData.Add((ISystem)Activator.CreateInstance(result));
                }
            }

            systemData.Add(new UISystem());
            systemData.Add(new AsyncSystem());
        }

        internal static void OnUpdate()
        {
            foreach (var system in systemData)
            {
                system.Update();
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
            get => systemData;
            set => Debug.LogError(value);
        }

        [HideInPlayMode, ValueDropdown("Systems")]
#endif
        [SerializeField]
        private List<string> systemList = new List<string>();
    }

    public struct UISystem : ISystem
    {
        public void Update()
        {
            for (int i = panelPage.Count - 1; i >= 0; i--)
            {
                panelPage[i].Update();
            }
        }
    }

    public struct AsyncSystem : ISystem
    {
        public void Update()
        {
            for (int i = asyncData.Count - 1; i >= 0; i--)
            {
                asyncData[i].Update();
            }
        }
    }
}