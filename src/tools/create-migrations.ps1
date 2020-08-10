Param(
  [Parameter(Mandatory = $true)]
  [string] $MigrationName
)

Push-Location

Get-ChildItem MigrationCreation.* `
  | ForEach-Object `
  {
      Set-Location $_
      ./create-migration.ps1 $MigrationName
  }

Pop-Location
