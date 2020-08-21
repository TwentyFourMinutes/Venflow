---
uid: Guides.GettingStarted.Installation
title: Installing Venflow
---

# Venflow Installation

## Supported Platforms

Venflow supports `.Net Framework 4.8`, `.Net Standard 2.1`, `.Net Core 3.1` and the latest pre-releases of `.Net 5`.

## Installation from NuGet

DulcisX is distributed through the official NuGet feed as a lot of the other packages, which makes its install as easy as its get.

> [!WARNING] 
> For now Venflow is published under the _pre-release_ tag and might still contain bugs or other issues, if you encounter something please create an [issue](https://github.com/TwentyFourMinutes/Venflow/issues) over on GitHub.

### [Using Visual Studio](#tab/visualstudio-install)

1. Right click on 'References', and select 'Manage NuGet packages'

2. Check the 'include prerelease' checkbox

3. In the "Browse" tab, search for Venflow

4. Click install.


### [Using the Nuget Package Manager](#tab/npm-install)

1. Click on 'Tools', 'Nuget Package Manager' and 'Package Manager Console'

2. Enter `Install-Package Venflow`