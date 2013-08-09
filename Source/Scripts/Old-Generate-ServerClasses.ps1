cd 'C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools'
.\wsdl.exe /n:AndrewTweddle.BattleCity.Contracts /out:C:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.Services\gen /server 'C:\Competitions\EntelectChallenge2013\Source\Contracts\ChallengeService.wsdl'
.\svcutil.exe /out:C:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.Services\gen\BattleCityService.cs /ServiceContract /tcv:Version35 'C:\Competitions\EntelectChallenge2013\Source\Contracts\ChallengeService.wsdl'
