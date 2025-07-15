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
    public partial class Entity : MonoBehaviour
    {
        internal Dictionary<Type, Source> sourceDict = new Dictionary<Type, Source>();

        public event Action OnShow;
        public event Action OnHide;
        public event Action OnFade;

        public event Action<Collider2D> OnEnter;
        public event Action<Collider2D> OnStay;
        public event Action<Collider2D> OnExit;

        protected virtual void Awake()
        {
            foreach (var source in sourceData)
            {
                AddSource(HeapManager.Dequeue<Source>(Service.Find.Type(source)));
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
            sourceDict.Clear();
            sourceData.Clear();
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

        public T GetSource<T>() where T : Source
        {
            return (T)sourceDict.GetValueOrDefault(typeof(T));
        }

        public void AddSource<T>(T source) where T : Source
        {
            if (sourceDict.TryAdd(source.GetType(), source))
            {
                EntityManager.Show(this);
                source.Id = this;
                source.OnAwake();

                OnFade += Remove;
                OnShow += source.OnShow;
                OnHide += source.OnHide;

                void Remove()
                {
                    HeapManager.Enqueue(source, source.GetType());
                    source.OnDestroy();
                }
            }
        }

        public static implicit operator int(Entity entity)
        {
            return entity.GetInstanceID();
        }
    }

    public partial class Entity
    {
#if UNITY_EDITOR && ODIN_INSPECTOR
        private static string[] sourceNames;
        private static string[] SourceNames => sourceNames ??= AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
            .Where(type => !type.IsAbstract && !type.IsGenericType && typeof(Source).IsAssignableFrom(type)).OrderBy(t => t.FullName)
            .Select(t => Service.Text.Format("{0}, {1}", t.FullName, t.Assembly.GetName().Name)).ToArray();

        [HideInEditorMode, ShowInInspector]
        private List<Source> sourceList
        {
            get => sourceDict.Values.ToList();
            set => sourceDict = value.ToDictionary(k => k.GetType(), v => v);
        }

        [HideInPlayMode, ValueDropdown("SourceNames")]
#endif
        [SerializeField]
        private List<string> sourceData = new List<string>();
    }
}