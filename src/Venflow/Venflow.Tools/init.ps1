param($installPath, $toolsPath, $package, $project)

$importModulePath = (Join-Path $toolsPath 'Venflow.Tools.dll')

$importedModule = Get-Module 'Venflow.Tools'

$shouldImport = $true

# Make sure that the module hasn't been imported yet.
if ($importedModule)
{
    # Check if already imported version has a lower version than the new one.
    if ($importedModule.Version -le (Get-Item $importModulePath).VersionInfo.FileVersion)
    {
        Remove-Module 'Venflow.Tools'
    }
    else
    {
        $shouldImport = $false
    }
}

if ($shouldImport)
{
    Import-Module $importModulePath
}

try
{
    $toolOutput = dotnet tool list -g | Out-String

    # Ensure that the dotnet tool is installed
    if(!$toolOutput.Contains('Venflow.Tools.CLI'))
    {
        dotnet tool install -g 'Venflow.Tools.CLI'
    }
}
catch
{
    Write-Error 'The dotnet sdk is not installed, you can install it from https://dotnet.microsoft.com/download.'
}