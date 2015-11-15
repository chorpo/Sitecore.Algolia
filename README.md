# Score Algolia search provider

## Setup Local Environment

Once you cloned the repo and **prior to opening the solution in Visual Studio** please do the following:

1. Check-out `score-automation` repository
2. Run `setup.ps1` script for desired Sitecore version. 
Sample script for 7.2 Update 4 -  `powershell -File ../score-automation/setup.ps1 -sitecoreVersion 7.2.150407`.
Sample script for 8.0 Update 3 -  `powershell -File ../score-automation/setup.ps1 -sitecoreVersion 8.0.150427`.
Sample script for 8.1 Initial  -  `powershell -File ../score-automation/setup.ps1 -sitecoreVersion 8.1.151003`. 
    * If Powershell complains about execution policy please do the following and try again
        * Run the `Command Prompt` with administrative privileges
        * Execute `powershell Set-ExecutionPolicy RemoteSigned`
    * If copy process complains about a particular Sitecore DLL being in use just run the setup script again
3. Copy Sitecore lincese xml file into `Score.ContentSearch.Algolia.Tests` folder

                                                                  


