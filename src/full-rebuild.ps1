Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }
# dotnet nuget locals --clear all

dotnet restore
dotnet build --configuration Release
dotnet pack --configuration Release --output ./packages --no-build
# cd packages
# dotnet nuget push *.nupkg --source nuget.org --api-key {API_KEY_HERE}
