using System;

namespace AndrewTweddle.TankMovement.Core
{
    public class MoveRequest
    {
        #region Input Properties

        public Unit Tank { get; set; }

        #endregion

        #region Calculated Properties

        public Direction NewDirection { get; set; }  // This is so that unit events will still get updated if the direction is different from before
        public Point NewPosition { get; set; }
        public Point OldPosition { get; set; }
        public Rectangle NewLeadingEdge { get; set; }
        public Rectangle UnchangedBody { get; set; }
        public Rectangle OldTrailingEdge { get; set; }
        public MoveStatus Status { get; set; }

        #endregion

        protected MoveRequest()
        {
        }

        public MoveRequest(Unit tank)
        {
            Tank = tank;
            CalculateProperties();
        }

        private void CalculateProperties()
        {
            if (Tank == null)
            {
                Status = MoveStatus.TankDoesNotExist;
            }
            else
            {
                Status = MoveStatus.Unresolved;
                OldPosition = new Point(Tank.X, Tank.Y);

                Rectangle currBody = new Rectangle(Tank.X - 2, Tank.Y - 2, Tank.X + 2, Tank.Y + 2);

                switch (Tank.Action)
                {
                    case Action.UP:
                        NewPosition     = new Point(Tank.X, Tank.Y - 1);
                        NewLeadingEdge  = new Rectangle( currBody.TopLeft.X, currBody.TopLeft.Y - 1, currBody.BottomRight.X, currBody.TopLeft.Y - 1     );
                        UnchangedBody   = new Rectangle( currBody.TopLeft.X, currBody.TopLeft.Y    , currBody.BottomRight.X, currBody.BottomRight.Y - 1 );
                        OldTrailingEdge = new Rectangle( currBody.TopLeft.X, currBody.BottomRight.Y, currBody.BottomRight.X, currBody.BottomRight.Y     );
                        NewDirection = Direction.UP;
                        break;
                    case Action.DOWN:
                        NewPosition     = new Point(Tank.X, Tank.Y + 1);
                        NewLeadingEdge  = new Rectangle( currBody.TopLeft.X, currBody.BottomRight.Y + 1, currBody.BottomRight.X, currBody.BottomRight.Y + 1 );
                        UnchangedBody   = new Rectangle( currBody.TopLeft.X, currBody.TopLeft.Y + 1    , currBody.BottomRight.X, currBody.BottomRight.Y     );
                        OldTrailingEdge = new Rectangle( currBody.TopLeft.X, currBody.TopLeft.Y        , currBody.BottomRight.X, currBody.TopLeft.Y         );
                        NewDirection = Direction.DOWN;
                        break;
                    case Action.LEFT:
                        NewPosition     = new Point(Tank.X - 1, Tank.Y);
                        NewLeadingEdge  = new Rectangle( currBody.TopLeft.X - 1, currBody.TopLeft.Y, currBody.TopLeft.X - 1    , currBody.BottomRight.Y );
                        UnchangedBody   = new Rectangle( currBody.TopLeft.X    , currBody.TopLeft.Y, currBody.BottomRight.X - 1, currBody.BottomRight.Y );
                        OldTrailingEdge = new Rectangle( currBody.BottomRight.X, currBody.TopLeft.Y, currBody.BottomRight.X    , currBody.BottomRight.Y );
                        NewDirection = Direction.LEFT;
                        break;
                    case Action.RIGHT:
                        NewPosition     = new Point(Tank.X + 1, Tank.Y);
                        NewLeadingEdge  = new Rectangle( currBody.BottomRight.X + 1, currBody.TopLeft.Y, currBody.BottomRight.X + 1, currBody.BottomRight.Y);
                        UnchangedBody   = new Rectangle( currBody.TopLeft.X + 1    , currBody.TopLeft.Y, currBody.BottomRight.X    , currBody.BottomRight.Y);
                        OldTrailingEdge = new Rectangle( currBody.TopLeft.X        , currBody.TopLeft.Y, currBody.TopLeft.X        , currBody.BottomRight.Y);
                        NewDirection = Direction.RIGHT;
                        break;
                    default:
                        NewPosition = OldPosition;
                        UnchangedBody = currBody;
                        NewDirection = Tank.Direction;
                        Status = MoveStatus.NotMoving;
                        break;
                }
            }
        }
    }
}
