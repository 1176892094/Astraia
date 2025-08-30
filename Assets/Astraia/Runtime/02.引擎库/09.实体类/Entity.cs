// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-12 17:07:42
// // # Recently: 2025-07-12 17:07:42
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Astraia.Common;
using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Astraia
{
    using static GlobalManager;

    public class Entity : MonoBehaviour
    {
        public event Action OnShow;
        public event Action OnHide;
        public event Action OnFade;

        protected virtual void Awake()
        {
            foreach (var agent in agentData)
            {
                AddAgent(Service.Find.Type(agent));
            }
        }

        protected virtual void OnEnable()
        {
            OnShow?.Invoke();
        }

        protected virtual void OnDisable()
        {
            OnHide?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            OnFade?.Invoke();
            OnFade = null;
            OnShow = null;
            OnHide = null;
            agentData.Clear();
        }

        public void AddAgent(IAgent agent)
        {
            EntityManager.AddAgent(this, agent);
        }

        public void AddAgent(Type realType)
        {
            EntityManager.AddAgent(this, realType, realType);
        }

        public void AddAgent(Type baseType, Type realType)
        {
            EntityManager.AddAgent(this, baseType, realType);
        }

        public T GetAgent<T>() where T : IAgent
        {
            return EntityManager.GetAgent<T>(this);
        }

        public static implicit operator int(Entity entity)
        {
            return entity.GetEntityId();
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private static List<string> AgentNames => GlobalSetting.GetAgents();

        [HideInEditorMode, ShowInInspector]
        private IEnumerable<IAgent> agentList
        {
            get => entityData.TryGetValue(this, out var agents) ? agents.Values.ToList() : null;
            set => Debug.LogWarning(value, this);
        }

        [HideInPlayMode, ValueDropdown("AgentNames")]
#endif
        [SerializeField]
        private List<string> agentData = new List<string>();
    }
}