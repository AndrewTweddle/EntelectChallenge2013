using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.MathFunctions
{
    public class UniformFunction: MathematicalFunction
    {
        public UniformFunction(double minX, double maxX, double value)
        {
            MinX = minX;
            MaxX = maxX;
            Value = value;
        }

        public double MinX { get; private set; }
        public double MaxX { get; private set; }
        public double Value { get; private set; }

        public override double Evaluate(double x)
        {
            if ((x >= MinX) && (x <= MaxX))
            {
                return Value;
            }
            else
            {
                return 0;
            }
        }
    }
}
