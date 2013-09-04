param (
    [string] $gameStateFilePath
)

$game = ecTest:\Get-Game.ps1
$game.Save($gameStateFilePath)
