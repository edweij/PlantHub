# =========================
# Build stage (Blazor Server)
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution & projects (we'll add these files in later steps)
# If you rename projects/solution later, update paths below accordingly.
COPY PlantHub.sln ./
COPY PlantHub.Web/ ./PlantHub.Web/

# Pick RID based on TARGETPLATFORM so we can build multi-arch images
# (linux-x64 for amd64, linux-arm64 for aarch64)
ARG TARGETPLATFORM
RUN echo "Building for TARGETPLATFORM=${TARGETPLATFORM}" && \
    if [ "${TARGETPLATFORM}" = "linux/arm64" ]; then export RID=linux-arm64; else export RID=linux-x64; fi && \
    dotnet publish PlantHub.Web/PlantHub.Web.csproj \
      -c Release -r $RID \
      -p:PublishSingleFile=true \
      -p:PublishTrimmed=true \
      --self-contained true \
      -o /out

# =========================
# Runtime stage (Home Assistant base)
# =========================
FROM ghcr.io/home-assistant/base:14.1.0

# Keep image small and deterministic
ENV ASPNETCORE_URLS=http://0.0.0.0:8099 \
    DOTNET_EnableDiagnostics=0 \
    COMPlus_EnableDiagnostics=0

WORKDIR /app
COPY --from=build /out/ ./

# s6 services & scripts (we add these files in the repo under rootfs/)
COPY rootfs/ /
RUN chmod +x /etc/services.d/planthub/run /etc/services.d/planthub/finish

# Ingress uses internal port only; don't publish externally
EXPOSE 8099

# s6-overlay will run our /etc/services.d/planthub/run automatically
