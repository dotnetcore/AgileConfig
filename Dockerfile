FROM microsoft/dotnet:latest

COPY . /app

WORKDIR /app

EXPOSE 5000/tcp

ENTRYPOINT ["dotnet", "AgileConfig.Server.Apisite.dll"]