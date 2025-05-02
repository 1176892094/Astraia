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
        private class HeapPool<T> : IPool
        {
            private readonly HashSet<T> cached = new HashSet<T>();
            private readonly Queue<T> unused = new Queue<T>();

            public HeapPool(Type type)
            {
                this.type = type;
            }

            public Type type { get; private set; }
            public string path { get; private set; }
            public int acquire => cached.Count;
            public int release => unused.Count;
            public int dequeue { get; private set; }
            public int enqueue { get; private set; }

            void IDisposable.Dispose()
            {
                cached.Clear();
                unused.Clear();
            }

            public T Dequeue()
            {
                T item;
                lock (unused)
                {
                    dequeue++;
                    if (unused.Count > 0)
                    {
                        item = unused.Dequeue();
                    }
                    else
                    {
                        item = (T)Activator.CreateInstance(type);
                    }

                    cached.Add(item);
                }

                return item;
            }

            public void Enqueue(T item)
            {
                lock (unused)
                {
                    if (cached.Remove(item))
                    {
                        enqueue++;
                        unused.Enqueue(item);
                    }
                }
            }
        }
    }
}