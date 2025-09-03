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
    public class Entity : MonoBehaviour
    {
        public event Action OnShow;
        public event Action OnHide;
        public event Action OnFade;

        protected virtual void Awake()
        {
            foreach (var agent in agentData)
            {
                var result = Service.Find.Type(agent);
                if (result != null)
                {
                    AddAgent(result);
                }
               
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

        public T AddAgent<T>(T agent) where T : IAgent
        {
            return (T)EntityManager.AddAgent(this, agent, agent.GetType());
        }

        public T AddAgent<T>(T agent, Type queryType) where T : IAgent
        {
            return (T)EntityManager.AddAgent(this, agent, agent.GetType(), queryType);
        }

        public T AddAgent<T>() where T : IAgent
        {
            return (T)EntityManager.AddAgent(this, typeof(T), typeof(T));
        }

        public T GetAgent<T>() where T : IAgent
        {
            return (T)EntityManager.GetAgent(this, typeof(T));
        }

        public IAgent AddAgent(Type realType)
        {
            return EntityManager.AddAgent(this, realType, realType);
        }

        public IAgent AddAgent(Type keyType, Type realType)
        {
            return EntityManager.AddAgent(this, keyType, realType);
        }

        public IAgent AddAgent(Type queryType, Type keyType, Type realType)
        {
            return EntityManager.AddAgent(this, keyType, realType, queryType);
        }

        public IAgent GetAgent(Type keyType)
        {
            return EntityManager.GetAgent(this, keyType);
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private static List<string> Agents = GlobalSetting.GetAgents();

        [HideInEditorMode, ShowInInspector]
        private IEnumerable<IAgent> agents
        {
            get => GlobalManager.agentData.TryGetValue(this, out var agent) ? agent.Values.ToList() : null;
            set => Debug.LogWarning(value, this);
        }

        [HideInPlayMode, ValueDropdown("Agents")]
#endif
        [SerializeField]
        private List<string> agentData = new List<string>();
    }
}