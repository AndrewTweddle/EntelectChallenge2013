using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    /// <summary>
    /// Used to implement a breadth-first queue for shortest path calculations.
    /// Note that blocks of items are added with the same distance.
    /// Also the next block has a distance one greater than the previous block.
    /// And there will be at most two blocks in the queue at a time.
    /// So we don't need to store the values on each item.
    /// We can calculate them by tracking where the next block starts.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TwoValuedCircularBuffer<T>
    {
#if DEBUG
        public T[] elements;
#else
        private T[] elements;
#endif

        public int InsertionIndex { get; private set; }
        public int RemovalIndex { get; private set; }
        public int NextValueStartsAt { get; private set; }

        public int CurrentValue { get; private set; }
        public int Capacity { get; private set; }
        public int Size { get; private set; }

        protected TwoValuedCircularBuffer()
        {
        }

        public TwoValuedCircularBuffer(int capacity)
        {
            Capacity = capacity;
            Size = 0;
            CurrentValue = 0;
            elements = new T[capacity];
            NextValueStartsAt = -1;
        }

        public void Add(CircularBufferItem<T> bufferItem)
        {
            Add(bufferItem.Item, bufferItem.Value);
        }

        public void Add(T item, int value)
        {
            elements[InsertionIndex] = item;

            if (Size == 0)
            {
                CurrentValue = value;
                NextValueStartsAt = -1;
            }
            else
            {
                // Check that the values inserted are successive and sequential:
                if (NextValueStartsAt == -1)
                {
                    if (value == CurrentValue + 1)
                    {
                        NextValueStartsAt = InsertionIndex;
                    }
                    else
                        if (value != CurrentValue)
                        {
                            throw new ApplicationException("Only two successive values may be in the circular buffer at a time");
                        }
                }
                else
                    if (value != CurrentValue + 1)
                    {
                        throw new ApplicationException("The value inserted must be a successor to the value being removed from the circular buffer");
                    }
            }
            InsertionIndex = (InsertionIndex + 1) % Capacity;
            Size++;
            if (Size == Capacity)
            {
                IncreaseArrayCapacity();
            }
        }

        public CircularBufferItem<T> Remove()
        {
            if (Size == 0)
            {
                return null;
            }
            T item = elements[RemovalIndex];
            if (RemovalIndex == NextValueStartsAt)
            {
                CurrentValue++;
                NextValueStartsAt = -1;
            }
            int value = CurrentValue;
            RemovalIndex = (RemovalIndex + 1) % Capacity;
            Size--;
            return new CircularBufferItem<T>(item, value);
        }

        public CircularBufferItem<T> Peek()
        {
            if (Size == 0)
            {
                return null;
            }
            T item = elements[RemovalIndex];
            if (RemovalIndex == NextValueStartsAt)
            {
                return new CircularBufferItem<T>(item, CurrentValue + 1);
            }
            else
            {
                return new CircularBufferItem<T>(item, CurrentValue);
            }
        }

        private void IncreaseArrayCapacity()
        {
            // WARNING: Untested code!
            int oldCapacity = Capacity;
            Capacity = 2 * Capacity;
            Array.Resize(ref elements, Capacity);
            Array.Copy(elements, RemovalIndex, elements, RemovalIndex + oldCapacity, oldCapacity - RemovalIndex + 1);
                // It SHOULD be safe to copy back to the same array, 
                // because there is no overlap of slots (due to doubling capacity)

            if (NextValueStartsAt >= RemovalIndex)
            {
                NextValueStartsAt += oldCapacity;
            }
            RemovalIndex += oldCapacity;
        }
    }
}
