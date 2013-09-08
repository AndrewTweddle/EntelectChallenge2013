using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.MathFunctions
{
    public class TriangularFunction: MathematicalFunction
    {
        public double StartX { get; set; }
        public double ModeX { get; set; }
        public double EndX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }

        public TriangularFunction(double startX, double modeX, double endX, double maxY, double minY = 0)
        {
            StartX = startX;
            ModeX = modeX;
            EndX = endX;
            MaxY = maxY;
            MinY = minY;
        }

        public override double Evaluate(double x)
        {
            if (x < StartX || x > EndX)
            {
                return MinY;
            }
            if (x <= ModeX)
            {
                return MinY + (x - StartX) / (ModeX - StartX) * (MaxY - MinY);
            }
            return MinY + (EndX - x) / (EndX - ModeX) * (MaxY - MinY);
        }
    }
}
