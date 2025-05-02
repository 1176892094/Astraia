// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 18:01:53
// # Recently: 2025-01-11 18:01:31
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia.Common
{
    internal interface IPool : IDisposable
    {
        public Type source { get; }
        public string path { get; }
        public int acquire { get; }
        public int release { get; }
        public int dequeue { get; }
        public int enqueue { get; }
    }

    internal readonly struct Pool : IPool
    {
        public Type type { get; }
        public string path { get; }
        public int acquire { get; }
        public int release { get; }
        public int dequeue { get; }
        public int enqueue { get; }

        public Pool(IPool pool)
        {
            type = pool.type;
            path = pool.path;
            acquire = pool.acquire;
            release = pool.release;
            dequeue = pool.dequeue;
            enqueue = pool.enqueue;
        }

        public override string ToString()
        {
            return Service.Text.Format("{0}\t\t{1}\t\t{2}\t\t{3}", release, acquire, dequeue, enqueue);
        }

        public void Dispose()
        {
        }
    }
}