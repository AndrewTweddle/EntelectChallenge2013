using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public struct Rectangle
    {
        public Point TopLeft { get; private set; }
        public Point BottomRight { get; private set; }

        public Rectangle(short leftX, short topY, short rightX, short bottomY): this()
        {
            TopLeft = new Point(leftX, topY);
            BottomRight = new Point(rightX, bottomY);
        }

        public bool IntersectsWith(Rectangle other)
        {
            bool overlapsHorizontally = TopLeft.X <= other.BottomRight.X && BottomRight.X >= other.TopLeft.X;
            bool overlapsVertically = TopLeft.Y <= other.BottomRight.Y && BottomRight.Y >= other.TopLeft.Y;
            return overlapsHorizontally && overlapsVertically;
        }

        public bool ContainsPoint(Point point)
        {
            return (point.X >= TopLeft.X && point.X <= BottomRight.X && point.Y >= TopLeft.Y && point.Y <= BottomRight.Y);
        }

        public IEnumerable<Point> GetPoints()
        {
            for (short x = TopLeft.X; x <= BottomRight.X; x++)
            {
                for (short y = TopLeft.Y; y <= BottomRight.Y; y++)
                {
                    yield return new Point(x, y);
                }
            }
        }

        public static bool operator ==(Rectangle r1, Rectangle r2)
        {
            return r1.TopLeft == r2.TopLeft && r1.BottomRight == r2.BottomRight;
        }

        public static bool operator !=(Rectangle r1, Rectangle r2)
        {
            return !(r1 == r2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is Rectangle)
            {
                Rectangle other = (Rectangle)obj;
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return TopLeft.GetHashCode() ^ BottomRight.GetHashCode();
        }
    }
}
