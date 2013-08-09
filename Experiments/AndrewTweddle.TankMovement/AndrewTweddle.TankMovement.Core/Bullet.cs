using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.TankMovement.Core
{
    public class Bullet
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Direction Direction { get; set; }

        public Bullet()
        {
        }
    }
}
