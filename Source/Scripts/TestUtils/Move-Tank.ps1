param (
    $gameState,
    [int] $playerIndex,
    [int] $tankNumber,
    [System.Int16] $x,
    [System.Int16] $y,
    [AndrewTweddle.BattleCity.Core.Direction] $dir
)

$game = .\Get-Game.ps1
$tank = $game.Players[$playerIndex].Tanks[$tankNumber]

$tankPos = new-object 'AndrewTweddle.BattleCity.Core.Point' -argumentList $x,$y
$tankState = new-object 'AndrewTweddle.BattleCity.Core.States.MobileState' -argumentList $tankPos,$dir,$true
$gameState.SetMobileState($tank.Index, [ref]$tankState)
