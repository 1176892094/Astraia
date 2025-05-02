using System;
using Astraia.Common;

namespace Astraia
{
    internal struct Reference
    {
        public string path;
        public Type source;
        public int acquire;
        public int release;
        public int dequeue;
        public int enqueue;

        public Reference(IPool pool)
        {
            this.path = pool.path;
            this.source = pool.source;
            this.acquire = pool.acquire;
            this.release = pool.release;
            this.dequeue = pool.dequeue;
            this.enqueue = pool.enqueue;
        }

        public override string ToString()
        {
            return Service.Text.Format("{0}\t\t{1}\t\t{2}\t\t{3}", release, acquire, dequeue, enqueue);
        }
    }
}