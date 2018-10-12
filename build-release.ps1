$dist= "dist"
$sln = ".\Thinktecture.Relay.sln"
$mgmtWeb = "Thinktecture.Relay.ManagementWeb\dist\*";

# Check for VS Command Prompt
If(![Environment]::GetEnvironmentVariable('DevEnvDir')){
	Write-Host "Error: Release builds must be run on a VS 2017 Developer Command Prompt"
	exit 1
}

# Check for .NET Core SDK
If(!(Get-Command "dotnet.exe" -ErrorAction SilentlyContinue)){
	Write-Host "Error: Release builds require an installed .NET Core SDK"
	exit 1
}

# Check for required node version
$nodeVer = node --version
If(!$nodeVer.StartsWith("v6.")){
	Write-Host "Error: Release builds require NODE version 6.x installed (preferably 6.14.4)"
	exit 1
}

# Prepare output
Write-Host "Prepare dist folder"
If(Test-Path $dist) {
	Remove-Item -Recurse -Force $dist
}
New-Item -ItemType Directory -Path $dist

# Build Management Web
Write-Host "Building management web"
$nm = "node_modules"
If(Test-Path $nm) {
	Remove-Item -Recurse -Force $nm
}
npm install
npm run build
Compress-Archive -Path $mgmtWeb -DestinationPath (Join-Path $dist "\ManagementWeb.zip")

# Build Solution
.\delete-bin-and-obj-folders.ps1
$packages = "packages"
If(Test-Path $packages) {
	Remove-Item -Recurse -Force $packages
}

$nuget = "nuget.exe"
If(!(Test-Path $nuget -PathType Leaf)) {
	Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $nuget
}
.\nuget.exe restore $sln

msbuild $sln /p:Configuration=Release

$srvRelease = ".\Thinktecture.Relay.Server\bin\Release\"
$mgmtWebTargetPath = Join-Path $srvRelease "ManagementWeb\"
New-Item -ItemType Directory -Path $mgmtWebTargetPath
Copy-Item -Recurse -Path $mgmtWeb -destination $mgmtWebTargetPath
Compress-Archive -Path (Join-Path $srvRelease "*") -DestinationPath (Join-Path $dist "\RelayServer.zip")

# Build Nuget Packages
dotnet pack ".\Thinktecture.Relay\" --configuration Release --include-symbols --include-source --output (Join-Path ".." $dist)
dotnet pack ".\Thinktecture.Relay.OnPremiseConnector\" --configuration Release --include-symbols --include-source --output (Join-Path ".." $dist)
