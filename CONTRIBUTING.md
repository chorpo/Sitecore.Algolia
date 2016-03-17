# Supported Sitecore Versions

Provider supports Sitecore versions from 7.0 to 8.1. These versions have slight differences in API but development can be performed against any of them. 
You need only sync Sitecore DLLs and project varriables defined in include file.  

# Local Environment Setup

Some manual steps required to setup solution:

1. Copy Sitecore DLLs for specific Sitecore version to **Libs/Sitecore** folder:
    * sitecore.nexus.dll
    * Newtonsoft.Json.dll
    * Sitecore.ContentSearch.dll
    * Sitecore.ContentSearch.Linq.dll
    * Sitecore.Kernel.dll

2. Copy **sitecore-version.props** file into solution root from **Automation/Sitecore-Versions/{SitecoreVersion}** 
3. Copy your sitecore license into **Score.ContentSearch.Algolia.Tests** folder to enable Sitecore.FakeDb tests

## Local Environment Setup for SCORE developers

Once you cloned the repo and **prior opening the solution in Visual Studio** perform the following steps:

1. Check-out **score-automation** repository
2. Run **setup.ps1** script for desired Sitecore version. 
3. Copy Sitecore lincese xml file into **Score.ContentSearch.Algolia.Tests** folder

### Powershell Script details

Sample Scripts

* 7.2 Update 4 -  `powershell -File ../score-automation/setup.ps1 -sitecoreVersion 7.2.150407`.
* 8.0 Update 3 -  `powershell -File ../score-automation/setup.ps1 -sitecoreVersion 8.0.150427`.
* 8.1 Initial  -  `powershell -File ../score-automation/setup.ps1 -sitecoreVersion 8.1.151003`. 

If Powershell complains about execution policy please do the following and try again

* Run the **Command Prompt** with administrative privileges
* Execute `powershell Set-ExecutionPolicy RemoteSigned`

If copy process complains about a particular Sitecore DLL being in use just run the setup script again

# Build Configurations

## Debug

Default DEV environment

## Release

Used for release bytes generation

## Sandbox

Similar to Debug but copies output DLLs into **/sandbox/Website/bin/** folder

