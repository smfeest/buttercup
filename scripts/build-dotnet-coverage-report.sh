#!/usr/bin/env bash

set -euo pipefail
pushd "$( dirname "${BASH_SOURCE[0]}" )/.."

dotnet reportgenerator \
  -assemblyfilters:-Buttercup.EntityModel.Migrations \
  -filefilters:-*.cshtml\;-*.g.cs \
  -reports:**/coverage.cobertura.xml \
  -targetdir:coverage \
  "$@"
