# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

AgileConfig is a lightweight, distributed configuration center for .NET (comparable to Apollo/Nacos). It supports Docker/IIS deployment, distributed multi-node clustering, multiple environments (DEV/TEST/STAGING/PROD), real-time config push over WebSocket, `IConfiguration`/`IOptions` client integration, a RESTful API, version management with rollback, OIDC/SSO, OpenTelemetry, and can also act as a simple service registry. The client SDK lives in a separate repo (git submodule `AgileConfig.Client`, published on NuGet as `AgileConfig.Client`) and is not part of this codebase.

## Build, test, and run

- Target framework: `net10.0` across all projects.
- Build: `dotnet build AgileConfig.sln`
- Run all tests: `dotnet test`
- Run one test project: `dotnet test test/AgileConfig.Server.ServiceTests/AgileConfig.Server.ServiceTests.csproj`
- Test framework is **MSTest** (with Moq for mocking). `AgileConfig.Server.ServiceTests` uses **Testcontainers** (MongoDB/MySQL/PostgreSQL) for integration tests, which requires Docker to be running locally.
- Admin UI frontend (Ant Design Pro / UmiJS, in `src/AgileConfig.Server.UI/react-ui-antd`): `npm install && npm run build`; `npm test` for frontend tests.
- Run the server directly: `dotnet run --project src/AgileConfig.Server.Apisite`, or via Docker: `docker run -e adminConsole=true -e db__provider=sqlite -e db__conn="Data Source=agile_config.db" -p 5000:5000 kklldog/agile_config:latest`.

## Architecture

This is a client-server config center. A "node" is one running instance of `AgileConfig.Server.Apisite`; any node can double as the admin console (`adminConsole=true`), and nodes cluster together (`cluster=true`) by auto-discovering and proxying each other (`RemoteServerProxyController`, `ServerNodeController`).

**Communication**: clients talk to a node over **WebSocket** for real-time config push (`src/AgileConfig.Server.Apisite/Websocket/` — `WebsocketHandlerMiddleware.cs`, `WebsocketCollection.cs`, `MessageHandlers/`), with message contracts defined in the shared `Agile.Config.Protocol` project. Config fetch/management also goes over REST (`Controllers/api/` for the client-facing API; top-level `Controllers/` for admin/management endpoints).

**Layering** (dependency direction): `Apisite` → `IService`/`Service` → `Data.Abstraction` → `Data.Repository.{Freesql|Mongodb}` (chosen by `Data.Repository.Selector`) → `Data.{Freesql|Mongodb}` → `Data.Entity`. `Event`/`EventHandler` projects provide in-process domain events for decoupling side effects from the main service logic.

**Storage backends**: selected at runtime via the `db:provider` setting, resolved through `Data.Repository.Selector`. Relational backends (sqlserver, mysql, sqlite, npgsql, oracle) go through FreeSql; `mongodb` goes through the MongoDB driver directly. There is no Redis dependency. Per-environment DB overrides are supported via `db:env:{TEST|STAGING|PROD}` config keys.

**Admin UI**: a separate React + Ant Design Pro (UmiJS) app under `src/AgileConfig.Server.UI/react-ui-antd`, served by the Apisite host through `UIExtension/ReactUIMiddleware.cs`. It is not an MSBuild project and is not part of `AgileConfig.sln` — build/test it independently with npm.

Key runtime config (`src/AgileConfig.Server.Apisite/appsettings.json`): `urls`, `adminConsole`, `cluster`, `saPassword`, `defaultApp`, `pathBase`, `preview_mode`, `db:provider`/`db:conn`, `JwtSetting:*`, `SSO:OIDC:*`, `otlp:*`. Logging is configured via `nlog.config`.

Note: `src/AgileConfig.Server.SyncPlugin*` directories exist on disk but contain only stale build output and are not referenced by the solution — treat them as dead code, not an active plugin system.
