#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Position = 1)]
    [String]$BaseUrl = "http://localhost:30473"
)

$ErrorActionPreference = "Stop"

$BaseUrl = $BaseUrl.TrimEnd("/")

$versions = @("v1", "v2")
$output = Join-Path $PSScriptRoot "docs" "api-specs"

New-Item $output -ItemType Directory -ErrorAction SilentlyContinue

foreach ($version in $versions) {
    Invoke-WebRequest -Uri "${BaseUrl}/swagger/${version}/swagger.json" -OutFile (Join-Path $output "${version}.json")
}
