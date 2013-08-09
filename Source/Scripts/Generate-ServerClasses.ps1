cd 'C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools'
.\svcutil.exe `
    /t:code "C:\Competitions\EntelectChallenge2013\Source\Contracts\ChallengeService.wsdl" `
    /d:C:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.Comms.Client `
    /out:WebServiceClient.cs
