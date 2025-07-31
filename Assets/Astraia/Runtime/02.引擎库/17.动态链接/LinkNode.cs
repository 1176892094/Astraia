// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-30 16:07:54
// // # Recently: 2025-07-30 16:07:54
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Threading.Tasks;

namespace Astraia
{
    [Serializable]
    public abstract partial class LinkNode
    {
        public LinkNode[] nodes;
        public abstract ValueTask<bool> Execute(int id);
    }

    public partial class LinkNode
    {
        [Serializable]
        public class Sequence : LinkNode
        {
            public override async ValueTask<bool> Execute(int id)
            {
                foreach (var node in nodes)
                {
                    if (!await node.Execute(id))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [Serializable]
        public class Selector : LinkNode
        {
            public override async ValueTask<bool> Execute(int id)
            {
                foreach (var node in nodes)
                {
                    if (await node.Execute(id))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Serializable]
        public class Revealer : LinkNode
        {
            public override async ValueTask<bool> Execute(int id)
            {
                var index = Service.Random.Next(nodes.Length);
                return await nodes[index].Execute(id);
            }
        }

        [Serializable]
        public class Repeater : LinkNode
        {
            public int count;

            public override async ValueTask<bool> Execute(int id)
            {
                for (int i = 0; i < count; i++)
                {
                    foreach (var node in nodes)
                    {
                        if (!await node.Execute(id))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        [Serializable]
        public class WaitTime : LinkNode
        {
            public Entity owner;
            public float waitTime;

            public override async ValueTask<bool> Execute(int id)
            {
                if (owner == null)
                {
                    return false;
                }

                await owner.Wait(waitTime);
                foreach (var node in nodes)
                {
                    if (!await node.Execute(id))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}