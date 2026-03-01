using System;
using System.Collections.Generic;

namespace bkTools
{
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private List<(TElement Element, TPriority Priority)> _heap = new List<(TElement, TPriority)>();

        public int Count => _heap.Count;

        public void Enqueue(TElement element, TPriority priority)
        {
            _heap.Add((element, priority));
            HeapifyUp(_heap.Count - 1);
        }

        public TElement Dequeue()
        {
            if (Count == 0) throw new InvalidOperationException("The priority queue is empty.");

            TElement rootElement = _heap[0].Element;
            _heap[0] = _heap[Count - 1];
            _heap.RemoveAt(Count - 1);
            HeapifyDown(0);

            return rootElement;
        }

        public TElement Peek()
        {
            if (Count == 0) throw new InvalidOperationException("The priority queue is empty.");
            return _heap[0].Element;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_heap[index].Priority.CompareTo(_heap[parentIndex].Priority) >= 0) break;

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            int lastIndex = _heap.Count - 1;
            while (index < lastIndex)
            {
                int leftChildIndex = 2 * index + 1;
                int rightChildIndex = 2 * index + 2;

                if (leftChildIndex > lastIndex) break;

                int smallerChildIndex = (rightChildIndex <= lastIndex &&
                                         _heap[rightChildIndex].Priority.CompareTo(_heap[leftChildIndex].Priority) < 0)
                    ? rightChildIndex
                    : leftChildIndex;

                if (_heap[index].Priority.CompareTo(_heap[smallerChildIndex].Priority) <= 0) break;

                Swap(index, smallerChildIndex);
                index = smallerChildIndex;
            }
        }

        private void Swap(int i, int j)
        {
            (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
        }

        public bool TryGetPriority(TElement element, out TPriority existingPriority)
        {
            for (int i = 0; i < _heap.Count; i++)
            {
                if (EqualityComparer<TElement>.Default.Equals(_heap[i].Element, element))
                {
                    existingPriority = _heap[i].Priority;
                    return true;
                }
            }

            existingPriority = default(TPriority);
            return false;
        }

        public bool UpdatePriority(TElement element, TPriority newPriority)
        {
            for (int i = 0; i < _heap.Count; i++)
            {
                if (EqualityComparer<TElement>.Default.Equals(_heap[i].Element, element))
                {
                    if (_heap[i].Priority.CompareTo(newPriority) != 0)
                    {
                        _heap[i] = (element, newPriority);
                        if (newPriority.CompareTo(_heap[(i - 1) / 2].Priority) < 0)
                        {
                            HeapifyUp(i);
                        }
                        else
                        {
                            HeapifyDown(i);
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _heap.Clear();
        }
    }
}