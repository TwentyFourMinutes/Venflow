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