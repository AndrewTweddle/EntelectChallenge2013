using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OptimalSnipingAlgorithm
{
    public static class SnipingDistanceCalculator
    {
        public static int Calculate(bool[] walls)
        {
            unchecked
            {
                int ticksTaken = 0;
                int tankPosition = -1;
                int targetPosition = walls.Length;
                int indexOfNextWall = -1;
                int indexOfLastWall = -1;

                while (indexOfNextWall != targetPosition)
                {
                    indexOfNextWall = targetPosition;
                    for (int i = indexOfLastWall + 1; i < walls.Length; i++)
                    {
                        if (walls[i])
                        {
                            indexOfNextWall = i;
                            break;
                        }
                    }

                    ticksTaken += 1 + ((indexOfNextWall - tankPosition) >> 1);
                    tankPosition += (indexOfNextWall - tankPosition) >> 1;
                    indexOfLastWall = indexOfNextWall;
                }
                return ticksTaken;
            }
        }

        // Slower than version above:
        public unsafe static int PointerCalculate(bool[] walls)
        {
            unchecked
            {
                int ticksTaken = 0;
                int tankPosition = -1;
                int targetPosition = walls.Length;
                int indexOfNextWall = -1;
                int indexOfLastWall = -1;

                while (indexOfNextWall != targetPosition)
                {
                    indexOfNextWall = targetPosition;
                    fixed (bool* lastWallPointer = &walls[indexOfLastWall + 1])
                    {
                        bool* nextCellPointer = lastWallPointer;
                        for (int i = indexOfLastWall + 1; i < targetPosition; i++)
                        {
                            if (*(nextCellPointer++))
                            {
                                indexOfNextWall = i;
                                break;
                            }
                        }
                    }

                    ticksTaken += 1 + ((indexOfNextWall - tankPosition) >> 1);
                    tankPosition += (indexOfNextWall - tankPosition) >> 1;
                    indexOfLastWall = indexOfNextWall;
                }
                return ticksTaken;
            }
        }

    }
}
