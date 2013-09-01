using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;

namespace AndrewTweddle.BattleCity.Core.Calculations.Bullets
{
    public class BulletThreat
    {
        public Tank FiringTank { get; set; }
        public Tank TankThreatened { get; set; }
        public Node[] NodePathToTakeOnBullet { get; set; }
        public Node[] LateralMoveInOneDirection { get; set; }
        public Node[] LateralMoveInOtherDirection { get; set; }
    }
}
