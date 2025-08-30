// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 16:01:50
// # Recently: 2025-01-11 18:01:36
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia.Common
{
    public static partial class EventManager
    {
        [Serializable]
        private class EventPool<T> : IPool where T : struct, IEvent
        {
            private readonly HashSet<IEvent<T>> cached = new HashSet<IEvent<T>>();
            private event Action<T> OnExecute;
            
            public EventPool(Type type)
            {
                this.Type = type;
            }

            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire => cached.Count;
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            void IDisposable.Dispose()
            {
                cached.Clear();
                OnExecute = null;
            }

            public void Listen(IEvent<T> obj)
            {
                Dequeue++;
                if (cached.Add(obj))
                {
                    OnExecute += obj.Execute;
                }
            }

            public void Remove(IEvent<T> obj)
            {
                Enqueue++;
                if (cached.Remove(obj))
                {
                    OnExecute -= obj.Execute;
                }
            }

            public void Invoke(T message)
            {
                Release++;
                OnExecute?.Invoke(message);
            }
        }
    }
}