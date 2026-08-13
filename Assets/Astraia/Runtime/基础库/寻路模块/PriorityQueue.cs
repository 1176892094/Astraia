using System;
using System.Collections.Generic;

namespace Astraia
{
    [Serializable]
    internal sealed class PriorityQueue
    {
        private readonly List<int> heap = new();
        private readonly int[] score;

        public PriorityQueue(int[] score)
        {
            this.score = score;
        }

        public int Count => heap.Count;

        public void Enqueue(int index)
        {
            heap.Add(index);

            var i = heap.Count - 1;

            while (i > 0)
            {
                var parent = (i - 1) >> 1;

                if (score[heap[parent]] <= score[heap[i]])
                {
                    break;
                }

                (heap[parent], heap[i]) = (heap[i], heap[parent]);
                i = parent;
            }
        }

        public int Dequeue()
        {
            var root = heap[0];

            var last = heap[^1];
            heap.RemoveAt(heap.Count - 1);

            if (heap.Count == 0)
            {
                return root;
            }

            heap[0] = last;

            var i = 0;

            while (true)
            {
                var left = i * 2 + 1;

                if (left >= heap.Count)
                {
                    break;
                }

                var right = left + 1;

                var smallest = left;

                if (right < heap.Count && score[heap[right]] < score[heap[left]])
                {
                    smallest = right;
                }

                if (score[heap[i]] <= score[heap[smallest]])
                {
                    break;
                }

                (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
                i = smallest;
            }

            return root;
        }

        public void Clear()
        {
            heap.Clear();
        }
    }
}