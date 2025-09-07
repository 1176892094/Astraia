// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-05 21:09:38
// // # Recently: 2025-09-05 21:09:38
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia.Common;

namespace Astraia
{
    public abstract class Singleton<T, TEntity> : Agent<TEntity>, IAgent where T : Singleton<T, TEntity> where TEntity : Entity
    {
        private static T instance;

        public static T Instance => instance;

        void IAgent.Create(Entity owner)
        {
            instance = (T)this;
            this.owner = (TEntity)owner;
        }
    }
}