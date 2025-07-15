// // *********************************************************************************
// // # Project: JFramework
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
        internal Dictionary<Type, Agent> agentDict = new Dictionary<Type, Agent>();

        public event Action OnShow;
        public event Action OnHide;
        public event Action OnFade;

        public Action<Collider2D> OnEnter;
        public Action<Collider2D> OnStay;
        public Action<Collider2D> OnExit;

        protected virtual void Awake()
        {
            foreach (var agent in agentData)
            {
                AddAgent(Service.Find.Type(agent));
            }
        }

        protected virtual void OnDestroy()
        {
            OnFade?.Invoke();
            OnShow = null;
            OnHide = null;
            OnFade = null;
            OnStay = null;
            OnExit = null;
            OnEnter = null;

            agentDict.Clear();
            agentData.Clear();
            EntityManager.Hide(this);
        }

        private void OnEnable()
        {
            OnShow?.Invoke();
        }

        private void OnDisable()
        {
            OnHide?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnEnter?.Invoke(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            OnStay?.Invoke(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            OnExit?.Invoke(other);
        }

        public T GetAgent<T>() where T : Agent
        {
            return (T)agentDict.GetValueOrDefault(typeof(T));
        }

        public void AddAgent(Type type)
        {
            AddAgentInternal(HeapManager.Dequeue<Agent>(type), type);
        }

        public void AddAgent<T>(Type type) where T : Agent
        {
            AddAgentInternal(HeapManager.Dequeue<T>(type), typeof(T));
        }

        internal void AddAgentInternal(Agent agent, Type type)
        {
            if (agentDict.TryAdd(type, agent))
            {
                EntityManager.Show(this);
                agent.Id = this;
                agent.OnLoad();

                OnFade += Faded;
                OnShow += agent.OnShow;
                OnHide += agent.OnHide;

                void Faded()
                {
                    HeapManager.Enqueue(agent, type);
                    agent.OnFade();
                }
            }
        }

        public static implicit operator int(Entity entity)
        {
            return entity.GetInstanceID();
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private static string[] agentNames;

        private static string[] AgentNames => agentNames ??= AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
            .Where(type => !type.IsAbstract && !type.IsGenericType && typeof(Agent).IsAssignableFrom(type)).OrderBy(t => t.FullName)
            .Select(t => Service.Text.Format("{0}, {1}", t.FullName, t.Assembly.GetName().Name)).ToArray();

        [HideInEditorMode, ShowInInspector]
        private List<Agent> agentList
        {
            get => agentDict.Values.ToList();
            set => agentDict = value.ToDictionary(k => k.GetType(), v => v);
        }

        [HideInPlayMode, ValueDropdown("AgentNames")]
#endif
        [SerializeField]
        private List<string> agentData = new List<string>();
    }
}