# How to setup local environment

Provider code is not tied to specific Sitecore version. Whenever you want to develop or test against specific Sitecore version you can ... But that flexibility has back side. Some manual steps should be performed to make solution compilable.

1. Copy Sitecore DLLs for specific Sitecore version to **Libs/Sitecore** folder:
    * sitecore.nexus.dll
    * Newtonsoft.Json.dll
    * Sitecore.ContentSearch.dll
    * Sitecore.ContentSearch.Linq.dll
    * Sitecore.Kernel.dll

2.  Copy **sitecore-version.props** file into solution root from **Automation\Sitecore-Versions\{SitecoreVersion}** 

## Setup Local Environment using PS script

Once you cloned the repo and **prior to opening the solution in Visual Studio** please do the following:

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
