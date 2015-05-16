# Entelect AI Challenge 2013
=========================

# Overview

This was my competition entry for the [2013 Entelect R100k Challenge](http://challenge.entelect.co.za/challenge-history-2013).

The theme was inspired by the 1980's tank warfare game, Battle City. A brief summary of the rules follows...

There are 8 fixed boards ranging in size from about 61x61 to 81x81. 
Each player has 2 tanks. 
A play wins by either shooting or driving over the enemy base. 

Bullets move at twice the speed of tanks, and tanks can shoot each other and shoot through walls.
A tank can only have one bullet in play at a time. 
So the tank is effectively disarmed until its bullet hits a wall, another tank, a base or the edge of the board. 

A tank either moves or fires a bullet in its current direction. It can't do both at the same time.
It can move again after its bullet has been fired, but before the bullet hits a target.

Tanks occupy a 5x5 area. 
Bases and bullets occupy a single cell. 
When a wall is shot, the two walls on either side are also destroyed (thus allowing a tank to shoot a path through the walls).
If tanks try to move into a wall or another tank, the tank will turn but not move. 

Both players move simultaneously and there are 3 seconds between turns.
The players communicate with the game engine via SOAP web service calls.

The entry has to be written in Java or in a standard Visual Studio language supported by msbuild (usually C# or C++). I chose C#.

I started working on my competition entry on 20th July, 2013 and final submission was just over 7 weeks later on the 9th September.

# Interesting algorithms

## Shortest path algorithms

### Overview of tank movement rules

Tanks can move in one of four directions or fire in the direction they are currently facing. 
If there is a wall in the way of the tank, then the tank will turn but not move. 
A single shot will then remove a section of wall the width of the tank (i.e. 5 cells wide).
If there is no wall in the way of the tank, then the tank will simultaneously change direction and move.

This means that it could take between 1 and 3 turns for a tank to move to an adjacent cell. 
For example, imagine the tank is facing north, but wishes to move one space west, and there is a wall immediately to the west.
Then the tank will need to issue a "MOVE WEST" command to turn to face west. 
Then it will need to issue a "FIRE" command to shoot the wall away.
Finally it will need to issue another "MOVE WEST" command to move into the vacated space.

### A graph structure to simplify the implementation of the Dijkstra algorithm

One option was to have a node per cell and use a priority queue to implement Dijkstra's algorithm.

Instead I created a graph with a separate node for each combination of a cell on the board + a direction (i.e. the direction the tank is currently facing).
Additionally, for each cell on the board, I added nodes for firing actions in each direction where the tank was blocked by a wall.
The resulting graph has 4 to 7 times as many nodes, but the distance between adjacent edges is always 1.
This allows a standard queue (implemented as a circular buffer) to be used to track nodes which still need to be visited.
New nodes can then be added to the end of the queue instead of needing to be inserted into the correct order in a priority queue.

Actually there are two types of graph structure.
One is used when calculating the matrix of distances from a single point (usually the position of a tank) to all cells on the board.
The other is used when calculating the matrix of distances from all cells on the board to a single point (typically the enemy base or an enemy tank).

### The node data structure

The [Node data structure](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core/Calculations/Distances/Node.cs)
is essentially just a convenient wrapper around an integer.
Bit manipulation is used to extract properties such as X and Y coordinates, direction, whether a firing or moving node, and so on.

The `GetAdjacentOutgoingNodes()`, `GetAdjacentOutgoingNodesWithoutFiring()` and `GetAdjacentIncomingNodes()` methods are used to calculate adjacent nodes for the two types of graphs.
These methods take an array of ["SegmentStates"](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core/SegmentState.cs) 
which indicate what the state of the segment of wall is in each direction.

### The distance matrix

The distance matrix is a [DirectionalMatrix](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core/Collections/DirectionalMatrix.cs)
storing [DistanceCalculation](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core/Calculations/Distances/DistanceCalculation.cs) 
entries for each movement node (defined by its cell coordinates and the direction the tank is facing).

It does not store a distance calculation for "firing" nodes, since shooting away a wall is not interesting in itself. It is just an interim action to allow further movement.

### The circular buffer

The [TwoValuedCircularBuffer](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core/Calculations/Distances/TwoValuedCircularBuffer.cs)
class is a queue implemented as an array with an insertion and removal index. These wrap around when the end of the array is reached.

I added one extra nuance to a standard ring buffer implementation.
When nodes are initially added to the circular buffer, they will be a distance of 1 away from the original node. 
When a node is removed from the buffer, its adjacent nodes will be calculated. 
Any of these nodes which haven't been previously visited will be added to the end of the buffer with a distance of 2 from the original node.
At this point all of the nodes in the buffer will have a distance of 1 except for the newly added nodes, which will all have a distance of 2.

As this process is repeated, the nodes in the buffer will only ever have one of 2 consecutive distance values. 
It is a waste of space to store these distances with every node.
Instead the circular buffer keeps track of the smaller of the two distance values, and the index in the array at which the next higher value starts.
When this index is reached during removal of the next node, the distance value will be incremented and the index will be updated to be the next insertion point in the buffer.

### The distance calculation algorithms

The [DistanceCalculator](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core/Calculations/Distances/DistanceCalculator.cs)
class calculates a matrix of distances from a particular tank position to every cell on the board.

The [AttackTargetDistanceCalculator](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core/Calculations/Distances/AttackTargetDistanceCalculator.cs)
class calculates a matrix of distances from every cell on the board to a particular position 
(usually the position of the enemy base, but possibly also the position of an enemy tank).

### Taboo regions and restricted regions

Optional taboo regions and restricted regions can be configured for the distance calculators.

For example, the region occupied by one's own base or the other friendly tank are both taboo.
You wouldn't want to commit suicide by accidentally riding over your own base or shooting your other tank, so these regions are made taboo.

Restricted regions allow one to speed up calculations by only calculating distances over a subset of the board.

### Firing distances

When attacking an enemy base, you can destroy it by moving onto it. 
But more typically you will just shoot it from a distance (if it is unprotected), since bullets move at twice the speed of tanks.

The distance calculation algorithms include optional modifications to take into account the shortest number of steps when you are able to shoot the target from a distance.
These use pre-calculated "firing lines" radiating out from the target in each of the 4 directions and 5 spaces wide 
(the width of the target base or tank, since a bullet can destroy an enemy target anywhere along its edge).
The following section discusses the algorithm for calculating these firing lines.

## The firing distance calculator

It's important to note that each tank can have at most one active bullet in play at a time.
Bullets move at 2 spaces per turn, tanks at 1 space per turn.
When you choose to shoot away a piece of wall that is between your tank and the target, 
then you might as well move your tank closer to the target while the current bullet is in motion.
That way your next bullet has less distance to travel.

There is an interesting and counter-intuitive consequence of these rules, which I discovered while trying to debug my algorithm... only to discover that it was not a bug after all!

Since bullets move twice as fast as tanks, your tank will halve its distance to the bullet's current target by the time that target is hit.
When an enemy base is protected by a few layers of bricks, you don't actually get much benefit from shooting from a distance.
You halve your remaining distance to the enemy base with each cell-wide layer of protective bricks that you have to shoot away.
Halve that distance a number of times because of the number of protective layers of bricks, and your tank will probably be next to the final layer of bricks when it shoots them away.
You might just as well have moved right up to the bricks and shot them away from close range.

The real benefit of the firing distance calculator is when there are bricks which are blocking you from getting any closer to the base, but which you can't shoot away.
This happens when the centre brick of a 5 brick wide wall segment is missing, but some of the adjacent bricks are still there.
The rules of the game require you to shoot the centre brick for the adjacent bricks to be destroyed.
Since you can't shoot the adjacent bricks out your way, you are able to shoot through the hole in the wall, but not move through it.

The actual [firing distance calculator](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core/Calculations/Firing/FiringDistanceCalculator.cs)
is quite complicated, due to complexities such as these walls which you can shoot through, but not move through or shoot out the way.

Additionally, you want to be able to ignore shooting positions which start with movement closer to the target or with a shot into a wall at point blank range.
This is because you can use the normal distance calculator to reach these same positions.
So you don't want to waste time calculating all combinations of normal movement and firing line movement if the resulting combined path is identical.
The first firing line action should be a shot from a distance.

A simpler, early experimental version of the calculator can be found [here in C#](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Experiments/OptimalSnipingAlgorithm/OptimalSnipingAlgorithm/SnipingDistanceCalculator.cs)
or [here in C++](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Experiments/COptimalSnipingAlgorithm/CSnipingConsole/SnipingDistanceCalculator.cpp).
I wrote both, since I wanted to compare the performance of the two languages to decide whether it was worth implementing parts of the code in C++ rather than C#.
I decided to go with a pure C# implementation since the coding time for the competition was severely constrained.

The optimal algorithm is conceptually quite simple. 
Keep track of your tank's distance from the target, and the indices of the next shootable and unshootable wall segments in front of you 
(and treat the target as being the final shootable wall segment).
An unshootable wall segment blocks you from moving any closer because its central brick has been shot away already, but some of the adjacent bricks are still standing.
If you're an even distance away from the next brick in your way (or the target), and you aren't blocked by an unshootable wall segment, then move one space closer before firing.
Otherwise fire a shot. Bullets move at two cells per tick, tanks at one cell per tick. 
So calculate how many ticks until the next wall segment is shot away, and how much closer to the target you will be by that time.
But take into account whether there is an unshootable wall segment that will block your movement.
Recalculate your new position and the next wall segment, and repeat.

## The scenario-based AI engine

My overall strategy was to create an engine which could evaluate 
[a range of scenarios](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Scenarios).

If the preconditions for a scenario were met, then the scenario would assign values to moves that were desirable in that scenario.

This would allow simple scenarios to be created initially (e.g. dodging enemy bullets, attacking the enemy base, defending one's own base), 
but more complex scenarios to be added later, time permitting.

In theory, this approach could have been used to define additional scenarios consisting of the opening moves for a particular game board.
But since 7 of the 8 boards were only revealed in the final week of the competition, this wasn't a practical strategy.

### Value functions

The most common value function used was the logistic curve (an S-shaped curve), 
or rather a [ReverseLogisticFunction](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core/MathFunctions/ReverseLogisticFunction.cs), 
since small values (e.g. small distances to the enemy base) are usually higher-valued.

One advantage of the logistic curve is that it can approximate a step-change in value.
For example, when a bullet is racing towards your tank, the value of side-stepping the bullet is low when the bullet is far away.
But it is extremely high when the bullet is getting close.

The shape of the logistic curve can easily be scaled to give a very steep slope for short-range scenarios such as bullet-dodging, 
but a very wide slope (almost linear) for long-range scenarios such as moving closer to the enemy base in order to attack it.

This allows a tank to trade off short-range and long-range goals, and make a reasonable compromise between the two.

### Downsides of the value-function approach

Eight different game boards were provided by the competition hosts. 
One of these involved a thick block of wall across most of the centre segment of the board, apart from a thin line down the very centre between the two bases.
Because of the symmetric initial position of the two friendly tanks, they would both try to shoot their way to this centre line in order to line up with the enemy base and shoot it.
When this happened they would get in each other's way and block each other from reaching the centre line, until an enemy tank shot one of them down the centre line.

To fix this, I added a [ScenarioOfFriendlyTanksBlockingEachOther](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Scenarios/ScenarioOfFriendlyTanksBlockingEachOther.cs) class.

My initial attempts to break this deadlock were ludicrously unsuccessful... when the tanks were adjacent, one of the tanks would turn away to get out of the first tank's way.
Unfortunately this would cause the precedence rules to reverse, and now the tanks would reverse role, with the other tank now trying to get out of the way of the first.
This led to an amusing rumba dance between the two tanks!

The final solution was to break the symmetry by giving preference to the tank moving upwards or to the right (but only if it was close to being blocked).

### A possible "killer" scenario

Unfortunately I ran out of time (and, possibly more importantly, mental alertness) to implement a set of scenarios for my "killer strategy".
So I don't know if it would have worked in practice.

My idea was to have one tank lock down an enemy tank in one of two ways:
* Engage it in a short-range firefight, where neither tank could move away without getting shot. Both tanks have no option but to keep shooting at the other tank's approaching bullet. This is the [ScenarioToApplyLockDownActions](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Scenarios/ScenarioToApplyLockDownActions.cs)
* Move next to the enemy tank, but offset from its centre, so that neither tank can shoot the other. The enemy tank is physically blocked from moving in one direction, and can't move in a second direction because it will be moving into the line of fire. If it moves in one of the other two directions, you move closer, shunting it towards the edge of the board.

In both cases there would be a preceding scenario, 
[LockDownEnemyTankForOtherTankToDestroyScenario](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Scenarios/LockDownEnemyTankForOtherTankToDestroyScenario.cs).
This scenario would assume the enemy tank is moving into position to attack your base, and finding a suitable point along its possible pathway at which to intercept it.
The complication here is that the enemy tank could be moving first horizontally then vertically, first vertically then horizontally, or along a zig-zag path (such as moving along the longer edge of the rectangle if both distances are the same).
I started created a modified distance calculator to address this complication.
However I was running out of time, so went with a much cruder (and probably very buggy) interception algorithm instead.

Once the enemy tank is locked down, the other friendly tank would attack and destroy the locked down enemy tank using the
[ScenarioToAttackLockedDownEnemyTank](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Scenarios/ScenarioToAttackLockedDownEnemyTank.cs).
At this point one friendly tank can defend the base against the remaining enemy tank, while the other friendly tank destroys the enemy base.
Or, if the remaining enemy tank moves back to defend its base, then both friendly tanks can attack the enemy base from different directions.

The scenario would need to ensure that the second friendly tank can attack the locked down enemy tank sooner than the other enemy tank can also join the fight.
It would also need to ensure that one of the friendly tanks can move back in defence of the base before it can be destroyed by the remaining enemy tank.

However both of these scenarios can be implemented by more fundamental scenario classes, 
such as [ClearRunAtBaseScenario](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Scenarios/ClearRunAtBaseScenario.cs).
These scenarios are generically useful, beyond the context of the possible killer strategy.

### Other possible scenarios

The beauty of the scenario approach is that you can keep adding new scenarios as long as time allows.

I had a variety of other ideas for scenarios, such as:
* Have 2 tanks advance towards an enemy tank, one behind and slightly to the side of the other. Both tanks should fire their bullets so that the bullets will reach the enemy tank at the same time, making it harder for the enemy tank to dodge both bullets.
* Line up 2 tanks in the same direction, having one fire a bullet then move out the way, and the other fire a second bullet along the same line of fire
  * This might allow a wall to be cleared away quicker
  * It could also be used to catch an enemy tank behind the wall by surprise

### Generation of scenarios

Some scenarios are symmetric, in the sense that they can be run from your perspective or the opponent's. 
The same scenario can be an indicator that you are getting close to a situation where the opponent can't prevent you from destroying their base.
But it could also indicate that the opponent is close to doing the same to you, prompting you to take defensive actions.

Other scenarios, such as a bullet-dodging scenario, are only of interest to you.

Some scenarios are only applicable to one or two tanks. Others might involve all 4 tanks.
Some scenarios can be run with different combinations of tank directions. 
For example, when you lock down an enemy tank from a particular direction, you want your second tank to attack the enemy tank from a different direction, so that your tanks don't get in each other's way.

To handle this variety of situations, I used a [MoveGenerator](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/MoveGenerator.cs) base class.
Derived classes generated various combinations of players, tanks or directions, and bound these to standard variables.

These standard variables allowed scenarios to be designed on paper using a Mathematical notation:
* p and pBar stood for the two players (protagonist and antagonist) in the scenario.
* i and iBar stood for the two tanks of player p.
* j and jBar stood for the two tanks of player pBar.
* dir1, dir2 and dir3 referred to arbitrary direction variables defined by each scenario (such as the direction which an enemy base or tank might be attacked from)

The [Move](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Move.cs)
class has a property for each of these variables.

A stack of MoveGenerator-derived classes is used to define the various combinations of values for each variable.
This stack is then used to create a tree of Move classes, with the leaf nodes of the tree having all required variables bound.
The scenario is then evaluated against each of the leaf nodes. 
If the preconditions for a scenario are met, then the value of relevant tank actions for a player's own tanks are updated.

After each scenario is evaluated, the highest valued action for each of your own tanks is then submitted to the game server.
This way if time runs out before all scenarios are evaluated, the best action so far has still been taken.

### A sample scenario to illustrate these concepts

The [ClearRunAtBaseScenario](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Scenarios/ClearRunAtBaseScenario.cs) 
illustrates many of these concepts.

The stack of move generators is shown below, and the first array element indicates that the scenario is evaluated for each player:

```cs
        public override MoveGenerator[] GetMoveGeneratorsByMoveTreeLevel()
        {
            return new MoveGenerator[]
            {
                new MoveGeneratorOfPlayers(),
                new MoveGeneratorOfTankCombinationsForPlayerP(),
                new MoveGeneratorOfDirectionsForDir1(ScenarioDecisionMaker.p)
                // NOT NEEDED, SYMMETRICAL: new MoveGeneratorOfTankCombinationsForPlayerPBar()
            };
        }
```

A leaf-level Move node is evaluated to see whether the scenario is applicable:
```cs
        public bool IsValid(Move move)
        {
            // Exclude directions of attack which aren't possible:
            if (!Game.Current.Players[move.pBar].Base.GetPossibleIncomingAttackDirections().Contains(move.dir1))
            {
                return false;
            }

            if (!GetTankState_i(move).IsActive)
            {
                return false;
            }

            TankSituation tankSit_i = GetTankSituation(move.p, move.i);
            if (tankSit_i.IsInLineOfFire || tankSit_i.IsLockedDown || tankSit_i.IsShutIntoQuadrant)
            {
                return false;
            }
            return true;
        }
```

`public override MoveResult EvaluateLeafNodeMove(Move move)` is where the bulk of the work happens. 
This method determines how far each player's attack tank is from attacking the enemy base and how far the defence tank is from getting into a defensive position.
It evaluates these 4 distances to determine how close the protagonist is from getting into a position where it can attack the enemy base first, 
and where the enemy can't get back to defend in time. This generates a slack value, indicating the amount of "slack" until the winning condition will be realised.

Depending on whether you are the protagonist (p) or antagonist (pBar) in the scenario, 
one of the following two methods will be called to convert this slack value 
into a value (on the reverse S-curve) for the relevant offensive or defensive actions for your tanks:
* `public override void ChooseMovesAsP(MoveResult moveResult)`
* `public override void ChooseMovesAsPBar(MoveResult moveResult)`

# Generated files

The files generated (in debug mode) are as follows:
   
1.  InitialGame.xml - a snapshot of the serialized Game object at the very start of the game.
   
    Powershell utilities were used to edit these files to create static test scenarios.
        
2.  GameState.bmp - a bitmap representation of the board.
   
    By opening this in a utility such as Windows Photo Viewer, it was possible to follow a game in progress.
   
3.  GameStateAsText.txt - a text art format for the latest game state as received from the test harness.
         
    For example, a subset of a game in progress is shown below:

```
    ------------------------------------###---###-----------------v-
 75 ------------------------------------###---###-------------------
    -----------------------------------###-----###------------------
    -----------------------------------###--0--###------------------
    -----------------------------------###-----###------------------
    -----------------------------------###-----###------------------
 80 ----------------------------------------------------------------
    0    5    1    1    2    2    3    3    4    4    5    5    6   
	0    5    0    5    0    5    0    5    0    5    0    5    0
````

This was very useful for getting the exact coordinates of a tank, bullet or cell.
         
4.  CalculatedGameStateAsText.txt - text art of the latest game state as calculated by the game state engine.
         
    This was used to test whether the internal game state engine matched the game state received from the test harness. 
    By doing a diff of the files, discrepancies could be identified and investigated.
         
5.  Game.xml - a serialization of the current Game object.
        
    This uses the DataContractSerializer.
    Additional command line parameters on ConsoleApp and ConsoleApp2 allow a game 
    to be replayed to a particular turn before switching back to using the solvers.
         
6.  ConsoleApp.log or ConsoleApp2.log: the same text as displayed in the console window in debug mode.
    
7.  The Images\ sub-folder:
    
    CalculatedGameStateAsText_{TurnNumber}.txt
    GameStateAsText_{TurnNumber}.txt
         
    Contains text art of the game after every turn.
    This is useful for doing visual diffs of the game, either:
    * between 2 points in time, or
    * between the calculated and received game states.


# Instructions for running the bot

1. You will first need to download and install the official test harness found at http://challenge.entelect.co.za/DisplayLink.aspx?group=Rules&name=Rules 
   _[Update: this link is broken - the harness has since been taken down by Entelect]_.
   I installed the harness to C:\Competitions\EntelectChallenge2013\Harnesses\TestHarnessBeta\harness.
   You may need to edit some of the batch files and/or Powershell scripts to run from elsewhere.

2. I have 2 bots set up. You can run these from within Visual Studio by:
    1. right clicking on the solution in solution explorer
    2. opening up the properties window for the solution
    3. choosing multiple start-up projects

3. You should first start the test harness before launching the bots.
    1. There is a batch file in Source\Scripts\LaunchHarness.bat which does this.
       However the path is hard-coded as described in step 1 above.
    2. For ease of access I added the Scripts folder as a toolbar on my Windows 7 task bar.

4. Edit the properties of the AndrewTweddle.BattleCity.ConsoleApp and ConsoleApp2 projects.
   
   These are (respectively) set to:
     1. http://localhost:7070/Challenge/ChallengeService C:\Competitions\EntelectChallenge2013\temp\GameLogs
     2. http://localhost:7071/Challenge/ChallengeService C:\Competitions\EntelectChallenge2013\temp\GameLogs
   
   Change the second parameter to whichever folders you would like these apps to save their files to.
   

# Structure of the main code base

The C# projects can be found under Source\AndrewTweddle.BattleCity


1. [AndrewTweddle.BattleCity.Core](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core) is used extensively by other modules.


2. [AndrewTweddle.BattleCity.AI](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI) contains common classes for:

    1. Coordinating games in [Coordinator.cs](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/Coordinator.cs) - i.e. common administrative functionality for the bot
        1.   This administers the Solver (see below)
        2.  It manages communication with the communicator component (e.g. the test harness web service)
        3. It manages timing and prompts the solvers when to stop thinking and make a move.

    2. Interfaces for communicating with the Game Engine/Harness - [ICommunicator](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ICommunicator.cs), [ICommunicatorCallback](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ICommunicatorCallback.cs), [RemoteCommunicatorCallback](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/RemoteCommunicatorCallback.cs)

    3. The [Solvers\](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/Solvers) folder contains the base classes for "solvers" (i.e. interfaces/base classes for the bots)

    4. The [ScenarioEngine\](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine) folder contains the framework I developed for evaluating a variety of scenarios.
     and adjusting the values of each tank's available actions according to the scenario.
     
     This was what I ended up using in the competition entry for my bot.
     
        1. The [ScenarioEngine\Scenarios](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Scenarios) sub-folder contains specific scenarios that I wrote code for.
        2. It also contains the static [ScenarioValueFunctions](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Scenarios/ScenarioValueFunctions.cs) class with the value functions used for various scenarios.
         TODO: Make this a non-static class and allow multiple scenario-driven bots to compete with different value functions & weightings.
        3. The [ScenarioEngine\MoveGenerators](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/MoveGenerators) subfolder contains classes
         to generate combinations of parameters to a scenario e.g. player indexes, tank indexes, directions of attack, etc.
         These could either be used to generate different options and apply weightings to them.
         Or it could be used to generate a search tree.
         For the search tree, I only implemented a mini-max algorithm 
         as performance was good enough, so I could avoid the additional complexity/risk
         of NegaMax/Alpha-Beta at such a late stage of the competition.
         However those algorithms would be a worthy addition to the framework.
        4. The [Move](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScenarioEngine/Move.cs) class contains all possible parameters that I needed.
         The properties have cryptic names mapping to a Mathematical notation I devised to represent scenarios.
         Some of these names (deliberately) break the standard C# naming convention for properties.
         TODO: Work out whether there is an elegant way to use generics to support custom Move sub-classes
         for different scenarios and different layers in the search/move tree for a scenario.

    5. The [ScriptEngine\](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ScriptEngine), [SchedulingEngine\](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/SchedulingEngine) and [Scripts\](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/Scripts) sub-folders are the debris from an earlier over-engineered attempt at a scenario engine.
     The goal was to plan out moves for all tanks & bullets for a scenario as generated by a scripting engine.
     This agenda of tank & bullet actions could then be quickly forward-projected to determine situations like bullet collisions, for example.
     The idea was that the scripting engine would then either:
        1. Modify the timelines by invoking sub-scripts which could
            1. insert delays into the tank path, or
            2. insert bullet avoidance sub-scripts into the original script timeline
        or
        2. create a search tree node to try out different alternative strategies
           (this would depend on search tree settings - e.g. an iterative deepening scheme)
         
    6. The [Intelligence\](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/Intelligence), [Strategies\](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/Strategies) and [ValueStrategy\](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.AI/ValueStrategy) subfolders were for a subsequent under-engineered attempt at a scenario engine. 
       I quickly got bogged down in the complexities of trying to code specific scenarios without first having defined the scenarios in pseudo-code, and without having a framework to help me arrange my ideas more logically.
     
     
    Note that there is a layer violation error with AndrewTweddle.BattleCity.AI as the AndrewTweddle.BattleCity.VisualUtils project is referenced.
    This was used by the coordinator to write out an image of the board after every turn, so that the progress of the game could be followed by opening the image in Windows Photo Viewer (or a similar tool which auto-updates whenever the image changes).
  
    TODO: Fix the layer violation by injecting in the dependency from the console app/s.
     

3. [AndrewTweddle.BattleCity.Aux](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Aux) - contains the code to read the Json board generated by the Entelect test harness.

	 
4. [AndrewTweddle.BattleCity.Bots](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Bots) - contains a number of bots derived from the Base Solver class.

    1. Bots used for testing:
        1. [NoBot](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Bots/NoBot.cs) does absolutely nothing.
        2. [ScaredyBot](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Bots/ScaredyBot.cs) gets its tanks out the way by moving them to the closest corners of the board
        3. [RandomBot](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Bots/RandomBot.cs) chooses random moves for its tanks.
        4. [ShortestPathBot](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Bots/ShortestPathBot.cs) attacks the enemy base with whichever tank is closest.
         With its other tank (if still alive) it will attack the closest enemy tank.
  
    2. [ScenarioDrivenBot](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Bots/ScenarioDrivenBot.cs) - the bot entered in the competition. It uses the ScenarioEngine from the AI project.
  
    3. Bots which are the debris of earlier attempts:
        1. [SmartBot](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Bots/SmartBot.cs) contains a number of hard-coded scenarios (instead of using any of the AI frameworks).
        2. [ValueMaximizerBot](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Bots/ValueMaximizerBot.cs) uses the obsolete code from the AI.Intelligence, AI.Strategies and AI.ValueStrategy namespaces.


5. [AndrewTweddle.BattleCity.Comms.Client](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Comms.Client) - contains the web service client proxy and ICommunicator implementation

    1. [WebServiceClient.cs](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Comms.Client/WebServiceClient.cs) contains the web service proxy class generated by WCF's svcutil tool.
    2. [WebServiceAdapter.cs](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Comms.Client/WebServiceAdapter.cs) contains the class which implements the ICommunicator interface called from the Coordinator.
  

6. [AndrewTweddle.BattleCity.ConsoleApp](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.ConsoleApp) and [AndrewTweddle.BattleCity.ConsoleApp2](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.ConsoleApp2) - the two console app's which run the bots.
    
    1. ConsoleApp is submitted as the entry in the competition and is typically run in debug mode.
    2. ConsoleApp2 is for testing ConsoleApp against and is typically started without debugging.
    
    Note the command line parameters:
    
    USAGE: serverUrl [logFolderPath [gameStateFileToReplay [TickToReplayTo|yourPlayerIndex]]]
    
    gameStateFileToReply is a path to an xml file of the game (a serialized version of Game.Current).
    
    If TickToReplayTo > 2, then the console app & Coordinator will replay all the moves found in the xml file up until TickToReplyTo. Only then will it switch into the normal Solver mode of play.
    So a breakpoint can be put in the solver code, and it will only be reached after that tick.
    
    If TickToReplayTo is 0 or 1, then it is instead treated as the console app's player index.
    In this case the Coordinator starts in a special snapshot mode where the solver make choices for the current turn only.
    In this mode the coordinator will not try to communicate with the test harness or make multiple moves.
    This mode is typically used for editing a game file (using Powershell utilities written for this purpose) to engineer a particular scenario and then test the next move the solver will make in this position.

7. [AndrewTweddle.BattleCity.Core.UnitTests](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Core.UnitTests) - contains a small number of unit tests

    1. Typically unit testing isn't ideal for performance-sensitive code such as an AI algorithm as it leads to many extra method calls.
    2. NUnit is assumed to be the unit testing framework.
    3. There is only one unit test file currently.This was used for testing the circular buffer data structure used as a BFS queue by the shortest path algorithm.

8. [AndrewTweddle.BattleCity.Core.Experimental.CommandLine](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Experimental.CommandLine)
    - a command line application for testing performance of various algorithms
    
    1. This was structured like a home-grown unit test application.
    2. It runs a variety of algorithms and saves the console output to a file, so that changes in performance data can be easily tracked by doing a text diff of the files.
    3. The folder path for this file is currently hard-coded into the application at:
    
        C:\Competitions\EntelectChallenge2013\temp\PerformanceStats\PerfStats_{DateAndTime}.txt
	
	4. The path to the JSON file generated by the Entelect test harness is also hard-coded to:
	
		C:\Competitions\EntelectChallenge2013\Harnesses\TestHarnessBeta\harness\boards\board.txt
		
	5. The path to generate a bitmap image of the board is hard-coded to:
	
		c:\Competitions\EntelectChallenge2013\temp\board.bmp
		
	6. The [SegmentStateMatrixTester](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Experimental.CommandLine/SegmentStateMatrixTester.cs) 
       class hard-codes the location of a variety of board images
	   which it generates (typically with overlays to show the calculation results).
    
    TODO: Remove hard-coding by adding command line parameters for the output folder path and the test harness path.
    
9. [AndrewTweddle.BattleCity.Harnesses.ConsoleRunner](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Harnesses.ConsoleRunner)
	- a command line utility for running two bots in a series of matches 
	
	* Command line parameters:
	
	Usage: Style[MANUAL/AUTO] EntelectHarnessFolderPath Player1Name Player1FolderPath Player1OutputPath Player2Name Player2FolderPath Player2OutputPath ResultsFolderPath [StartIndex [EndIndex]]
	
	TODO: Determine the game winner by reading the json files generated by the test harness.
	
10. [AndrewTweddle.BattleCity.Tournaments](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.Tournaments) - contains classes used by the ConsoleRunner

11. [AndrewTweddle.BattleCity.UI](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.UI)
        - The debris of an attempt to build a UI to play the game against / interrogate the calculated data for a board position 
		(similar to what I did for the Entelect Tron competition last year)

	1. This is incomplete.
	2. It was taking too long to open up the board file.
	3. I realised I didn't have enough time to write a UI and still get benefit from it, 
	   especially if I was going to have to solve performance problems as well.
	   
12. [AndrewTweddle.BattleCity.VisualUtils](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/AndrewTweddle.BattleCity/AndrewTweddle.BattleCity.VisualUtils)
    - This contains an ImageGenerator class used to generate bitmap images of the board and/or current game state.


# Structure of the utility scripts

The main Powershell scripts and batch files can be found under Source\Scripts

1. [Generate-ServerClasses.ps1](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/Scripts/Generate-ServerClasses.ps1)
   generates the web service proxy code using svcutil.
2. [Create-Entry.ps1](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/Scripts/Create-Entry.ps1)
   packages up an entry for uploading to the Entelect competition site.
3. [LaunchHarness.bat](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/Scripts/LaunchHarness.bat)
   is useful for quickly launching the Entelect harness from the Windows 7 toolbar.
4. [Select-Board.ps1](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/Scripts/Select-Board.ps1)
   prompts one to choose which board to use for the test harness.
   Its second parameter is the path to the harness.
5. [Run-Tournament.ps1](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/Scripts/Run-Tournament.ps1)
   is a thin wrapper around the tournament runner utility, adding the convenience of Powershell argument-handling.
6. [Generate-XamlBoard.ps1](https://github.com/AndrewTweddle/EntelectChallenge2013/blob/master/Source/Scripts/Generate-XamlBoard.ps1)
   is debris from my (suspended) attempt to build a UI for the game.
7. The [TestUtils\](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/Scripts/TestUtils)
   subfolder contains useful utilities for editing a game and saving a new game file for testing specific scenarios:
   1. The [TestUtils\TestScripts](https://github.com/AndrewTweddle/EntelectChallenge2013/tree/master/Source/Scripts/TestUtils/TestScripts)
      sub-folder contains code to generate a few of the test files. 
      Use these as a sample of how to use the Powershell scripts to generate a test scenario.
