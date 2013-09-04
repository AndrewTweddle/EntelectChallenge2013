param (
    [string] $gameStateFilePath
)

$game = [AndrewTweddle.BattleCity.Core.Elements.Game]::Load($gameStateFilePath)
[AndrewTweddle.BattleCity.Core.Elements.Game]::Current = $game
