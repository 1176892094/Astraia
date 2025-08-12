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
        internal Dictionary<Type, IAgent> agentDict = new Dictionary<Type, IAgent>();

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
            agentDict.Clear();
            agentData.Clear();
            EntityManager.Hide(this);
        }

        public T GetAgent<T>() where T : IAgent
        {
            return (T)agentDict.GetValueOrDefault(typeof(T));
        }

        public void AddAgent(Type type)
        {
            IAgent agent = null;
            try
            {
                agent = HeapManager.Dequeue<IAgent>(type);
                AddAgentInternal(agent, type);
            }
            catch (Exception e)
            {
                Debug.LogWarning(Service.Text.Format("无法添加代理组件: {0} 类型: {1}\n{2}", agent, type, e), gameObject);
            }
        }

        public void AddAgent<T>(Type type) where T : IAgent
        {
            IAgent agent = null;
            try
            {
                agent = HeapManager.Dequeue<IAgent>(type);
                AddAgentInternal(agent, typeof(T));
            }
            catch (Exception e)
            {
                Debug.LogWarning(Service.Text.Format("无法添加代理组件: {0} 类型: {1}\n{2}", agent, type, e), gameObject);
            }
        }

        internal void AddAgentInternal(IAgent agent, Type type)
        {
            try
            {
                if (agentDict.TryAdd(type, agent))
                {
                    EntityManager.Show(this);
                    agent.OnAwake(this);
                    agent.OnAwake();
                    OnShow += agent.OnShow;
                    OnHide += agent.OnHide;
                    OnFade += Faded;

                    void Faded()
                    {
                        HeapManager.Enqueue(agent, type);
                        agent.OnDestroy();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(Service.Text.Format("无法添加代理组件: {0} 类型: {1}\n{2}", agent, type, e), gameObject);
            }
        }

        public static implicit operator int(Entity entity)
        {
            return entity.GetInstanceID();
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private static List<string> AgentNames => GlobalSetting.GetAgents();

        [HideInEditorMode, ShowInInspector]
        private List<IAgent> agentList
        {
            get => agentDict.Values.ToList();
            set => agentDict = value.ToDictionary(t => t.GetType());
        }

        [HideInPlayMode, ValueDropdown("AgentNames")]
#endif
        [SerializeField]
        private List<string> agentData = new List<string>();
    }
}