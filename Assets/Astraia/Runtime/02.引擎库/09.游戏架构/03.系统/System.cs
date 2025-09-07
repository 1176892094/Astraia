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
using System.Linq;
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
            foreach (var agent in systemData)
            {
                var result = Service.Find.Type(agent);
                if (result != null)
                {
                    GlobalManager.systemData[result] = (ISystem)Activator.CreateInstance(result);
                }
            }
        }
#if UNITY_EDITOR && ODIN_INSPECTOR
        private static readonly List<string> caches;
        private static List<string> Systems = caches ??= GlobalSetting.GetAgents<ISystem>(caches);

        [HideInEditorMode, ShowInInspector]
        private IEnumerable<ISystem> systems
        {
            get => GlobalManager.systemData.Values.ToList();
            set => Debug.LogError(value);
        }

        [HideInPlayMode, ValueDropdown("Systems")]
#endif
        [SerializeField]
        private List<string> systemData = new List<string>();
    }
}