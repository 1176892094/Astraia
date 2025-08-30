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
                this.Type = type;
                this.Path = path;
            }

            public Type Type { get; }
            public string Path { get; }
            public int Acquire => cached.Count;
            public int Release => unused.Count;
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public AudioSource Load()
            {
                Dequeue++;
                AudioSource item;
                if (unused.Count > 0)
                {
                    item = unused.Dequeue();
                    if (item)
                    {
                        cached.Add(item);
                        return item;
                    }

                    Enqueue++;
                    cached.Remove(item);
                }

                item = new GameObject(Path).AddComponent<AudioSource>();
                Object.DontDestroyOnLoad(item.gameObject);
                item.name = Path;
                cached.Add(item);
                return item;
            }

            public void Push(AudioSource item)
            {
                if (cached.Remove(item))
                {
                    Enqueue++;
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