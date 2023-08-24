dotnet restore
dotnet build --configuration Release --no-restore --no-incremental
dotnet pack --configuration Release --output ./packages --no-build
# cd packages
# dotnet nuget push *.nupkg --source nuget.org --api-key {API_KEY_HERE}
