# syntax=docker/dockerfile:1
# 多架构构建：由 buildx 通过 --platform 驱动，无需为 arm64 单独维护分支。
# 例：docker buildx build --platform linux/amd64,linux/arm64 -t kklldog/agile_config:latest --push .

# 运行时基础镜像，架构由 buildx 的目标平台自动选择
FROM mcr.microsoft.com/dotnet/aspnet:10.0.0 AS base
RUN for f in /etc/ssl/openssl.cnf /usr/lib/ssl/openssl.cnf; do \
        if [ -f "$f" ]; then \
            sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' "$f"; \
            sed -i 's/MinProtocol = TLSv1.2/MinProtocol = TLSv1/g' "$f"; \
        fi; \
    done
WORKDIR /app
EXPOSE 5000

# 前端与架构无关，固定跑在构建机原生平台上，避免 QEMU 模拟拖慢 npm
FROM --platform=$BUILDPLATFORM node:20 AS ui-build
WORKDIR /src/AgileConfig.Server.UI/react-ui-antd
COPY ["src/AgileConfig.Server.UI/react-ui-antd/package.json", "src/AgileConfig.Server.UI/react-ui-antd/package-lock.json", "./"]
RUN npm ci
COPY ["src/AgileConfig.Server.UI/react-ui-antd/.", "./"]
RUN npm run build

# SDK 同样跑在原生平台，用 -r 交叉编译到目标架构（比 QEMU 模拟快很多）
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
# linux-x64 / linux-arm64
RUN case "${TARGETARCH}" in \
        amd64) echo linux-x64   > /tmp/rid ;; \
        arm64) echo linux-arm64 > /tmp/rid ;; \
        *) echo "unsupported platform: ${TARGETARCH}" >&2; exit 1 ;; \
    esac
WORKDIR /src
COPY ["src/AgileConfig.Server.Apisite/AgileConfig.Server.Apisite.csproj", "AgileConfig.Server.Apisite/"]
COPY ["src/AgileConfig.Server.Data.Entity/AgileConfig.Server.Data.Entity.csproj", "AgileConfig.Server.Data.Entity/"]
COPY ["src/Agile.Config.Protocol/Agile.Config.Protocol.csproj", "Agile.Config.Protocol/"]
COPY ["src/AgileConfig.Server.Service/AgileConfig.Server.Service.csproj", "AgileConfig.Server.Service/"]
COPY ["src/AgileConfig.Server.IService/AgileConfig.Server.IService.csproj", "AgileConfig.Server.IService/"]
COPY ["src/AgileConfig.Server.Data.Freesql/AgileConfig.Server.Data.Freesql.csproj", "AgileConfig.Server.Data.Freesql/"]
COPY ["src/AgileConfig.Server.Common/AgileConfig.Server.Common.csproj", "AgileConfig.Server.Common/"]
COPY ["src/AgileConfig.Server.OIDC/AgileConfig.Server.OIDC.csproj", "AgileConfig.Server.OIDC/"]

RUN dotnet restore "AgileConfig.Server.Apisite/AgileConfig.Server.Apisite.csproj" -r "$(cat /tmp/rid)"
COPY src/. .
COPY --from=ui-build /src/AgileConfig.Server.UI/react-ui-antd/dist ./AgileConfig.Server.Apisite/wwwroot/ui
WORKDIR "/src/AgileConfig.Server.Apisite"

FROM build AS publish
RUN dotnet publish "AgileConfig.Server.Apisite.csproj" -c Release -o /app/publish \
        -r "$(cat /tmp/rid)" --self-contained false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AgileConfig.Server.Apisite.dll"]
