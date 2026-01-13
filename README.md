# Trivia App

Decisions made during the implementation of the service are documented using Architectural Decisions Records in [`docs/ADRs.md`](./docs/ADRs.md)

## Instructions

This repo uses .NET Aspire to orchestrate the services in the application.

Use the following docs to install what you need:

- .NET (Core) SDK: https://dotnet.microsoft.com/en-us/download
  - https://learn.microsoft.com/en-us/dotnet/core/install/windows
  - https://learn.microsoft.com/en-us/dotnet/core/install/linux
- .NET Aspire setup:https://aspire.dev/get-started/install-cli/
- Docker Desktop/Engine (for PostgreSQL and other containers): https://docs.docker.com/get-docker/
- Node.js + npm (for the frontend): https://nodejs.org/en/download/

Once those are installed, you should be able to run `dotnet aspire run` in the root of this repository, which will open a dashboard.
