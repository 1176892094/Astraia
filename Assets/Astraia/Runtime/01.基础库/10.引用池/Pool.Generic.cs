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
            private readonly Queue<T> unused = new Queue<T>();
            public HeapPool(Type type) => this.source = type;
            public Type source { get; private set; }
            public string path { get; private set; }
            public int acquire { get; private set; }
            public int release { get; private set; }
            public int dequeue { get; private set; }
            public int enqueue { get; private set; }

            void IDisposable.Dispose()
            {
                unused.Clear();
            }

            public T Dequeue()
            {
                T item;
                lock (unused)
                {
                    dequeue++;
                    if (unused.TryDequeue(out item))
                    {
                        release--;
                    }
                    else
                    {
                        item = (T)Activator.CreateInstance(source);
                    }
                  
                    acquire++;
                }

                return item;
            }

            public void Enqueue(T item)
            {
                lock (unused)
                {
                    enqueue++;
                    acquire--;
                    release++;
                    unused.Enqueue(item);
                }
            }
        }
    }
}