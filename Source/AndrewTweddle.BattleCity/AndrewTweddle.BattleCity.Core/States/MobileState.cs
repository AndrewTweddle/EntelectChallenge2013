using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AndrewTweddle.BattleCity.Core.States
{
    [DataContract]
    public struct MobileState
    {
        [DataMember]
        public Point Pos { get; private set; }

        [DataMember]
        public Direction Dir { get; private set; }

        [DataMember]
        public bool IsActive { get; private set; }

        public MobileState(int x, int y, Direction dir, bool isActive)
            : this(new Point((short) x, (short) y), dir, isActive)
        {
        }

        public MobileState(Point pos, Direction dir, bool isActive): this()
        {
            this.Pos = pos;
            this.Dir = dir;
            this.IsActive = isActive;
        }

        public MobileState Clone()
        {
            MobileState clonedState = new MobileState
            {
                Pos = this.Pos,
                Dir = this.Dir,
                IsActive = this.IsActive
            };
            return clonedState;
        }

        public MobileState Move()
        {
            return Move(Dir);
        }

        public MobileState Move(Direction dir)
        {
            return MoveTo(Pos + Dir.GetOffset());
        }

        public MobileState MoveTo(Point newPos)
        {
            return new MobileState
            {
                Pos = newPos,
                Dir = Dir,
                IsActive = IsActive
            };
        }

        public MobileState MoveTo(int x, int y)
        {
            return MoveTo(new Point((short) x, (short) y));
        }

        public MobileState ChangeDirection(Direction newDir)
        {
            if (newDir != Dir)
            {
                return new MobileState
                {
                    Pos = Pos,
                    Dir = newDir,
                    IsActive = IsActive
                };
            }
            else
            {
                return this;
            }
        }

        public MobileState Kill()
        {
            return new MobileState
            {
                Pos = Pos,
                Dir = Dir,
                IsActive = false
            };
        }

        public Rectangle GetTankExtent()
        {
            int topLeftX, bottomRightX, topLeftY, bottomRightY;
            topLeftX = Pos.X - Constants.TANK_EXTENT_OFFSET;
            topLeftY = Pos.Y - Constants.TANK_EXTENT_OFFSET;
            bottomRightX = Pos.X + Constants.TANK_EXTENT_OFFSET;
            bottomRightY = Pos.Y + Constants.TANK_EXTENT_OFFSET;
            return new Rectangle(
                (short)topLeftX, (short)topLeftY,
                (short)bottomRightX, (short)bottomRightY);
        }

        public MobileState FireABulletAndGetItsState()
        {
            int bulletX = Pos.X;
            int bulletY = Pos.Y;

            switch (Dir)
            {
                case Direction.UP:
                    bulletY = bulletY - Constants.TANK_EXTENT_OFFSET - 1;
                    break;
                case Direction.DOWN:
                    bulletY = bulletY + Constants.TANK_EXTENT_OFFSET + 1;
                    break;
                case Direction.LEFT:
                    bulletX = bulletX - Constants.TANK_EXTENT_OFFSET - 1;
                    break;
                case Direction.RIGHT:
                    bulletX = bulletX + Constants.TANK_EXTENT_OFFSET + 1;
                    break;
                default:
                    throw new InvalidOperationException("A tank can't fire a bullet if its direction has not been set");
            }

            return new MobileState
            {
                Dir = this.Dir,
                Pos = new Point((short) bulletX, (short) bulletY),
                IsActive = true
            };
        }

        public static bool operator==(MobileState one, MobileState other)
        {
            if (one.IsActive)
            {
                return (one.Pos == other.Pos) && (one.Dir == other.Dir) && (other.IsActive == true);
            }
            else
            {
                return !other.IsActive;
            }
        }

        public static bool operator !=(MobileState one, MobileState other)
        {
            return !(one == other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is MobileState))
            {
                return false;
            }
            return this == (MobileState)obj;
        }

        public override int GetHashCode()
        {
            if (IsActive)
            {
                return Pos.GetHashCode() ^ (((int)Dir) << 16);
            }
            else
            {
                return Pos.GetHashCode() ^ (((int)Dir) << 16) ^ (1 << 20);
            }
        }
    }
}
