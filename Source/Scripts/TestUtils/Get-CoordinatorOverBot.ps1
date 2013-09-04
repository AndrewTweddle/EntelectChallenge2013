param (
    [string] $botClassName = 'ScenarioDrivenBot',
    [string] $gameStateClassName = 'AndrewTweddle.BattleCity.Core.States.MutableGameState'
)

$solver = new-object "AndrewTweddle.BattleCity.Bots.$botClassName[$gameStateClassName]"
$coordinator = new-object "AndrewTweddle.BattleCity.AI.Coordinator[$gameStateClassName]" -argumentList $solver,$null,$null
$coordinator
