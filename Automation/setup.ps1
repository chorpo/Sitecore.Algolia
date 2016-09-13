Param (
    [Parameter(Mandatory=$False)]
    [string]$solution,

    [Parameter(Mandatory=$True)]
    [string]$sitecoreVersion
)

function CopyFile($file, $destination)
{
    "Copying - $file ==> $destination"

    if (!(Test-Path $file))
    {
        Write-Host "-- Cannot Copy File $($file) - FILE NOT FOUND" -foregroundcolor "yellow"
        return
    }

    Copy-Item -force "$file" "$destination"
}

function ReplaceInContent($file, $regex, $replace)
{

    if (!(Test-Path $file))
    {
        Write-Host "-- Cannot Replace content in file $($file) - FILE NOT FOUND" -foregroundcolor "yellow"
        return
    }
	

    (Get-Content $file) | ForEach-Object { $_ -replace $regex, $replace} `
                        | Out-File $file -Force -Encoding ascii
}
 
#try to figure-out solutionName ($solution) based on files in current folder
if(-Not $solution)
{
    $solutionNames = Get-ChildItem .\*.sln  | select BaseName
    
    $solutionsCount = @($solutionNames).Count

    if ($solutionCount -eq 0)
    {
        Write-Host "No VS Solutions (*.sln) detected in folder" 
        return    
    }

    if ($solutionCount -gt 1)
    {
        Write-Host "Multiple VS Solutions detected in forlder. Cannot Resolve solution."
        return    
    }

    $solution = $solutionNames[0].BaseName
    Write-Host "Solution Name is detected based on files in current folder. Name - '$solutionName'"    
}


    if (-Not $solution)
    {
        throw "Solution parameter is not provided and script cannot autodetect it"
    }

$sitecore = @{"Version"="NA"; "Revision"="NA";}

If ($sitecoreVersion -match '(\d\.\d)\.(\d{6})') {

    $sitecore.Version = $matches[1]
    $sitecore.Revision = $matches[2]

} Else {
    throw "$sitecoreVersion version doesn't match the expected d.d.dddddd format for Sitecore version"
}

" "
"Generate sitecore-version.props for $($sitecore.Version).$($sitecore.Revision)"
"=============================================="

@"
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <SitecoreVersion>$($sitecore.Version)</SitecoreVersion>
    <SitecoreRevision>$($sitecore.Revision)</SitecoreRevision>
  </PropertyGroup>
</Project>
"@ | Out-File ".\sitecore-version.props" -Encoding ascii

" "
"For Sitecore $($sitecore.Version) - Copy pre-built config files"
"==============================================================="

# Copy packages.config with Sitecore references to all projects
Get-ChildItem -Path . -Filter "$solution*" | ?{ $_.PSIsContainer } |`
  %{ CopyFile ".\Automation\Sitecore-Versions\$($sitecore.Version)\packages.config" $_.Name }

# Copy references
CopyFile ".\Automation\Sitecore-Versions\$($sitecore.Version)\references-qualified.proj" .

Get-ChildItem -Path . -Filter "$solution*Tests" | ?{ $_.PSIsContainer } |`
  %{ CopyFile ".\Automation\Sitecore-Versions\$($sitecore.Version)\App.config" $_.Name }

" "
"Set packges and references to work with Sitecore $($sitecore.Version).$($sitecore.Revision)"
"=============================================================="
$configs = Get-ChildItem $solution*\packages.config
foreach ($config in $configs)
{
    "Patching - $config"
    ReplaceInContent $config "\$\{sitecore\.version\}" "$($sitecore.Version).$($sitecore.Revision)"
}

"Patching - references-qualified.proj"
ReplaceInContent ".\references-qualified.proj" "\$\{sitecore\.version\}" "$($sitecore.Version).$($sitecore.Revision)"

" "
"Restoring NuGet packages for solution $solution.sln"
"========================"
.\.nuget\nuget.exe restore "$solution.sln"

" "
Write-Host "All Done" -foregroundcolor "green"
