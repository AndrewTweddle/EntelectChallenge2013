using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Core.MathFunctions
{
    public class ReverseLogisticFunction: MathematicalFunction
    {
        /// <summary>
        /// Roughly -6 for a standard logistic curve
        /// </summary>
        public double LeftAsymptoticX { get; private set; }
        /// <summary>
        /// Roughly +6 for a standard logistic curve
        /// </summary>
        public double RightAsymptoticX { get; private set; }
        public double MinAsymptoticY { get; private set; }
        public double MaxAsymptoticY { get; private set; }

        public double MidX { get; private set; }
        public double HalfX { get; private set; }
        public double VertRange { get; private set; }

        public ReverseLogisticFunction(
            double leftAsymptoticX, double rightAsymptoticX,
            double minAsymptoticY, double maxAsymptoticY)
        {
            LeftAsymptoticX = leftAsymptoticX;
            RightAsymptoticX = rightAsymptoticX;
            MinAsymptoticY = minAsymptoticY;
            MaxAsymptoticY = maxAsymptoticY;

            // Convenience properties:
            MidX = (LeftAsymptoticX + RightAsymptoticX) / 2.0;
            HalfX = (RightAsymptoticX - LeftAsymptoticX) / 2.0;
            VertRange = MaxAsymptoticY - MinAsymptoticY;
        }

        public override double Evaluate(double x)
        {
            return MinAsymptoticY + VertRange * ( 1.0 / (1 + System.Math.Exp(6 / HalfX * (x - MidX))));
        }
    }
}
