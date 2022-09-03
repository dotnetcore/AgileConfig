FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

# 安装所需依赖
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        wget \
        gdb \
        lldb

# 安装 procdump
RUN wget https://packages.microsoft.com/repos/microsoft-debian-buster-prod/pool/main/p/procdump/procdump_1.1.1-220_amd64.deb -O procdump.deb \
    && dpkg -i procdump.deb \
    && rm procdump.deb

RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf
RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /usr/lib/ssl/openssl.cnf
RUN sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' /usr/lib/ssl/openssl.cnf
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
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

RUN echo "#!/bin/bash \n\
procdump -M 100 -w dotnet & \n\
dotnet \$1 \n\
" > ./start.sh
RUN chmod +x ./start.sh

ENTRYPOINT ["./start.sh", "AgileConfig.Server.Apisite.dll"]
