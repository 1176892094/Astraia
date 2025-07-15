// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-12 18:07:19
// // # Recently: 2025-07-12 18:07:19
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public abstract class Source
    {
        internal int Id;
        public Entity owner => EntityManager.Find(Id);
        public Transform transform => owner != null ? owner.transform : null;
        public GameObject gameObject => owner != null ? owner.gameObject : null;

        public virtual void OnLoad()
        {
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }

        public virtual void OnFade()
        {
        }

        public static implicit operator bool(Source source)
        {
            return source.owner != null && source.owner.isActiveAndEnabled;
        }
    }
}