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

namespace Astraia.Common
{
    public static partial class AudioManager
    {
        [Serializable]
        private class Pool : IPool
        {
            private readonly HashSet<AudioSource> cached = new HashSet<AudioSource>();
            private readonly Queue<AudioSource> unused = new Queue<AudioSource>();

            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public AudioSource Load()
            {
                Dequeue++;
                Acquire++;
                AudioSource item;
                if (unused.Count > 0)
                {
                    item = unused.Dequeue();
                    cached.Remove(item);
                    Release--;
                    if (item)
                    {
                        return item;
                    }

                    Enqueue++;
                    Dequeue++;
                }

                item = new GameObject(Path).AddComponent<AudioSource>();
                item.name = Path;
                return item;
            }

            public void Push(AudioSource item)
            {
                Enqueue++;
                if (cached.Add(item))
                {
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

            public static Pool Create(Type type, string path)
            {
                var instance = Activator.CreateInstance<Pool>();
                instance.Type = type;
                instance.Path = path;
                return instance;
            }
        }
    }
}