FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["AgileConfig.Server.Apisite/AgileConfig.Server.Apisite.csproj", "AgileConfig.Server.Apisite/"]
COPY ["AgileConfig.Server.Data.Entity/AgileConfig.Server.Data.Entity.csproj", "AgileConfig.Server.Data.Entity/"]
COPY ["Agile.Config.Protocol/Agile.Config.Protocol.csproj", "Agile.Config.Protocol/"]
COPY ["AgileConfig.Server.Service/AgileConfig.Server.Service.csproj", "AgileConfig.Server.Service/"]
COPY ["AgileConfig.Server.IService/AgileConfig.Server.IService.csproj", "AgileConfig.Server.IService/"]
COPY ["AgileConfig.Server.Data.Freesql/AgileConfig.Server.Data.Freesql.csproj", "AgileConfig.Server.Data.Freesql/"]
COPY ["AgileConfig.Server.Common/AgileConfig.Server.Common.csproj", "AgileConfig.Server.Common/"]
RUN dotnet restore "AgileConfig.Server.Apisite/AgileConfig.Server.Apisite.csproj"
COPY . .
WORKDIR "/src/AgileConfig.Server.Apisite"
RUN dotnet build "AgileConfig.Server.Apisite.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AgileConfig.Server.Apisite.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgileConfig.Server.Apisite.dll"]
