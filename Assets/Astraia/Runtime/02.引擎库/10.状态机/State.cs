// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:32
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia
{
    public abstract partial class StateMachine
    {
        [Serializable]
        public abstract class State
        {
            internal int Id;
            public Entity owner => EntityManager.Find(Id);

            public abstract void OnEnter();

            public abstract void OnUpdate();

            public abstract void OnExit();
        }
    }
}