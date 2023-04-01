# Libraries-Common

A Common Library in .NET 7 which will be used by multiple microservices

## Create and publish package to Local Folder using dotnet CLI

```dotnetcli
dotnet new classlib -n CommonLibrary 

dotnet clean
dotnet build
dotnet pack -o C:\LordKrishna\Packages\

dotnet nuget add source C:\LordKrishna\Packages -n Local-Packages
```

## Create and publish package to GitHub using PowerShell

```powershell
$version="1.0.13"
$owner="Microservices-for-Small-App"
$username="vishipayyallore"
$repo="Libraries-Common"
$gh_pat="[PAT HERE]"

dotnet clean
dotnet build -c Release
dotnet pack --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/$repo -o ..\..\packages

dotnet nuget push ..\..\packages\CommonLibrary.$version.nupkg --api-key $gh_pat --source "github"
```
