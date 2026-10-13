# Bouwt Lumen en maakt de Windows-installer (artifacts\installer\Lumen-Setup-<versie>-x64.exe).
# Vereist: .NET 8 SDK en Inno Setup 6 (winget install JRSoftware.InnoSetup)
param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$publishDir = Join-Path $root "artifacts\publish\win-x64"

Write-Host "==> Publiceren ($Version)..." -ForegroundColor Cyan
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish (Join-Path $root "src\Lumen\Lumen.csproj") `
    -c Release -r win-x64 --self-contained true `
    -p:Version=$Version -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish is mislukt" }

$isccCommand = Get-Command iscc.exe -ErrorAction SilentlyContinue
$iscc = if ($isccCommand) { $isccCommand.Source } else { $null }
if (-not $iscc) { $iscc = Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe" }
if (-not (Test-Path $iscc)) { throw "Inno Setup 6 niet gevonden. Installeer met: winget install JRSoftware.InnoSetup" }

Write-Host "==> Installer maken..." -ForegroundColor Cyan
& $iscc "/DAppVersion=$Version" "/DSourceDir=$publishDir" (Join-Path $PSScriptRoot "Lumen.iss")
if ($LASTEXITCODE -ne 0) { throw "Inno Setup is mislukt" }

Write-Host "==> Klaar: artifacts\installer" -ForegroundColor Green
