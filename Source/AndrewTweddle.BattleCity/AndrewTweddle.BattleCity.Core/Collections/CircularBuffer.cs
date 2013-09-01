using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Collections
{
    public class CircularBuffer<T>
    {
#if DEBUG
        public T[] elements;
#else
        private T[] elements;
#endif

        public int InsertionIndex { get; private set; }
        public int RemovalIndex { get; private set; }

        public int Capacity { get; private set; }
        public int Size { get; private set; }

        protected CircularBuffer()
        {
        }

        public CircularBuffer(int capacity)
        {
            Capacity = capacity;
            Size = 0;
            elements = new T[capacity];
        }

        public void Append(T item)
        {
            elements[InsertionIndex] = item;
            InsertionIndex = (InsertionIndex + 1) % Capacity;
            Size++;
            if (Size == Capacity)
            {
                DoubleArrayCapacity();
            }
        }

        public T this[int index]
        {
            get
            {
                return elements[(Capacity + index - RemovalIndex) % Capacity];
            }
            set
            {
                elements[(Capacity + index - RemovalIndex) % Capacity] = value;
            }
        }

        public T RemoveFirst()
        {
            if (Size == 0)
            {
                throw new InvalidOperationException("The circular buffer is empty, so nothing can be removed from it");
            }
            T item = elements[RemovalIndex];
            RemovalIndex = (RemovalIndex + 1) % Capacity;
            Size--;
            return item;
        }

        private void DoubleArrayCapacity()
        {
            // WARNING: Untested code!
            int oldCapacity = Capacity;
            Capacity = 2 * Capacity;
            Array.Resize(ref elements, Capacity);
            Array.Copy(elements, RemovalIndex, elements, RemovalIndex + oldCapacity, oldCapacity - RemovalIndex + 1);
                // It SHOULD be safe to copy back to the same array, 
                // because there is no overlap of slots (due to doubling capacity)

            RemovalIndex += oldCapacity;
        }
    }
}
