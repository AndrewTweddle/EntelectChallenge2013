using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core
{
    [DataContract]
    public struct Point
    {
        [DataMember]
        public short X { get; private set; }

        [DataMember]
        public short Y { get; private set; }

        public Parity Parity
        {
            get
            {
                if (((X + Y) % 2) == 0)
                {
                    return Parity.Even;
                }
                else
                {
                    return Parity.Odd;
                }
            }
        }

        public Point(short x, short y): this()
        {
            X = x;
            Y = y;
        }

        public int BoardIndex
        {
            get
            {
                return Y * Game.Current.BoardWidth + X;
            }
        }

        public static Point ConvertBoardIndexToPoint(int boardIndex)
        {
            int x = boardIndex % Game.Current.BoardWidth;
            int y = boardIndex / Game.Current.BoardWidth;
            return new Point((short) x, (short) y);
        }

        public static Point operator *(int factor, Point offset)
        {
            return new Point((short)(factor * offset.X), (short)(factor * offset.Y));
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is Point)
            {
                Point other = (Point) obj;
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (((ushort)X) << 16) | (ushort)Y;
            }
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        public static Point operator+(Point point1, Point point2)
        {
            unchecked
            {
                return new Point((short)(point1.X + point2.X), (short)(point1.Y + point2.Y));
            }
        }

        public static Point operator -(Point point1, Point point2)
        {
            unchecked
            {
                return new Point((short)(point1.X - point2.X), (short)(point1.Y - point2.Y));
            }
        }

        public Point[] GetRelativePoints(Point[] offsets)
        {
            Point[] relativePoints = (Point[]) offsets.Clone();
            for (int i = 0; i < relativePoints.Length; i++)
            {
                unchecked
                {
                    relativePoints[i] = new Point((short)(X + relativePoints[i].X), (short)(Y + relativePoints[i].Y));
                }
            }
            return relativePoints;
        }
    }
}
