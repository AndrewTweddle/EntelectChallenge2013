param (
    [string] $pathToHarnessFolder = 'C:\Competitions\EntelectChallenge2013\Harnesses\TestHarnessBeta\harness',
    [string] $pathToWsdl = 'C:\Competitions\EntelectChallenge2013\Source\Contracts\ChallengeService.wsdl',
    [switch] $useWsdlPath = $false,
    [int] $secondsToWaitForHarness = 15,
    [switch] $launchHarness = $false
)

if (-not $useWsdlPath)
{
    if ($launchHarness)
    {
        start-process -FilePath "$pathToHarnessFolder\launch.bat" -workingdirectory $pathToHarnessFolder
        start-sleep -seconds $secondsToWaitForHarness
    }
    [string] $path = 'http://localhost:7070/Challenge/ChallengeService?wsdl'
} else {
    [string] $path = $pathToWsdl
}

cd 'C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools'
.\svcutil.exe `
    /t:code $path `
    /d:C:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.Comms.Client `
    /out:WebServiceClient.cs `
    /svcutilConfig:c:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.Comms.Client\output.config
