// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 15:09:05
// # Recently: 2026-09-02 15:13:05
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using UnityEngine;

namespace Astraia
{
    public class Entity : Export
    {
        protected override void Awake()
        {
            var modules = GetComponents<IDequeue>();
            foreach (var module in modules)
            {
                module.Dequeue();
            }
        }

        protected override void OnDestroy()
        {
            var modules = GetComponents<IEnqueue>();
            for (var i = modules.Length - 1; i >= 0; i--)
            {
                modules[i].Enqueue();
            }
        }
    }

    public abstract class Export : MonoBehaviour
    {
        protected virtual void Awake() { }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void OnDestroy() { }
    }
}