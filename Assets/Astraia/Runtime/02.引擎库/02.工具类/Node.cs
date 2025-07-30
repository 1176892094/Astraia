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
using Astraia;

namespace Character
{
    [Serializable]
    internal abstract partial class Node
    {
        public Node[] nodes;
        public Entity owner;
        public abstract ValueTask<bool> Execute(int id);
    }

    internal partial class Node
    {
        [Serializable]
        internal class Sequence : Node
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
        internal class Selector : Node
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
        internal class Revealer : Node
        {
            public override async ValueTask<bool> Execute(int id)
            {
                var index = Service.Random.Next(nodes.Length);
                return await nodes[index].Execute(id);
            }
        }

        [Serializable]
        internal class Repeater : Node
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
        internal class WaitTime : Node
        {
            public float waitTime;

            public override async ValueTask<bool> Execute(int id)
            {
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