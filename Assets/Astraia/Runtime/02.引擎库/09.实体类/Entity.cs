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

namespace Astraia
{
    public class Entity : MonoBehaviour
    {
        internal readonly Dictionary<Type, Source> sources = new Dictionary<Type, Source>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.PropertyOrder(1), Source]
#endif
        public List<string> sourcesData = new List<string>();

        public event Action OnShow;
        public event Action OnHide;
        public event Action OnRelease;

        public event Action<Collider2D> OnEnter;
        public event Action<Collider2D> OnStay;
        public event Action<Collider2D> OnExit;

        protected virtual void Awake()
        {
            foreach (var source in sourcesData)
            {
                AddSource(HeapManager.Dequeue<Source>(Service.Find.Type(source)));
            }
        }

        public T GetSource<T>() where T : Source
        {
            return (T)sources.GetValueOrDefault(typeof(T));
        }

        public void AddSource<T>(T source) where T : Source
        {
            if (sources.TryAdd(source.GetType(), source))
            {
                EntityManager.Show(this);
                source.Id = this;
                source.OnAwake();

                OnShow += source.OnShow;
                OnHide += source.OnHide;
                OnRelease += () =>
                {
                    HeapManager.Enqueue(source, source.GetType());
                    source.OnDestroy();
                };
                OnEnter += source.OnEnter;
                OnStay += source.OnStay;
                OnExit += source.OnExit;
            }
        }

        protected virtual void OnDestroy()
        {
            OnRelease?.Invoke();
            OnShow = null;
            OnHide = null;
            OnRelease = null;
            OnEnter = null;
            OnStay = null;
            OnExit = null;
            sources.Clear();
            sourcesData.Clear();
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

        public static implicit operator int(Entity entity)
        {
            return entity.GetInstanceID();
        }
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.PropertyOrder(0), Sirenix.OdinInspector.ShowInInspector]
        public List<Source> sourceList
        {
            get => sources.Values.ToList();
            set => Debug.Log("原始数据被修改" + value);
        }
#endif
    }
}