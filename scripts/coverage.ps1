#!/usr/bin/env pwsh
# Ejecuta la suite completa con recoleccion de cobertura y genera el informe HTML
# en docs/coverage/. Reproduce lo que hace ARCHITECTURE.md §10.

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

if (Test-Path "coverage-raw") {
    Remove-Item -Recurse -Force "coverage-raw"
}

dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage-raw
if ($LASTEXITCODE -ne 0) {
    throw "dotnet test terminó con errores."
}

if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "Instalando dotnet-reportgenerator-globaltool..."
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

reportgenerator `
    -reports:"coverage-raw/**/coverage.cobertura.xml" `
    -targetdir:"docs/coverage" `
    -reporttypes:"Html;TextSummary"

Write-Host "Informe generado en docs/coverage/index.html"
