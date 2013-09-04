param (
	$gameStateFilePath = "C:\Competitions\EntelectChallenge2013\temp\GameLogs\2013-09-02_0330\ConsoleApp\Game.xml"
)

cd ectest:\

.\Load-Assemblies.ps1
.\Load-GameStateFile.ps1 $gameStateFilePath

$gameState = .\Get-CurrentGameState.ps1
Write-Host "Loaded game state from $gameStateFilePath :" -foregroundColor Green
.\Show-GameState.ps1 $gameState

$dir = .\Get-Dir.ps1 'UP'
.\Move-Tank.ps1 $gameState 0 0 10 10 $dir  # tank A
.\Move-Tank.ps1 $gameState 0 1 70 70 $dir  # tank C
.\Move-Tank.ps1 $gameState 1 0 10 20 $dir  # tank B
.\Move-Tank.ps1 $gameState 1 1 60 10 $dir  # tank D

$coordinator = .\Get-CoordinatorOverBot.ps1
$coordinator.ChooseMovesForNextTick()


