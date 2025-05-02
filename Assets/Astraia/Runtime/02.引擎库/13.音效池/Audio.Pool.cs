// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:28
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia.Common
{
    public static partial class AudioManager
    {
        private class AudioPool : IPool
        {
            private readonly HashSet<AudioSource> cached = new HashSet<AudioSource>();
            private readonly Queue<AudioSource> unused = new Queue<AudioSource>();

            public AudioPool(Type type, string path)
            {
                this.type = type;
                this.path = path;
            }

            public Type type { get; }
            public string path { get; }
            public int acquire => cached.Count;
            public int release => unused.Count;
            public int dequeue { get; private set; }
            public int enqueue { get; private set; }

            public AudioSource Dequeue()
            {
                dequeue++;
                AudioSource item;
                if (unused.Count > 0)
                {
                    item = unused.Dequeue();
                    if (item != null)
                    {
                        cached.Add(item);
                        return item;
                    }

                    enqueue++;
                    cached.Remove(item);
                }

                item = new GameObject(path).AddComponent<AudioSource>();
                Object.DontDestroyOnLoad(item.gameObject);
                item.name = path;
                cached.Add(item);
                return item;
            }

            public void Enqueue(AudioSource item)
            {
                if (cached.Remove(item))
                {
                    enqueue++;
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