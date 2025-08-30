// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 18:01:34
// # Recently: 2025-01-11 18:01:32
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia.Common
{
    public static partial class HeapManager
    {
        [Serializable]
        private class Pool<T> : IPool
        {
            private readonly HashSet<T> cached = new HashSet<T>();
            private readonly Queue<T> unused = new Queue<T>();

            public Pool(Type type)
            {
                Type = type;
                Path = type.Name;
            }

            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public T Load()
            {
                T item;
                if (unused.Count > 0)
                {
                    item = unused.Dequeue();
                    cached.Remove(item);
                }
                else
                {
                    item = (T)Activator.CreateInstance(Type);
                }

                Dequeue++;
                Acquire++;
                Release--;
                return item;
            }

            public void Push(T item)
            {
                if (cached.Add(item))
                {
                    Enqueue++;
                    Acquire--;
                    Release++;
                    unused.Enqueue(item);
                }
            }

            void IDisposable.Dispose()
            {
                cached.Clear();
                unused.Clear();
            }
        }
    }
}