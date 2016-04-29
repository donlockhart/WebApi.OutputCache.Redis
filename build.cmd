@echo off
cls

ECHO.
ECHO Building Primary Nuget Package WebApi.OutputCache.Redis
ECHO =======================================================

nuget pack WebApi.OutputCache.Redis\WebApi.OutputCache.Redis.csproj -build -Prop Configuration=Release -IncludeReferencedProjects -OutputDirectory artifacts
