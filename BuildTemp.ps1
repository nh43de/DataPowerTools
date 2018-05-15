if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

EnsurePsbuildInstalled

exec { & dotnet restore }

Invoke-MSBuild

$revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$revision = "{0:D4}" -f [convert]::ToInt32($revision, 10)

exec { & dotnet test .\DataPowerTools.Tests\DataPowerTools.Tests.csproj -c Release }

exec { & dotnet pack .\DataPowerTools\DataPowerTools.csproj -c Release -o .\artifacts --version-suffix=$revision }  