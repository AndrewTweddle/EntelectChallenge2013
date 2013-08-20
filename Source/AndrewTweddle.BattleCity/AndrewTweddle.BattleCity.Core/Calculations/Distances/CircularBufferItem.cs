using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    public class CircularBufferItem<T>
    {
        public T Item { get; private set; }
        public int Value { get; private set; }

        protected CircularBufferItem()
        {
        }

        public CircularBufferItem(T item, int value)
        {
            Item = item;
            Value = value;
        }
    }
}
