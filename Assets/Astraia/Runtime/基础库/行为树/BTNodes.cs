// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:08
// # Recently: 2026-08-13 22:49:08
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Threading.Tasks;

namespace Astraia
{
    public interface INode
    {
        Task<Async.State> OnTick(int[] indices, Blackboard<int> root);
    }

    [Serializable]
    public readonly struct Sequence : INode
    {
        private readonly int Index;
        private readonly INode[] Nodes;

        public Sequence(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public async Task<Async.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var current = indices[Index];
            while (current < Nodes.Length)
            {
                var state = await Nodes[current].OnTick(indices, root);
                if (state == Async.State.Running)
                {
                    return Async.State.Running;
                }

                if (state == Async.State.Failure)
                {
                    indices[Index] = 0;
                    return Async.State.Failure;
                }

                current++;
                indices[Index] = current;
            }

            indices[Index] = 0;
            return Async.State.Success;
        }
    }

    [Serializable]
    public readonly struct Selector : INode
    {
        private readonly int Index;
        private readonly INode[] Nodes;

        public Selector(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public async Task<Async.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var current = indices[Index];
            while (current < Nodes.Length)
            {
                var state = await Nodes[current].OnTick(indices, root);
                if (state == Async.State.Running)
                {
                    return Async.State.Running;
                }

                if (state == Async.State.Success)
                {
                    indices[Index] = 0;
                    return Async.State.Success;
                }

                current++;
                indices[Index] = current;
            }

            indices[Index] = 0;
            return Async.State.Failure;
        }
    }

    [Serializable]
    public readonly struct Parallel : INode
    {
        private readonly bool IsAny;
        private readonly INode[] Nodes;

        public Parallel(string isAny, INode[] nodes)
        {
            IsAny = isAny == "Any";
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public async Task<Async.State> OnTick(int[] indices, Blackboard<int> root)
        {
            if (IsAny)
            {
                foreach (var node in Nodes)
                {
                    var state = await node.OnTick(indices, root);
                    if (state == Async.State.Success)
                    {
                        return Async.State.Success;
                    }

                    if (state == Async.State.Failure)
                    {
                        return Async.State.Failure;
                    }
                }

                return Async.State.Running;
            }

            var isAll = true;
            foreach (var node in Nodes)
            {
                var state = await node.OnTick(indices, root);
                if (state == Async.State.Failure)
                {
                    return Async.State.Failure;
                }

                if (state == Async.State.Running)
                {
                    isAll = false;
                }
            }

            return isAll ? Async.State.Success : Async.State.Running;
        }
    }

    [Serializable]
    public readonly struct Randomer : INode
    {
        private readonly int Index;
        private readonly INode[] Nodes;

        public Randomer(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public async Task<Async.State> OnTick(int[] indices, Blackboard<int> root)
        {
            if (indices[Index] == 0)
            {
                indices[Index] = Seed.Next(Nodes.Length) + 1;
            }

            var state = await Nodes[indices[Index] - 1].OnTick(indices, root);
            if (state == Async.State.Running)
            {
                return Async.State.Running;
            }

            indices[Index] = 0;
            return state;
        }
    }

    [Serializable]
    public readonly struct Repeater : INode
    {
        private readonly int Index;
        private readonly int Count;
        private readonly INode Node;

        public Repeater(int index, int count, INode node)
        {
            Node = node;
            Index = index;
            Count = count;
        }

        public async Task<Async.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var state = await Node.OnTick(indices, root);
            if (state == Async.State.Running)
            {
                return Async.State.Running;
            }

            indices[Index]++;
            if (Count < 0 || indices[Index] < Count)
            {
                return Async.State.Running;
            }

            indices[Index] = 0;
            return Async.State.Success;
        }
    }

    [Serializable]
    public readonly struct Inverter : INode
    {
        private readonly INode Node;

        public Inverter(INode node)
        {
            Node = node;
        }

        public async Task<Async.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var state = await Node.OnTick(indices, root);
            switch (state)
            {
                case Async.State.Success: return Async.State.Failure;
                case Async.State.Failure: return Async.State.Success;
            }

            return Async.State.Running;
        }
    }

    [Serializable]
    public readonly struct Success : INode
    {
        private readonly INode Node;

        public Success(INode node)
        {
            Node = node;
        }

        public async Task<Async.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var state = await Node.OnTick(indices, root);
            return state == Async.State.Running ? Async.State.Running : Async.State.Success;
        }
    }

    [Serializable]
    public readonly struct Failure : INode
    {
        private readonly INode Node;

        public Failure(INode node)
        {
            Node = node;
        }

        public async Task<Async.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var state = await Node.OnTick(indices, root);
            return state == Async.State.Running ? Async.State.Running : Async.State.Failure;
        }
    }
}