param (
    [string] $sourceFolder = 'C:\Competitions\EntelectTronChallenge\Software'
}

[string] $sourceFilesPath = "$sourceFolder\AndrewTweddle\Source"
cd $sourceFolder

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
        copy-item 'C:\Competitions\EntelectTronChallenge\Software\TronMarshall\Entry' . -recurse
    }
    Invoke-PromptingAction 'Rename Entry to AndrewTweddle' {
        rename-item .\Entry AndrewTweddle
    }
    Invoke-PromptingAction 'Copy solution files to Source sub-folder' {
        copy-item C:\Competitions\EntelectTronChallenge\Software\TronMarshall\*.sln $sourceFilesPath
    }
    
    $projectFolders = @(
        'C:\Competitions\EntelectTronChallenge\Software\TronMarshall\AndrewTweddle.Tron.Bots'
        'C:\Competitions\EntelectTronChallenge\Software\TronMarshall\AndrewTweddle.Tron.Console'
        'C:\Competitions\EntelectTronChallenge\Software\TronMarshall\AndrewTweddle.Tron.Core'
        'C:\Competitions\EntelectTronChallenge\Software\TronMarshall\AndrewTweddle.Tron.UI'
    )
    
    Invoke-PromptingAction 'Copy projects under the Source sub-folder' {
        foreach ($projectFolder in $projectFolders)
        {
            Invoke-PromptingAction "Copy $projectFolder" {
                $destFolderPath = $projectFolder.Replace('C:\Competitions\EntelectTronChallenge\Software\TronMarshall\', 'C:\Competitions\EntelectTronChallenge\Software\AndrewTweddle\Source\')
                copy-item $projectFolder $sourceFilesPath -recurse
                
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
            'C:\Competitions\EntelectTronChallenge\Software\AndrewTweddle\Source'
            'C:\Competitions\EntelectTronChallenge\Software\AndrewTweddle\compile.bat'
            'C:\Competitions\EntelectTronChallenge\Software\AndrewTweddle\start.bat'
        )
        write-zip -path $toZip AndrewTweddle\AndrewTweddle.zip -includeEmptyDirectories
    }
}
