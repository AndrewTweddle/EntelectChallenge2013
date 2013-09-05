param (
	$initialGameStateFilePath = 'C:\Competitions\EntelectChallenge2013\temp\InitialGameStates\original\InitialGame.xml',
    $modifiedGameStateFilePath = 'C:\Competitions\EntelectChallenge2013\temp\InitialGameStates\original\ModifiedGame.xml'
)

cd ectest:\

.\Load-Assemblies.ps1
.\Load-GameStateFile.ps1 $initialGameStateFilePath

$gameState = .\Get-CurrentGameState.ps1

Write-Host "Loaded game state from $initialGameStateFilePath :" -foregroundColor Green
.\Show-GameState.ps1 $gameState

$down = .\Get-Dir.ps1 'DOWN'
$up = .\Get-Dir.ps1 'UP'
$left = .\Get-Dir.ps1 'LEFT'
$right = .\Get-Dir.ps1 'RIGHT'

.\Move-Tank.ps1 $gameState 0 0 12 35 $down  # tank A
.\Move-Tank.ps1 $gameState 0 1 48 15 $down  # tank C
.\Move-Tank.ps1 $gameState 1 0 22 15 $up  # tank B
.\Move-Tank.ps1 $gameState 1 1 58 15 $up  # tank D

Write-Host "Modified game state for saving to $modifiedGameStateFilePath :" -foregroundColor Green
.\Show-GameState.ps1 $gameState

.\Save-GameStateFile.ps1 $modifiedGameStateFilePath
