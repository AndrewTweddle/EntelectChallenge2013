using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.BattleCity.Experimental.CommandLine
{
    public static class DataGenerator
    {
        /// <summary>
        /// Generate random points on a board for testing purposes
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="pointCount"></param>
        /// <param name="seed">The random seed to use, or a negative number to randomize the seed</param>
        /// <returns></returns>
        public static Core.Point[] GenerateRandomPoints(Core.Point topLeft, Core.Point bottomRight, int pointCount, int seed = -1)
        {
            int width = bottomRight.X - topLeft.X + 1;
            int height = bottomRight.Y - topLeft.Y + 1;

            Random rnd;
            if (seed < 0)
            {
                rnd = new Random();
            }
            else
            {
                rnd = new Random(seed);
            }
            Core.Point[] randomPoints = new Core.Point[pointCount];
            for (int i = 0; i < randomPoints.Length; i++)
            {
                int randomX = topLeft.X + rnd.Next(width);
                int randomY = topLeft.Y + rnd.Next(height);
                Core.Point randomPoint = new Core.Point((short)randomX, (short)randomY);
                randomPoints[i] = randomPoint;
            }
            return randomPoints;
        }
    }
}
