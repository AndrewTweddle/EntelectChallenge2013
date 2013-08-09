using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core
{
    public class TankState
    {
        public Direction Direction { get; set; }
        public BulletState FiredBullet { get; set; }
        public int CentreX { get; set; }
        public int CentreY { get; set; }

        public bool HasFired 
        {
            get
            {
                return FiredBullet != null && FiredBullet.IsFired;
            }
        }

        public int TopLeftX
        {
            get
            {
                return CentreX - 2;
            }
        }
        public int TopLeftY
        {
            get
            {
                return CentreY - 2;
            }
        }
    }
}
