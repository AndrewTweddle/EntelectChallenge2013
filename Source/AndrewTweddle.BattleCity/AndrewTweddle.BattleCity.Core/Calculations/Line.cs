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

        public Line(Point startOfLine, Direction directionOfLine, int lengthOfLine)
        {
            Items = new T[lengthOfLine];
            StartOfLine = startOfLine;
            DirectionOfLine = directionOfLine;
        }

        public int ConvertPointToIndex(ref Point point)
        {
            bool isError = false;
            Point offset = DirectionOfLine.GetOffset();
            int xDiff = point.X - StartOfLine.X;
            int yDiff = point.Y - StartOfLine.Y;
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
                    String.Format("Point {0} is not on the line from {1} to {2}", point, StartOfLine, EndOfLine));
            }
            return index;
        }
    }
}
