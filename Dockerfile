FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
COPY /. /app
RUN dotnet restore
WORKDIR /app/AgileConfig.Server.Apisite
RUN dotnet publish -o ./out -c Release
EXPOSE 5000
ENTRYPOINT ["dotnet", "out/AgileConfig.Server.Apisite.dll"]
