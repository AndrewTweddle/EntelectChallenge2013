using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.Elements
{
    public class Tank: Element
    {
        public int Id { get; set; }
        public Point InitialCentrePosition { get; set; }
        public Direction InitialDirection { get; set; }
        public TankAction InitialAction { get; set; }
        public Bullet Bullet { get; set; }

        public Tank()
        {
            Bullet = new Bullet(this);
        }
    }
}
