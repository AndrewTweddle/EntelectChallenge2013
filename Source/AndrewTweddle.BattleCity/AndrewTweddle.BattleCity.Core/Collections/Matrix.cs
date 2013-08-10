using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.Collections
{
    public class Matrix<T>
    {
        private T[] cells;

        public Point TopLeft { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Length
        {
            get
            {
                return Width * Height;
            }
        }

        public Matrix(Point topLeft, int width, int height)
        {
            TopLeft = topLeft;
            Width = width;
            Height = height;
            cells = new T[Length];
        }

        public Matrix(int width, int height): this(new Point(0,0), width, height)
        {
        }

        public Matrix(): this(Game.Current.BoardWidth, Game.Current.BoardHeight)
        {
        }

        public T this[int x, int y]
        {
            get
            {
                return cells[(y - TopLeft.Y) * Width + (x - TopLeft.X)];
            }
            set
            {
                cells[(y - TopLeft.Y) * Width + (x - TopLeft.X)] = value;
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
