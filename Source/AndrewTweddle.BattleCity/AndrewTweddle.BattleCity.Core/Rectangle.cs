using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core
{
    [DataContract]
    public struct Rectangle
    {
        public static Rectangle Unrestricted { get; private set; }

        static Rectangle()
        {
            Unrestricted = new Rectangle(-300, -300, 300, 300);
        }

        [DataMember]
        public Point TopLeft { get; private set; }

        [DataMember]
        public Point BottomRight { get; private set; }

        public Rectangle(short leftX, short topY, short rightX, short bottomY): this()
        {
            TopLeft = new Point(leftX, topY);
            BottomRight = new Point(rightX, bottomY);
        }

        public Rectangle(Point topLeft, Point bottomRight): this()
        {
            TopLeft = topLeft;
            BottomRight = bottomRight;
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

        public Rectangle Merge(Rectangle rectangle)
        {
            return new Rectangle(
                (short)Math.Min(rectangle.TopLeft.X, TopLeft.X),
                (short)Math.Min(rectangle.TopLeft.Y, TopLeft.Y),
                (short)Math.Max(rectangle.BottomRight.X, BottomRight.X),
                (short)Math.Max(rectangle.BottomRight.Y, BottomRight.Y));
        }

        public Rectangle Expand(int expansion)
        {
            return Expand(expansion, expansion);
        }

        public Rectangle Expand(int horizontalExpansion, int verticalExpansion)
        {
            return new Rectangle(
                (short) (TopLeft.X - horizontalExpansion), 
                (short) (TopLeft.Y - verticalExpansion),
                (short) (BottomRight.X + horizontalExpansion),
                (short) (BottomRight.Y + verticalExpansion));
        }

        public Rectangle GetOuterEdgeInDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.UP:
                    return new Rectangle(TopLeft.X, (short)(TopLeft.Y - 1), BottomRight.X, (short) (TopLeft.Y - 1));
                case Direction.DOWN:
                    return new Rectangle(TopLeft.X, (short)(BottomRight.Y + 1), BottomRight.X, (short)(BottomRight.Y + 1));
                case Direction.LEFT:
                    return new Rectangle((short)(TopLeft.X - 1), TopLeft.Y, (short)(TopLeft.X - 1), BottomRight.Y);
                case Direction.RIGHT:
                    return new Rectangle((short)(BottomRight.X + 1), TopLeft.Y, (short)(BottomRight.X + 1), BottomRight.Y);
            }
            return this;
        }
    }
}
