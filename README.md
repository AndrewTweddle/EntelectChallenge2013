# Entelect AI Challenge 2013
=========================

# Overview

This is my competition entry for the 2013 Entelect R100k Challenge.

# Instructions for running the bot

1. You will first need to download and install the official test harness found at http://challenge.entelect.co.za/DisplayLink.aspx?group=Rules&name=Rules.
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
   
   NB: The files generated (in debug mode) are as follows:
   
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
	0    5    0    5    0    5    0    5    0    5    0   
```
	
         This was very useful for getting the exact coordinates of a tank, bullet or cell.
         
    4.  CalculatedGameStateAsText.txt - text art of the latest game state as calculated by the game state engine.
         
         This was used to test whether the internal game state engine 
         matched the game state received from the test harness. 
         By doing a diff of the files, discrepancies could be identified and investigated.
         
    5.   Game.xml - a serialization of the current Game object.
        
         This uses the DataContractSerializer.
         Additional command line parameters on ConsoleApp and ConsoleApp2 allow a game 
         to be replayed to a particular turn before switching back to using the solvers.
         
    6.  ConsoleApp.log or ConsoleApp2.log: the same text as displayed in the console window in debug mode.
    
    7. The Images\ sub-folder:
    
         CalculatedGameStateAsText_{TurnNumber}.txt
         GameStateAsText_{TurnNumber}.txt
         
         Contains text art of the game after every turn.
         This is useful for doing visual diffs of the game, either:
         1. between 2 points in time, or
         2. between the calculated and received game states.
         


# Structure of the main code base

The C# projects can be found under Source\AndrewTweddle.BattleCity


1.AndrewTweddle.BattleCity.Core is used extensively by other modules.


2. AndrewTweddle.BattleCity.AI contains common classes for:

    1. Coordinating games (Coordinator.cs) - i.e. common administrative functionality for the bot
        1.   This administers the Solver (see below)
        2.  It manages communication with the communicator component (e.g. the test harness web service)
        3. It manages timing and prompts the solvers when to stop thinking and make a move.

    2. Interfaces for communicating with the Game Engine/Harness (ICommunicator, ICommunicatorCallback, RemoteCommunicatorCallback)

    3. The Solvers\ folder contains the base classes for "solvers" (i.e. interfaces/base classes for the bots)

    4. The ScenarioEngine\ folder contains the framework I developed for evaluating a variety of scenarios.
     and adjusting the values of each tank's available actions according to the scenario.
     
     This was what I ended up using in the competition entry for my bot.
     
        1. The ScenarioEngine\Scenarios sub-folder contains specific scenarios that I wrote code for.
        2. It also contains the static ScenarioValueFunctions class with the value functions used for various scenarios.
         TODO: Make this a non-static class and allow multiple scenario-driven bots to compete with different value functions & weightings.
        3. The ScenarioEngine\MoveGenerators\ subfolder contains classes
         to generate combinations of parameters to a scenario e.g. player indexes, tank indexes, directions of attack, etc.
         These could either be used to generate different options and apply weightings to them.
         Or it could be used to generate a search tree.
         For the search tree, I only implemented a mini-max algorithm 
         as performance was good enough, so I could avoid the additional complexity/risk
         of NegaMax/Alpha-Beta at such a late stage of the competition.
         However those algorithms would be a worthy addition to the framework.
        4. The Move class contains all possible parameters that I needed.
         The properties have cryptic names mapping to a Mathematical notation I devised to represent scenarios.
         Some of these names (deliberately) break the standard C# naming convention for properties.
         TODO: Work out whether there is an elegant way to use generics to support custom Move sub-classes
         for different scenarios and different layers in the search/move tree for a scenario.

    5. The ScriptEngine\, SchedulingEngine\ and Scripts\ sub-folders are the debris from an earlier over-engineered attempt at a scenario engine.
     The goal was to plan out moves for all tanks & bullets for a scenario as generated by a scripting engine.
     This agenda of tank & bullet actions could then be quickly forward-projected to determine situations like bullet collisions, for example.
     The idea was that the scripting engine would then either:
        1. Modify the timelines by invoking sub-scripts which could
            1. insert delays into the tank path, or
            2. insert bullet avoidance sub-scripts into the original script timeline
        or
        2. create a search tree node to try out different alternative strategies
           (this would depend on search tree settings - e.g. an iterative deepening scheme)
         
    6. The Intelligence\, Strategies\ and ValueStrategy\ subfolders were for a subsequent under-engineered attempt at a scenario engine. 
       I quickly got bogged down in the complexities of trying to code specific scenarios without first having defined the scenarios in pseudo-code, and without having a framework to help me arrange my ideas more logically.
     
     
    Note that there is a layer violation error with AndrewTweddle.BattleCity.AI as the AndrewTweddle.BattleCity.VisualUtils project is referenced.
    This was used by the coordinator to write out an image of the board after every turn, so that the progress of the game could be followed by opening the image in Windows Photo Viewer (or a similar tool which auto-updates whenever the image changes).
  
    TODO: Fix the layer violation by injecting in the dependency from the console app/s.
     

3. AndrewTweddle.BattleCity.Aux - contains the code to read the Json board generated by the Entelect test harness.

	 
4. AndrewTweddle.BattleCity.Bots - contains a number of bots derived from the Base Solver class.

    1. Bots used for testing:
        1.   NoBot does absolutely nothing.
        2.  ScaredyBot gets its tanks out the way by moving them to the closest corners of the board
        3. RandomBot chooses random moves for its tanks.
        4.  ShortestPathBot attacks the enemy base with whichever tank is closest.
         With its other tank (if still alive) it will attack the closest enemy tank.
  
    2. ScenarioDrivenBot - the bot entered in the competition. It uses the ScenarioEngine from the AI project.
  
    3. Bots which are the debris of earlier attempts:
        1.   SmartBot contains a number of hard-coded scenarios (instead of using any of the AI frameworks).
        2.  ValueMaximizerBot uses the obsolete code from the AI.Intelligence, AI.Strategies and AI.ValueStrategy namespaces.


5. AndrewTweddle.BattleCity.Comms.Client - contains the web service client proxy and ICommunicator implementation

    1. WebServiceClient.cs contains the web service proxy class generated by WCF's svcutil tool.
    2. WebServiceAdapter.cs contains the class which implements the ICommunicator interface called from the Coordinator.
  

6. AndrewTweddle.BattleCity.ConsoleApp and AndrewTweddle.BattleCity.ConsoleApp2 - the two console app's which run the bots.
    
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

7. AndrewTweddle.BattleCity.Core.UnitTests - contains a small number of unit tests

    1. Typically unit testing isn't ideal for performance-sensitive code such as an AI algorithm as it leads to many extra method calls.
    2. NUnit is assumed to be the unit testing framework.
    3. There is only one unit test file currently.This was used for testing the circular buffer data structure used as a BFS queue by the shortest path algorithm.

8. AndrewTweddle.BattleCity.Core.Experimental 
    - a command line application for testing performance of various algorithms
    
    1. This was structured like a home-grown unit test application.
    2. It runs a variety of algorithms and saves the console output to a file,
       so that changes in performance data can be easily tracked by doing a text diff of the files.
    3. The folder path for this file is currently hard-coded into the application at:
    
        C:\Competitions\EntelectChallenge2013\temp\PerformanceStats\PerfStats_{DateAndTime}.txt
	
	4. The path to the JSON file generated by the Entelect test harness is also hard-coded to:
	
		C:\Competitions\EntelectChallenge2013\Harnesses\TestHarnessBeta\harness\boards\board.txt
		
	5. The path to generate a bitmap image of the board is hard-coded to:
	
		c:\Competitions\EntelectChallenge2013\temp\board.bmp
		
	6. The SegmentStateMatrixTester class hard-codes the location of a variety of board images
	   which it generates (typically with overlays to show the calculation results).
    
    TODO: Remove hard-coding by adding command line parameters for the output folder path and the test harness path.
    
9. AndrewTweddle.BattleCity.Harnesses.ConsoleRunner 
	- a command line utility for running two bots in a series of matches 
	
	* Command line parameters:
	
	Usage: Style[MANUAL/AUTO] EntelectHarnessFolderPath Player1Name Player1FolderPath Player1OutputPath Player2Name Player2FolderPath Player2OutputPath ResultsFolderPath [StartIndex [EndIndex]]
	
	TODO: Determine the game winner by reading the json files generated by the test harness.
	
10. AndrewTweddle.BattleCity.Tournaments - contains classes used by the ConsoleRunner

11. AndrewTweddle.BattleCity.UI - The debris of an attempt to build a UI to play the game against / interrogate the calculated data for a board position 
		(similar to what I did for the Entelect Tron competition last year)

	1. This is incomplete.
	2. It was taking too long to open up the board file.
	3. I realised I didn't have enough time to write a UI and still get benefit from it, 
	   especially if I was going to have to solve performance problems as well.
	   
12. AndrewTweddle.BattleCity.VisualUtils - This contains an ImageGenerator class used to generate bitmap images of the board and/or current game state.


# Structure of the utility scripts

The main Powershell scripts and batch files can be found under Source\Scripts

1. Generate-ServerClasses.ps1 generates the web service proxy code using svcutil.
2. Create-Entry.ps1 packages up an entry for uploading to the Entelect competition site.
3. LaunchHarness.bat is useful for quickly launching the Entelect harness from the Windows 7 toolbar.
4. Select-Board.ps1 prompts one to choose which board to use for the test harness.
   Its second parameter is the path to the harness.
5. Run-Tournament.ps1 is a thin wrapper around the tournament runner utility, 
   adding the convenience of Powershell argument-handling.
6. Generate-XamlBoard.ps1 is debris from my (suspended) attempt to build a UI for the game.
7. The TestUtils\ subfolder contains useful utilities for editing a game and saving a new game file for testing specific scenarios:
    1. The TestUtils\TestScripts sub-folder contains code to generate a few of the test files. Use these as a sample of how to use the Powershell scripts to generate a test scenario.
