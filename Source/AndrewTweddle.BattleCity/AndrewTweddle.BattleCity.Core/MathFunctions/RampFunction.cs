using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.MathFunctions
{
    public class RampFunction: MathematicalFunction
    {
        public RampFunction(double leftX, double rightX, double minY, double maxY)
        {
            LeftX = leftX;
            RightX = rightX;
            MinY = minY;
            MaxY = maxY;
        }

        public double LeftX { get; set; }
        public double RightX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }

        public override double Evaluate(double x)
        {
            if (x <= LeftX)
            {
                return MinY;
            }
            if (x >= RightX)
            {
                return MaxY;
            }
            return MinY + (x - LeftX) / (RightX - LeftX) * (MaxY - MinY);
        }
    }
}
