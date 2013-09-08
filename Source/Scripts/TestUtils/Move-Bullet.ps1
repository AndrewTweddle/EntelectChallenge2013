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

$bulletPos = new-object 'AndrewTweddle.BattleCity.Core.Point' -argumentList $x,$y
$bulletState = new-object 'AndrewTweddle.BattleCity.Core.States.MobileState' -argumentList $bulletPos,$dir,$true
$gameState.SetMobileState($tank.Index + 4, [ref]$bulletState)
