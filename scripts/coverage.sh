#!/usr/bin/env bash
# Ejecuta la suite completa con recoleccion de cobertura y genera el informe HTML
# en docs/coverage/. Reproduce lo que hace ARCHITECTURE.md §10.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$REPO_ROOT"

rm -rf coverage-raw

dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage-raw

if ! command -v reportgenerator >/dev/null 2>&1; then
  echo "Instalando dotnet-reportgenerator-globaltool..."
  dotnet tool install --global dotnet-reportgenerator-globaltool
fi

reportgenerator \
  -reports:"coverage-raw/**/coverage.cobertura.xml" \
  -targetdir:"docs/coverage" \
  -reporttypes:"Html;TextSummary"

echo "Informe generado en docs/coverage/index.html"
