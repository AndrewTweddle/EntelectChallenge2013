using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Collections;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Calculations.Distances
{
    // TODO: Complete CacheNode and compare performance (if there is time)...

    /// <summary>
    /// CacheNode is like Node except it stores all the fields instead of calculating them.
    /// It is a class not a struct (since Node is really just an embellished int!)
    /// </summary>
    public class CacheNode
    {
        private int mobilityId;
        private byte x;
        private byte y;
        private Direction dir;
        private ActionType actionType;

        public int MobilityId 
        {
            get
            {
                return mobilityId;
            }
            set
            {
                mobilityId = value;
                x = (byte) (MobilityId & 0xFF);
                y = (byte)((MobilityId & 0xFF00) >> 8);
                dir = (Direction) (MobilityId & 0x30000 >> 16);
                actionType = (ActionType)(MobilityId & 0xC0000 >> 18);
            }
        }

        byte X
        {
            get
            {
                return (byte) (mobilityId & 0xFF);
            }
            set
            {
                x = value;
                mobilityId = (mobilityId & (~0xFF)) | value;
            }
        }

        byte Y
        {
            get
            {
                return (byte)((MobilityId & 0xFF00) >> 8);
            }
            set
            {
                y = value;
                mobilityId = (mobilityId & (~0xFF00)) | (value << 8);
            }
        }

        Direction Dir 
        {
            get
            {
                return (Direction) (mobilityId & 0x30000 >> 16);
            }
            set
            {
                dir = value;
                mobilityId = (mobilityId & (~0x30000)) | ((int) value << 16);
            }
        }

        ActionType ActionType 
        {
            get
            {
                return (ActionType)(MobilityId & 0xC0000 >> 18);
            }
            set
            {
                actionType = value;
                mobilityId = (mobilityId & (~0xC0000)) | ((int) value << 18);
            }
        }

        public CacheNode(int mobilityId)
        {
            MobilityId = mobilityId;
        }

        public CacheNode(ActionType actionType, Direction dir, Point pos)
        {
            this.actionType = actionType;
            this.dir = dir;
            this.x = (byte) pos.X;
            this.y = (byte) pos.Y;
            this.mobilityId = ((byte)actionType << 18) | (byte)dir << 16 | (byte)pos.Y << 8 | (byte)pos.X;
        }

        public CacheNode(ActionType actionType, MobileState mobileState)
            : this(actionType, mobileState.Dir, mobileState.Pos)
        {
        }

        public Node[] GetAdjacentNodes(Matrix<SegmentState> segStateMatrix)
        {
            throw new NotImplementedException();
        }
    }
}
