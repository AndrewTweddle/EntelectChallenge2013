param (
    [string] $outputPath = 'C:\Competitions\EntelectChallenge2013\xaml',
    [int] $rowCount = 120,
    [int] $colCount = 120
)

$rowDefinitions = 1..$rowCount | % {
    @"
                <RowDefinition Height="Auto" />
"@
}
$rowDefinitionsXaml = join-string $rowDefinitions -NewLine
[IO.File]::WriteAllText("$outputPath\RowDefinitions1.txt", $rowDefinitionsXaml )

$colDefinitions = 1..$colCount | % {
    @"
                <ColumnDefinition Width="Auto" />
"@
}
$colDefinitionsXaml = join-string $colDefinitions -NewLine
[IO.File]::WriteAllText("$outputPath\ColumnDefinitions1.txt", $colDefinitionsXaml )

$rowLabels = 1..$rowCount | % {
    @"
            <Border Grid.Row="$_" Height="25" Width="25" BorderBrush="Black" BorderThickness="1">
                <TextBlock>$($_-1)</TextBlock>
            </Border>
            
"@
}
$rowLabelXaml = join-string $rowLabels -NewLine
[IO.File]::WriteAllText("$outputPath\RowLabels.txt", $rowLabelXaml )

$colLabels = 1..$colCount | % {
    @"
            <Border Grid.Column="$_" Height="25" Width="25" BorderBrush="Black" BorderThickness="1">
                <TextBlock>$($_ - 1)</TextBlock>
            </Border>
            
"@
}

$colLabelXaml = join-string $colLabels -NewLine
[IO.File]::WriteAllText("$outputPath\ColumnLabels.txt", $colLabelXaml )
