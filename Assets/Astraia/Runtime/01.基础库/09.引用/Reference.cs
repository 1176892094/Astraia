using System;
using Astraia.Common;

namespace Astraia
{
    internal struct Reference
    {
        public Type type;
        public string path;
        public int acquire;
        public int release;
        public int dequeue;
        public int enqueue;

        public Reference(IPool pool)
        {
            this.type = pool.type;
            this.path = pool.path;
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