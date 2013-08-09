using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.Collections
{
    public class Matrix<T> where T:new()
    {
        private T[] cells;

        public short Width { get; private set; }
        public short Height { get; private set; }
        public int Length
        {
            get
            {
                return Width * Height;
            }
        }

        public Matrix(short width, short height)
        {
            Width = width;
            Height = height;
            cells = new T[Length];
        }

        public Matrix(): this(Game.Current.BoardWidth, Game.Current.BoardHeight)
        {
        }

        public T this[short x, short y]
        {
            get
            {
                return cells[y * Width + x];
            }
            set
            {
                cells[y * Width + x] = value;
            }
        }

        public T this[Point point]
        {
            get
            {
                return this[point.X, point.Y];
            }
            set
            {
                this[point.X, point.Y] = value;
            }
        }
    }
}
