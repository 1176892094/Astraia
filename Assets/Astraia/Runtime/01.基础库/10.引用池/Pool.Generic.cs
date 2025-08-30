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
                this.Type = type;
            }

            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire => cached.Count;
            public int Release => unused.Count;
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            void IDisposable.Dispose()
            {
                cached.Clear();
                unused.Clear();
            }

            public T Load()
            {
                T item;
                lock (unused)
                {
                    Dequeue++;
                    if (unused.Count > 0)
                    {
                        item = unused.Dequeue();
                    }
                    else
                    {
                        item = (T)Activator.CreateInstance(Type);
                    }

                    cached.Add(item);
                }

                return item;
            }

            public void Push(T item)
            {
                lock (unused)
                {
                    if (cached.Remove(item))
                    {
                        Enqueue++;
                        unused.Enqueue(item);
                    }
                }
            }
        }
    }
}