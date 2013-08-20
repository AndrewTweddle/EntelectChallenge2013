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
    public class TwoValuedCircularBuffer<T>
    {
        private T[] elements;

        public int InsertionIndex { get; private set; }
        public int RemovalIndex { get; private set; }
        public int NextValueStartsAt { get; private set; }

        public int CurrentValue { get; private set; }
        public int Capacity { get; private set; }
        public int Size { get; private set; }

        public TwoValuedCircularBuffer(int capacity)
        {
            Capacity = capacity;
            Size = 0;
            CurrentValue = 0;
            elements = new T[capacity];
            NextValueStartsAt = -1;
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
            InsertionIndex = (InsertionIndex + 1) % Capacity;
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
            T item = elements[RemovalIndex];
            if (RemovalIndex == NextValueStartsAt)
            {
                CurrentValue++;
                NextValueStartsAt = -1;
            }
            int value = CurrentValue;
            RemovalIndex = (RemovalIndex + 1) % Capacity;
            Size--;
            return Tuple.Create(item, value);
        }

        public Tuple<T, int> Peek()
        {
            if (Size == 0)
            {
                return null;
            }
            T item = elements[RemovalIndex];
            if (RemovalIndex == NextValueStartsAt)
            {
                return Tuple.Create(item, CurrentValue + 1);
            }
            else
            {
                return Tuple.Create(item, CurrentValue);
            }
        }

        private void IncreaseArrayCapacity()
        {
            throw new NotImplementedException(
                "Due to time constraints, expansion of the circular buffer has not been implemented yet.");
        }
    }
}
