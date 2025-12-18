# Logarr

All-in-one media manager for movies, books, and audiobooks.

## Overview

Logarr (from Greek λόγος - "word, reason") is a unified media management system forked from Radarr. It provides automated monitoring, downloading, and library management for multiple media types through a single interface.

**Current Status:** Early development. Movie functionality inherited from Radarr is working. Book and audiobook support is planned.

## Features

**Movies (working):**
- Automated monitoring and quality upgrades
- Metadata and artwork management
- Integration with download clients and indexers

**Books (planned):**
- EPUB, MOBI, PDF quality tracking
- Author and series hierarchy
- Goodreads/Hardcover metadata

**Audiobooks (planned):**
- M4B, MP3, FLAC support
- Narrator tracking and duration metadata
- Audible metadata integration

**General:**
- Usenet and BitTorrent support
- SABnzbd, NZBGet, qBittorrent, Deluge, rTorrent, Transmission integration
- Plex and Kodi integration

## Quick Start

```bash
docker run -d \
  --name=logarr \
  -e PUID=1000 \
  -e PGID=1000 \
  -p 7878:7878 \
  -v /path/to/config:/config \
  -v /path/to/media:/media \
  --restart unless-stopped \
  ghcr.io/cody-k/logarr:latest
```

Web interface: `http://localhost:7878`

## Building from Source

Requirements: .NET 8.0 SDK, Node.js 20+, Yarn

```bash
git clone https://github.com/Cody-k/logarr.git
cd logarr
./build.sh --backend --frontend
dotnet run --project src/Radarr
```

## Roadmap

1. **Foundation** - Generalize database schema, implement hierarchical monitoring (Author → Series → Item)
2. **Multi-Media** - Add book and audiobook quality profiles, port metadata providers
3. **Interface** - Unified dashboard with type filters, type-specific detail views

## Contributing

Early development phase. Not currently accepting contributions while core architecture is established.

## License

[GNU GPL v3](http://www.gnu.org/licenses/gpl.html)

Logarr is a derivative of [Radarr](https://github.com/Radarr/Radarr). Copyright 2010-2025.
