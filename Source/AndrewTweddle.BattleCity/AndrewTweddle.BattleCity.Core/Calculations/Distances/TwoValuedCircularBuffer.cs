using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public class CircularBuffer<T>
    {
        private T[] elements;

        private int insertionIndex;
        private int removalIndex;
        private int nextValueStartsAt = -1;

        public int CurrentValue { get; private set; }
        public int Capacity { get; private set; }
        public int Size { get; private set; }

        public CircularBuffer(int capacity)
        {
            Capacity = capacity;
            Size = 0;
            CurrentValue = 0;
            elements = new T[capacity];
        }

        public void Add(T item, int value)
        {
            elements[insertionIndex] = item;

            if (Size == 0)
            {
                CurrentValue = value;
                nextValueStartsAt = -1;
            }
            else
            {
                // Check that the values inserted are successive and sequential:
                if (nextValueStartsAt == -1)
                {
                    if (value == CurrentValue + 1)
                    {
                        nextValueStartsAt = insertionIndex;
                    }
                    else
                        if (value != CurrentValue)
                        {
                            throw new ArgumentException(
                                "Only two successive values may be in the circular buffer at a time",
                                "value");
                        }
                }
                else
                {
                    if (value != CurrentValue + 1)
                    {
                        throw new ArgumentException(
                            "The value inserted must be a successor to the value being removed from the circular buffer",
                            "value");
                    }
                }
            }
            insertionIndex = (insertionIndex + 1) % Capacity;
            Size++;
            if (Size > Capacity)
            {
                IncreaseArrayCapacity();
            }
        }

        public Tuple<T, int> Remove()
        {
            if (Size == 0)
            {
                return null;
            }
            T item = elements[removalIndex];
            if (removalIndex == nextValueStartsAt)
            {
                CurrentValue++;
                nextValueStartsAt = -1;
            }
            int value = CurrentValue;
            removalIndex = (removalIndex + 1) % Capacity;
            Size--;
            return Tuple.Create(item, value);
        }

        private void IncreaseArrayCapacity()
        {
            throw new NotImplementedException(
                "Due to time constraints, expansion of the circular buffer has not been implemented yet.");
        }
    }
}
