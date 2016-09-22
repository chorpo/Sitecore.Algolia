# Supported Sitecore Versions

Provider supports Sitecore versions from 7.0 to 8.1. These versions have slight differences in API but development can be performed against any of them. 
You need only sync Sitecore DLLs and project varriables defined in include file.  

# Local Environment Setup

Once you cloned the repo and **prior opening the solution in Visual Studio** perform the following steps:

1. Copy your sitecore license into **Score.ContentSearch.Algolia.Tests** folder to enable Sitecore.FakeDb tests
1. Run **setup.ps1** script with parameters defined below for desired Sitecore version. 

### Powershell Script details


* 7.2 Update 4 -  `powershell -File ./automation/setup.ps1 -sitecoreVersion 7.2.150407`
* 8.0 Update 3 -  `powershell -File ./automation/setup.ps1 -sitecoreVersion 8.0.150427`
* 8.1 Update 3 -  `powershell -File ./automation/setup.ps1 -sitecoreVersion 8.1.160519` 
* 8.2 Initial  -  `powershell -File ./automation/setup.ps1 -sitecoreVersion 8.2.160729` 


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

