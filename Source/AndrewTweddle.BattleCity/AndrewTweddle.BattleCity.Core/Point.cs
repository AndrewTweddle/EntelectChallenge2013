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

        public Direction GetHorizontalDirectionToPoint(Point targetPoint)
        {
            if (targetPoint.X > X)
            {
                return Direction.RIGHT;
            }

            if (targetPoint.X < X)
            {
                return Direction.LEFT;
            }
            
            return Direction.NONE;
        }

        public Direction GetVerticalDirectionToPoint(Point targetPoint)
        {
            if (targetPoint.Y > Y)
            {
                return Direction.DOWN;
            }

            if (targetPoint.Y < Y)
            {
                return Direction.UP;
            }

            return Direction.NONE;
        }

        public Point[] GetPointsOnZigZagLineToTargetPoint(Point targetPoint)
        {
            Direction horizDir = GetHorizontalDirectionToPoint(targetPoint);
            Direction vertDir = GetVerticalDirectionToPoint(targetPoint);
            int x = X;
            int y = Y;
            int xDiff = targetPoint.X - x;
            int yDiff = targetPoint.Y - y;
            Point[] points = new Point[Math.Abs(xDiff) + Math.Abs(yDiff)];
            int pointIndex = 0;
            Point currPoint = this;

            while (currPoint != targetPoint)
            {
                if (Math.Abs(xDiff) > Math.Abs(yDiff))
                {
                    currPoint += horizDir.GetOffset();
                }
                else
                {
                    currPoint += vertDir.GetOffset();
                }
                points[pointIndex] = currPoint;
                pointIndex++;

                xDiff = targetPoint.X - currPoint.X;
                yDiff = targetPoint.Y - currPoint.Y;
            }
            return points;
        }

        public Point BringIntoBounds(Rectangle bounds)
        {
            int newX = X;
            int newY = Y;
            if (bounds.TopLeft.X > X)
            {
                newX = bounds.TopLeft.X;
            }
            else
                if (bounds.BottomRight.X < X)
                {
                    newX = bounds.BottomRight.X;
                }
            if (bounds.TopLeft.Y > Y)
            {
                newY = bounds.TopLeft.Y;
            }
            else
                if (bounds.BottomRight.Y < Y)
                {
                    newY = bounds.BottomRight.Y;
                }
            return new Point((short) newX, (short) newY);
        }

        public BoundaryProximity GetClosestBoundary(Rectangle bounds)
        {
            int midX = (bounds.TopLeft.X + bounds.BottomRight.X) / 2;
            int midY = (bounds.TopLeft.Y + bounds.BottomRight.Y) / 2;
            int xOffset = X - midX;
            int yOffset = Y - midY;
            if (Math.Abs(xOffset) > Math.Abs(yOffset))
            {
                if (xOffset > 0)
                {
                    return new BoundaryProximity(Direction.RIGHT, (short) (bounds.BottomRight.X - X));
                }
                else
                {
                    return new BoundaryProximity(Direction.LEFT, (short) (X - bounds.TopLeft.X));
                }
            }
            else
            {
                if (yOffset > 0)
                {
                    return new BoundaryProximity(Direction.DOWN, (short) (bounds.BottomRight.Y - Y));
                }
                else
                {
                    return new BoundaryProximity(Direction.UP, (short) (Y - bounds.TopLeft.Y));
                }
            }
        }

        public Rectangle ToPointRectangle()
        {
            return new Rectangle(X, Y, X, Y);
        }
    }
}
