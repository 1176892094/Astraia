// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-10 21:09:48
// // # Recently: 2025-09-10 21:09:48
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia.Common
{
    internal static partial class ArgManager
    {
        public sealed class Pool<T> : IPool
        {
            private readonly Dictionary<Enum, T> fields = new Dictionary<Enum, T>();

            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public void Set(Enum id, T value)
            {
                Enqueue++;
                Acquire--;
                Release++;
                fields[id] = value;
            }

            public T Get(Enum id)
            {
                Dequeue++;
                Acquire++;
                Release--;
                return fields[id];
            }

            void IDisposable.Dispose()
            {
                fields.Clear();
            }

            public static Pool<T> Create(Type type, string path)
            {
                var instance = new Pool<T>();
                instance.Type = type;
                instance.Path = path;
                return instance;
            }
        }
    }
}