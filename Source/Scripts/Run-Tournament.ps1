param (
    [int]    $startIteration = $( [int]::Parse( (read-host 'Start iteration') ) ),
    [int]    $endIteration = $( [int]::Parse( (read-host 'End iteration') ) ),
    [string] $tournamentRunnerPath =  'C:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.Harnesses.ConsoleRunner\bin\Debug\AndrewTweddle.BattleCity.Harnesses.ConsoleRunner.exe',
    [string] $testHarnessFolder = 'C:\Competitions\EntelectChallenge2013\Harnesses\TestHarnessBeta\harness',
    [string] $player1Name = 'ConsoleApp',
    [string] $player1Folder = 'C:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.ConsoleApp\bin\Debug',
    [string] $player2Name = 'ConsoleApp2',
    [string] $player2Folder = 'C:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.ConsoleApp2\bin\Debug',
    [string] $gameLogsFolder = 'C:\Competitions\EntelectChallenge2013\temp\GameLogs',
    [string] $resultsFolder = 'C:\Competitions\EntelectChallenge2013\temp\GameLogs\TournamentResults',
    [switch] $isManualLaunch = $true
)

[string] $launchType = if ($isManualLaunch) { 'MANUAL' } else { 'AUTO' }

& $tournamentRunnerPath `
    $launchType `
    $testHarnessFolder `
    $player1Name `
    $player1Folder `
    $gameLogsFolder `
    $player2Name `
    $player2Folder `
    $gameLogsFolder `
    $resultsFolder `
    $startIteration `
    $endIteration
