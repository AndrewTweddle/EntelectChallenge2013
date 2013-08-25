using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core.Collections
{
    [DataContract]
    public class Matrix<T>
    {
        [DataMember]
        private T[] cells;

        [DataMember]
        public int Width { get; private set; }

        [DataMember]
        public int Height { get; private set; }

        public int Length
        {
            get
            {
                return Width * Height;
            }
        }

        [DataMember]
        public Point TopLeft { get; private set; }

        public Point BottomRight
        {
            get
            {
                int bottomRightX = TopLeft.X + Width - 1;
                int bottomRightY = TopLeft.Y + Height - 1;
                return new Point((short) bottomRightX, (short) bottomRightY);
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
