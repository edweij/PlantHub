# =========================
# Build stage (Blazor Server)  **ALPINE SDK**
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy solution & project
COPY PlantHub.sln ./
COPY PlantHub.Web/ ./PlantHub.Web/

# Map platform -> musl RID
# Vi försöker först TARGETPLATFORM (buildx), annars kollar vi BUILD_ARCH (HA builder)
ARG TARGETPLATFORM
ARG BUILD_ARCH
RUN echo "TARGETPLATFORM=${TARGETPLATFORM} BUILD_ARCH=${BUILD_ARCH}" && \
    if [ "${TARGETPLATFORM}" = "linux/arm64" ] || [ "${BUILD_ARCH}" = "aarch64" ]; then \
        export RID=linux-musl-arm64; \
    else \
        export RID=linux-musl-x64; \
    fi && \
    dotnet publish PlantHub.Web/PlantHub.Web.csproj \
      -c Release -r $RID \
      --self-contained true \
      -p:PublishSingleFile=true \
      -p:PublishTrimmed=true \
      -p:InvariantGlobalization=true \
      -o /out

# =========================
# Runtime stage (Home Assistant base)
# =========================
FROM ghcr.io/home-assistant/base:14.1.0
WORKDIR /app

# Copy published self-contained app
COPY --from=build /out/ ./

# s6 services & scripts
COPY rootfs/ /
RUN chmod +x /etc/services.d/planthub/run /etc/services.d/planthub/finish

# Ingress internal port
EXPOSE 8099
