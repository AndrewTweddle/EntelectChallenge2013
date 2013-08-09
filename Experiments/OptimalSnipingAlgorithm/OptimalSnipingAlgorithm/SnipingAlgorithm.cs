using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndrewTweddle.ORToolkit.DynamicProgramming;

namespace OptimalSnipingAlgorithm
{
    /// <summary>
    /// The decision is whether to fire or not. If not firing then the tank moves one space.
    /// </summary>
    public class SnipingAlgorithm: DynamicProgramBase<SniperState, bool>
    {
        protected override IEnumerable<bool> GenerateDecisions(SniperState state)
        {
            if (state.TargetIsHit == true)
            {
                yield break;
            }
            yield return true;  // Firing is always an option

            // Don't move if right next to the enemy base. Just fire instead:
            if (state.TankPosition == state.Walls.Length - 1)
            {
                yield break;
            }

            // Don't move if a wall is in the way
            if (state.Walls[state.TankPosition + 1])
            {
                yield break;
            }
            
            // If there is an empty space then a wall, then always fire, 
            // as you end up in the same position as if you moved then fired. 
            // So this cuts down on unnecessary duplication:
            if ((state.TankPosition < state.Walls.Length - 2) && (!state.Walls[state.TankPosition + 1]) && state.Walls[state.TankPosition+2])
            {
                // Moving is only possible if the next space is not a wall
                yield break;
            }

            yield return false;
        }

        protected override SniperState GenerateNewState(SniperState state, bool decision, int stage)
        {
            SniperState newState = state.Clone();
            if (decision)
            {
                // Firing
                int bulletPosition = newState.TankPosition + 1;
                newState.TurnNumber++;
                AddStateString(newState, bulletPosition);

                if (bulletPosition == newState.Walls.Length)
                {
                    // Hit the base:
                    newState.TargetIsHit = true;
                    return newState;
                }

                if (newState.Walls[bulletPosition])
                {
                    // Destroy the wall immediately
                    newState.Walls[bulletPosition] = false;
                    return newState;
                }

                while (bulletPosition < newState.Walls.Length-1)
                {
                    newState.TurnNumber++;
                    newState.TankPosition++;
                    bulletPosition++;
                    if (newState.Walls[bulletPosition])
                    {
                        // Destroy the wall 
                        newState.Walls[bulletPosition] = false;
                        AddStateString(newState, bulletPosition);
                        return newState;
                    }
                    bulletPosition++;
                    if (bulletPosition == newState.Walls.Length)
                    {
                        AddStateString(newState, bulletPosition);
                        break;
                    }
                    if (newState.Walls[bulletPosition])
                    {
                        // Destroy the wall 
                        newState.Walls[bulletPosition] = false;
                        AddStateString(newState, bulletPosition);
                        return newState;
                    }
                    AddStateString(newState, bulletPosition);
                }
                newState.TargetIsHit = true;
                return newState;
            }
            else
            {
                // Move one space forward
                newState.TurnNumber++;
                newState.TankPosition++;
                AddStateString(newState);
                return newState;
            }
        }

        protected override double GetDecisionValue(SniperState priorState, bool decision, int stage)
        {
            // Calculate how many turns are used up by this decision:
            SniperState newState = priorState.Clone();
            if (decision)
            {
                // Firing
                int bulletPosition = newState.TankPosition + 1;
                newState.TurnNumber++;

                if (bulletPosition == newState.Walls.Length)
                {
                    return priorState.TurnNumber - newState.TurnNumber;
                }

                if (newState.Walls[bulletPosition])
                {
                    return priorState.TurnNumber - newState.TurnNumber;
                }

                while (bulletPosition < newState.Walls.Length - 1)
                {
                    newState.TurnNumber++;
                    newState.TankPosition++;
                    bulletPosition++;
                    if (newState.Walls[bulletPosition])
                    {
                        // Destroy the wall 
                        return priorState.TurnNumber - newState.TurnNumber;
                    }
                    bulletPosition++;
                    if (bulletPosition == newState.Walls.Length)
                    {
                        return priorState.TurnNumber - newState.TurnNumber;
                    }
                    if (newState.Walls[bulletPosition])
                    {
                        // Destroy the wall 
                        return priorState.TurnNumber - newState.TurnNumber;
                    }
                }
                return priorState.TurnNumber - newState.TurnNumber;
            }
            else
            {
                // Move one space forward
                return -1;
            }
        }

        protected override BranchStatus GetBranchStatus(SniperState state, int stage)
        {
            if (state.TargetIsHit)
            {
                return BranchStatus.Complete;
            }
            return BranchStatus.Incomplete;
        }

        private void AddStateString(SniperState state, int bulletPosition = -1)
        {
            char[] spaces = new char[state.Walls.Length + 2];
            spaces[0] = '-';
            spaces[state.Walls.Length + 1] = 'B';
            char[] wallSpaces = state.Walls.Select(isWall => isWall ? '#' : '-').ToArray();
            Array.Copy(wallSpaces, 0, spaces, 1, wallSpaces.Length);
            spaces[state.TankPosition + 1] = 'T';
            if (bulletPosition != -1)
            {
                spaces[bulletPosition + 1] = '*';
            }
            string stateString = String.Format("{0,3}: {1}", state.TurnNumber, new string(spaces));
            state.StateByTurn.Add(stateString);
        }

    }
}
