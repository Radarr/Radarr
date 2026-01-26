# syntax=docker/dockerfile:1
ARG BASE_IMAGE=ghcr.io/linuxserver/radarr:6.1.1-develop
FROM ${BASE_IMAGE}

ARG BUILD_DATE
ARG VERSION
ARG RADARR_BRANCH=develop
ARG PACKAGE_AUTHOR="github.com/realzombee/Radarr"
ARG RADARR_REPO="realzombee/Radarr"
ARG TARGETARCH

LABEL org.opencontainers.image.created="${BUILD_DATE}" \
  org.opencontainers.image.source="${PACKAGE_AUTHOR}" \
  org.opencontainers.image.version="${VERSION}"

RUN apk add --no-cache curl tar && \
  mkdir -p /app/radarr/bin /tmp/radarr && \
  case "$TARGETARCH" in \
    amd64) runtime="linux-musl-core-x64" ;; \
    arm64) runtime="linux-musl-core-arm64" ;; \
    *) echo "Unsupported TARGETARCH: $TARGETARCH" >&2; exit 1 ;; \
  esac && \
  curl -fsSL -o /tmp/radarr/radarr.tar.gz \
    "https://github.com/${RADARR_REPO}/releases/download/v${VERSION}/Radarr.${RADARR_BRANCH}.${VERSION}.${runtime}.tar.gz" && \
  rm -rf /app/radarr/bin/* && \
  tar -xzf /tmp/radarr/radarr.tar.gz -C /app/radarr/bin --strip-components=1 && \
  echo -e "UpdateMethod=docker\nBranch=${RADARR_BRANCH}\nPackageVersion=${VERSION:-LocalBuild}\nPackageAuthor=${PACKAGE_AUTHOR}" > /app/radarr/package_info && \
  printf "Linuxserver.io version: ${VERSION}\nBuild-date: ${BUILD_DATE}" > /build_version && \
  echo "**** cleanup ****" && \
  rm -rf \
    /app/radarr/bin/Radarr.Update \
    /tmp/*
