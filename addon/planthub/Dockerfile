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

# Build args
ARG TARGETARCH
ARG BUILD_VERSION

# Copy application source from repo root
# Home Assistant provides the entire repository as build context
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

# Optional sanity check (can be removed later)
RUN set -eux; \
    echo "== publish output =="; \
    ls -la /out; \
    ls -la /out/wwwroot || true

# =======================
# Final stage (Home Assistant base image)
# =======================
FROM $BUILD_FROM

# Label image with add-on version
LABEL io.hass.version=$BUILD_VERSION

# Copy published app and s6 scripts
COPY --from=build /out /opt/planthub/
COPY rootfs/ /

# Run from app directory (important for ContentRoot/WebRoot)
WORKDIR /opt/planthub

# Ensure s6 scripts are executable
RUN chmod +x /etc/services.d/planthub/run /etc/cont-init.d/10-config || true

# Runtime environment
ENV ASPNETCORE_URLS=http://0.0.0.0:8099
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV DOTNET_EnableDiagnostics=0
