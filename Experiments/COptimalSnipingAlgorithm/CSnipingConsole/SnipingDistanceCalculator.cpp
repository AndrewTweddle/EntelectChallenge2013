#include "stdafx.h"
#include "SnipingDistanceCalculator.h"


SnipingDistanceCalculator::SnipingDistanceCalculator(void)
{
}


SnipingDistanceCalculator::~SnipingDistanceCalculator(void)
{
}

int SnipingDistanceCalculator::Calculate(bool walls[], int wallLength)
{
    int ticksTaken = 0;
    int tankPosition = -1;
    int targetPosition = wallLength;
    int indexOfNextWall = -1;
    int indexOfLastWall = -1;

    while (indexOfNextWall != targetPosition)
    {
        indexOfNextWall = targetPosition;
        for (int i = indexOfLastWall + 1; i < wallLength; i++)
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
