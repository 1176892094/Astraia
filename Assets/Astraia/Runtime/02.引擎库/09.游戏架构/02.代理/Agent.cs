// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-05 15:09:36
// // # Recently: 2025-09-05 15:09:36
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

namespace Astraia.Common
{
    public interface IAgent
    {
        internal void Create(Entity owner);
        void Dequeue();
        void OnShow();
        void OnHide();
        void Enqueue();
    }
}