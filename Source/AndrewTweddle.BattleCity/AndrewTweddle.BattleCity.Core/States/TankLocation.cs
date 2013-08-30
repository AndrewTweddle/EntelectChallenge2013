using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Calculations;

namespace AndrewTweddle.BattleCity.Core.States
{
    public class TankLocation: Location
    {
        #region Public Properties

        public TankState[] TankStatesByDirection { get; private set; }
        public Rectangle TankBody { get; set; }
        public Rectangle InteriorOfTankBody { get; set; }
        public Segment[] InsideEdgesByDirection { get; private set; }
        public Segment[] OutsideEdgesByDirection { get; private set; }
        public Cell[,] CellsOnEdgeByDirectionAndEdgeOffset { get; private set; }
        public bool IsValid { get; set; }

        #endregion

        
        #region Constructors

        public TankLocation()
        {
            InsideEdgesByDirection = new Segment[Constants.RELEVANT_DIRECTION_COUNT];
            OutsideEdgesByDirection = new Segment[Constants.RELEVANT_DIRECTION_COUNT];
            TankStatesByDirection = new TankState[Constants.RELEVANT_DIRECTION_COUNT];

            CellsOnEdgeByDirectionAndEdgeOffset = new Cell[Constants.RELEVANT_DIRECTION_COUNT, Constants.SEGMENT_SIZE];
        }

        #endregion
    }
}
