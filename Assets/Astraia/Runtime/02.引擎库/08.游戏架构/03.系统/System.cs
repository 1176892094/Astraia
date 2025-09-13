// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-31 00:08:10
// // # Recently: 2025-08-31 00:08:10
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
    public interface ISystem
    {
        void Update();
    }

    internal sealed class System : MonoBehaviour
    {
        private void Awake()
        {
            foreach (var system in systemList)
            {
                var result = Service.Ref.GetType(system);
                if (result != null)
                {
                    GlobalManager.systemLoop.Add((ISystem)Activator.CreateInstance(result));
                }
            }
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private static List<string> Systems = GlobalSetting.systems;

        [HideInEditorMode, ShowInInspector]
        private IEnumerable<ISystem> systems
        {
            get => GlobalManager.systemLoop;
            set => Debug.LogError(value);
        }

        [HideInPlayMode, ValueDropdown("Systems")]
#endif
        [SerializeField]
        private List<string> systemList = new List<string>();
    }
}