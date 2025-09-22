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
            private readonly LinkedList<AudioSource> cached = new LinkedList<AudioSource>();
            private readonly Queue<AudioSource> unused = new Queue<AudioSource>();
            private int Capacity;

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
                    Release--;
                    if (item)
                    {
                        cached.AddFirst(item);
                        return item;
                    }
                }

                if (cached.Count >= Capacity && Capacity > 0)
                {
                    item = cached.Last.Value;
                    cached.RemoveLast();
                    cached.AddFirst(item);
                    return item;
                }

                item = new GameObject(Path).AddComponent<AudioSource>();
                item.name = Path;
                cached.AddFirst(item);
                return item;
            }

            public void Push(AudioSource item)
            {
                Enqueue++;
                if (cached.Remove(item))
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

            public static Pool Create(Type type, string path, int capacity)
            {
                var instance = new Pool();
                instance.Type = type;
                instance.Path = path;
                instance.Capacity = capacity;
                return instance;
            }
        }
    }
}