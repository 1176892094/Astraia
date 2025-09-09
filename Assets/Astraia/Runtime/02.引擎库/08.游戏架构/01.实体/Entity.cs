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
    public partial class Entity : MonoBehaviour
    {
        internal readonly Dictionary<Type, IAgent> agentData = new Dictionary<Type, IAgent>();
        internal bool isDestroy;
        public event Action OnShow;
        public event Action OnHide;
        public event Action OnFade;

        protected virtual void Awake()
        {
            foreach (var agent in agentList)
            {
                AddComponent(Service.Ref.GetType(agent));
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
            isDestroy = true;
            OnFade?.Invoke();
            OnFade = null;
            OnShow = null;
            OnHide = null;
            agentList.Clear();
            agentData.Clear();
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private static List<string> Agents = GlobalSetting.agents;

        [HideInEditorMode, ShowInInspector]
        private IEnumerable<IAgent> agents
        {
            get => agentData.Values.ToList();
            set => Debug.LogError(value);
        }

        [HideInPlayMode, ValueDropdown("Agents")]
#endif
        [SerializeField]
        private List<string> agentList = new List<string>();
    }

    public partial class Entity
    {
        public T AddComponent<T>(T agent) where T : IAgent
        {
            return (T)EntityManager.AddComponent(this, agent, agent.GetType());
        }

        public T AddComponent<T>(T agent, Type queryType) where T : IAgent
        {
            return (T)EntityManager.AddComponent(this, agent, agent.GetType(), queryType);
        }

        public T AddComponent<T>() where T : IAgent
        {
            return (T)EntityManager.AddComponent(this, typeof(T), typeof(T));
        }

        public T FindComponent<T>() where T : IAgent
        {
            return agentData.TryGetValue(typeof(T), out var agent) ? (T)agent : default;
        }

        public IAgent AddComponent(Type realType)
        {
            return EntityManager.AddComponent(this, realType, realType);
        }

        public IAgent AddComponent(Type keyType, Type realType)
        {
            return EntityManager.AddComponent(this, keyType, realType);
        }

        public IAgent AddComponent(Type queryType, Type keyType, Type realType)
        {
            return EntityManager.AddComponent(this, keyType, realType, queryType);
        }

        public IAgent FindComponent(Type keyType)
        {
            return agentData.TryGetValue(keyType, out var agent) ? agent : null;
        }
    }
}