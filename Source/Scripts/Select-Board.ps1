param (
	[string] $boardName = '',
	[string] $harnessRootPath = 'C:\Competitions\EntelectChallenge2013\Harnesses\TestHarnessBeta\harness'
)

[string] $boardsPath = [IO.Path]::Combine( $harnessRootPath, 'boards')
[string] $otherBoardsPath = [IO.Path]::Combine( $harnessRootPath, 'OtherBoards')

[string[]] $boardNames = 'board','lattice','navigation','openFields','original', 'board-center-counter', 'board-maze-warfare', 'board-optical-illusion'

while (($boardName -ne 'all') -and -not ($boardNames -contains $boardName))
{
    Write-Host 'CHOOSE A BOARD:'
    $boardNames | % { Write-Host "    $_" }
    Write-Host '    all (make all boards available)'
    Write-Host
    $boardName = Read-Host "Enter board name"
}

if ($boardName -eq 'all')
{
    get-childitem $otherBoardsPath -recurse | move-item -destination $boardsPath
} else {
    $boardFileName = "$($boardName).txt"
    
    [IO.Directory]::CreateDirectory($otherBoardsPath) | out-null
    
    [string] $srcBoardPath = [IO.Path]::Combine( $otherBoardsPath, $boardFileName)
    
    get-childitem $boardsPath -recurse | ? { $_.Name -ne $boardFileName } | move-item -destination $otherBoardsPath
    
    if ([IO.File]::Exists($srcBoardPath))
    {
        move-item -path $srcBoardPath -destination $boardsPath -force
    }
}
