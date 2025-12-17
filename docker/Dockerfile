FROM ghcr.io/linuxserver/baseimage-alpine:3.21

ARG TARGETPLATFORM

ENV XDG_CONFIG_HOME="/config/xdg" \
    COMPlus_EnableDiagnostics=0 \
    TMPDIR=/run/radarr-temp

RUN apk add -U --upgrade --no-cache \
    icu-libs \
    sqlite-libs \
    xmlstarlet

ARG GIT_BRANCH
ARG COMMIT_HASH
ARG BUILD_DATE

LABEL maintainer="cody-k" \
      org.opencontainers.image.title="Logarr" \
      org.opencontainers.image.description="All-in-one media manager (Radarr fork)" \
      org.opencontainers.image.source="https://github.com/Cody-k/logarr"

RUN mkdir -p /app/radarr

RUN --mount=type=bind,source=_output,target=_output \
    case "$TARGETPLATFORM" in \
    "linux/amd64") cp -r /_output/net8.0/linux-musl-x64 /app/radarr/bin ;; \
    "linux/arm64") cp -r /_output/net8.0/linux-musl-arm64 /app/radarr/bin ;; \
    "darwin/amd64") cp -r /_output/net8.0/osx-x64 /app/radarr/bin ;; \
    "darwin/arm64") cp -r /_output/net8.0/osx-arm64 /app/radarr/bin ;; \
    "windows/amd64") cp -r /_output/net8.0/win-x64 /app/radarr/bin ;; \
    *) echo "Unknown platform: $TARGETPLATFORM" && exit 1 ;; \
    esac; \
    cp -r /_output/UI /app/radarr/bin/UI

RUN echo -e "UpdateMethod=docker\nBranch=${GIT_BRANCH}\nPackageVersion=${COMMIT_HASH}}\nPackageAuthor=cody-k" > /app/radarr/package_info && \
    printf "version: ${COMMIT_HASH}}\nBuild-date: ${BUILD_DATE}" > /build_version

COPY docker/root/ /

EXPOSE 7878

VOLUME /config
