param (
    [string] $rootFolder = 'C:\Competitions\EntelectChallenge2013'
)
[string] $sourceFilesPath = "$rootFolder\Source"
[string] $entrySourceFilesPath = "$rootFolder\AndrewTweddle\Source"
cd $rootFolder

Invoke-PromptingAction 'Create competition entry' {
    # Invoke-PromptingAction 'Backup old folder' {
    #     Invoke-PromptingAction 'Remove AndrewTweddle.bak' {
    #         remove-item AndrewTweddle.bak -recurse -force
    #     }
    #     Invoke-PromptingAction 'Backup AndrewTweddle to AndrewTweddle.bak' {
    #         rename-item AndrewTweddle AndrewTweddle.bak -force
    #     }
    # }
    
    if (test-path 'AndrewTweddle')
    {
        throw 'The AndrewTweddle sub-folder already exists'
    }
    
    Invoke-PromptingAction 'Copy Entry batch files to Entry folder' {
        copy-item "$rootFolder\Source\Entry" . -recurse
    }
    Invoke-PromptingAction 'Rename Entry to AndrewTweddle' {
        rename-item .\Entry AndrewTweddle
    }
    Invoke-PromptingAction 'Copy solution files to Source sub-folder' {
        copy-item "$sourceFilesPath\AndrewTweddle.BattleCity\AndrewTweddle.BattleCity.Entry.sln" $entrySourceFilesPath
    }
    
    # $projectFolders = [IO.Directory]::GetDirectories('C:\Competitions\EntelectChallenge2013\Source\AndrewTweddle.BattleCity')
    $solutionFolder = "$sourceFilesPath\AndrewTweddle.BattleCity"
    $projectNames = @(
        'AndrewTweddle.BattleCity.Core'
        'AndrewTweddle.BattleCity.AI'
        'AndrewTweddle.BattleCity.Bots'
        'AndrewTweddle.BattleCity.Comms.Client'
        'AndrewTweddle.BattleCity.ConsoleApp'
        'AndrewTweddle.BattleCity.VisualUtils'
    )
    $projectFolders = $projectNames | % { "$solutionFolder\$_" }
    
    Invoke-PromptingAction 'Copy projects under the Source sub-folder' {
        foreach ($projectFolder in $projectFolders)
        {
            Invoke-PromptingAction "Copy $projectFolder" {
                $destFolderPath = $projectFolder.Replace( "$solutionFolder\", "$entrySourceFilesPath\")
                copy-item $projectFolder $entrySourceFilesPath -recurse
                
                Invoke-PromptingAction 'Remove bin sub-folder' {
                    remove-item "$destFolderPath\bin" -recurse
                }
                Invoke-PromptingAction 'Remove obj sub-folder' {
                    remove-item "$destFolderPath\obj" -recurse
                }
            }
        }
    }
    
    Invoke-PromptingAction 'Generate zip file' {
        $toZip = @( 
            'C:\Competitions\EntelectChallenge2013\AndrewTweddle\Source'
            'C:\Competitions\EntelectChallenge2013\AndrewTweddle\compile.bat'
            'C:\Competitions\EntelectChallenge2013\AndrewTweddle\start.bat'
        )
        write-zip -path $toZip AndrewTweddle\AndrewTweddle.zip -includeEmptyDirectories
    }
}
