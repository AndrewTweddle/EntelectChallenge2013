using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.TankMovement.Core
{
    public struct Rectangle
    {
        public Point TopLeft;
        public Point BottomRight;

        public Rectangle(int leftX, int topY, int rightX, int bottomY)
        {
            TopLeft = new Point { X = leftX, Y = topY };
            BottomRight = new Point { X = rightX, Y = bottomY };
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
            for (int x = TopLeft.X; x <= BottomRight.X; x++)
            {
                for (int y = TopLeft.Y; y <= BottomRight.Y; y++)
                {
                    yield return new Point(x, y);
                }
            }
        }
    }
}
