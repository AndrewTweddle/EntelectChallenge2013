using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Calculations
{
    public class Line<T>
    {
        public Point StartOfLine { get; set; }
        public Point EndOfLine
        {
            get
            {
                return StartOfLine + (Length - 1) * DirectionOfLine.GetOffset();
            }
        }
        public int Length { get; set; }
        public Direction DirectionOfLine { get; set; }
        Direction DirectionTowardsStartPoint
        {
            get
            {
                return DirectionOfLine.GetOpposite();
            }
        }

        public T[] Items { get; private set; }

        public T this[int index] 
        { 
            get
            {
                return Items[index];
            }
            set
            {
                Items[index] = value;
            }
        }

        public T this[Point point]
        {
            get
            {
                int index = ConvertPointToIndex(ref point);
                return this[index];
            }
            set
            {
                int index = ConvertPointToIndex(ref point);
                this[index] = value;
            }
        }

        public T this[int x, int y]
        {
            get
            {
                int index = ConvertCoordinatesToIndex(x, y);
                return this[index];
            }
            set
            {
                int index = ConvertCoordinatesToIndex(x, y);
                this[index] = value;
            }
        }

        public Line(Point startOfLine, Direction directionOfLine, int lengthOfLine)
        {
            Items = new T[lengthOfLine];
            StartOfLine = startOfLine;
            DirectionOfLine = directionOfLine;
            Length = lengthOfLine;
        }

        public int ConvertPointToIndex(ref Point point)
        {
            return ConvertCoordinatesToIndex(point.X, point.Y);
        }

        public int ConvertCoordinatesToIndex(int x, int y)
        {
            bool isError = false;
            Point offset = DirectionOfLine.GetOffset();
            int xDiff = x - StartOfLine.X;
            int yDiff = y - StartOfLine.Y;
            int index = 0;

            if (offset.X == 0)
            {
                if (xDiff > 0)
                {
                    isError = true;
                }
                else
                {
                    index = yDiff / offset.Y;
                }
            }
            else
            {
                if (yDiff > 0)
                {
                    isError = true;
                }
                else
                {
                    index = xDiff / offset.X;
                }
            }

            if (isError || index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException(
                    String.Format("Point ({0}, {1}) is not on the line from {2} to {3}", x, y, StartOfLine, EndOfLine));
            }
            return index;
        }
    }
}
