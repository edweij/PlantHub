# =======================
# Global args (must be declared before first FROM)
# =======================
ARG BUILD_FROM
ARG BUILD_ARCH
ARG BUILD_VERSION

# =======================
# Build stage
# =======================
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /build

ARG TARGETARCH
ARG BUILD_VERSION

# Copy application source
COPY src/ ./src/

# Map TARGETARCH -> musl RID
RUN set -eux; \
    RID=linux-musl-x64; \
    if [ "$TARGETARCH" = "arm64" ]; then RID=linux-musl-arm64; \
    elif [ "$TARGETARCH" = "arm" ]; then RID=linux-musl-arm; fi; \
    dotnet publish "src/PlantHub.Web/PlantHub.Web.csproj" \
      -c Release \
      -r "$RID" \
      --self-contained true \
      -o /out

# =======================
# Final stage (Home Assistant base image)
# =======================
FROM $BUILD_FROM

ARG BUILD_VERSION
LABEL io.hass.version=$BUILD_VERSION

RUN echo "Building PlantHub version ${BUILD_VERSION}"

COPY --from=build /out /opt/planthub/
COPY rootfs/ /

WORKDIR /opt/planthub

RUN chmod +x /etc/services.d/planthub/run /etc/cont-init.d/10-config || true

ENV ASPNETCORE_URLS=http://0.0.0.0:8099
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV DOTNET_EnableDiagnostics=0
