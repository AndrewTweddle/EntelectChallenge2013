param (
    [string] $assemblyFolderPath = 'C:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.ConsoleApp\bin\Debug'
)

$assemblyFileNames = @(
    'AndrewTweddle.BattleCity.Core.dll',
    'AndrewTweddle.BattleCity.AI.dll',
    'AndrewTweddle.BattleCity.Bots.dll'
)

$assemblyFileNames | % {
    [string] $assemblyFilePath = [IO.Path]::Combine( $assemblyFolderPath, $_ )
    [System.Reflection.Assembly]::Load( $assemblyFilePath )
}
