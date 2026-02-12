#!/bin/bash

# Path to your project
PROJECT_PATH="../Api/Api.csproj"

# Set the environment to Development
export ASPNETCORE_ENVIRONMENT=Development

dotnet run --project "$PROJECT_PATH" --launch-profile Development
