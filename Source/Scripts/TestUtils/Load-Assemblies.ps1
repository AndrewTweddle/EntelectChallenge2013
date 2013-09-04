param (
    [string] $assemblyFolderPath = 'C:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.ConsoleApp\bin\Debug',
    [string] $tempAssemblyFolderPath = 'C:\Competitions\EntelectChallenge2013\temp\PSTestAssemblies'
)

copy-item -path "$assemblyFolderPath\*.dll" -destination $tempAssemblyFolderPath

$assemblyFileNames = @(
    'AndrewTweddle.BattleCity.Core.dll',
    'AndrewTweddle.BattleCity.VisualUtils.dll',
    'AndrewTweddle.BattleCity.AI.dll',
    'AndrewTweddle.BattleCity.Bots.dll',
    'AndrewTweddle.BattleCity.Comms.Client.dll'
)

$assemblyFileNames | % {
    [string] $assemblyFilePath = [IO.Path]::Combine( $tempAssemblyFolderPath, $_ )
    add-type -path $assemblyFilePath
}
