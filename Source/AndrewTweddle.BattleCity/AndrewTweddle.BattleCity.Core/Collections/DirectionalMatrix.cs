using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;

namespace AndrewTweddle.BattleCity.Core.Collections
{
    public class DirectionalMatrix<T>
    {
        private T[] cells;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Length
        {
            get
            {
                return Width * Height * Constants.RELEVANT_DIRECTION_COUNT;
            }
        }

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

        public DirectionalMatrix(Point topLeft, int width, int height)
        {
            TopLeft = topLeft;
            Width = width;
            Height = height;
            cells = new T[Length];
        }

        public DirectionalMatrix(int width, int height): this(new Point(0,0), width, height)
        {
        }

        public DirectionalMatrix(): this(Game.Current.BoardWidth, Game.Current.BoardHeight)
        {
        }

        public T this[Direction dir, int x, int y]
        {
            get
            {
                return cells[4 * ((y - TopLeft.Y) * Width + (x - TopLeft.X)) + (int) dir];
            }
            set
            {
                cells[4 * ((y - TopLeft.Y) * Width + (x - TopLeft.X)) + (int)dir] = value;
            }
        }

        public T this[Direction dir, Point point]
        {
            get
            {
                return this[dir, point.X, point.Y];
            }
            set
            {
                this[dir, point.X, point.Y] = value;
            }
        }
    }
}
