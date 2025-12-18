# Aletheia

All-in-one media manager for movies, books, and audiobooks.

## Overview

Aletheia (from Greek ἀλήθεια - "truth, disclosure") is a unified media management system forked from Radarr. It provides automated monitoring, downloading, and library management for multiple media types through a single interface.
It's an ambitious attemp to merge much of the functionality of the arr apps. This, in addition to many personal feature requests, QoL improvements, and privacy/security updates.  

**Current Status:** Active development. Movie functionality inherited from Radarr is working. Multi-media foundation being implemented.

## Features

**Movies (working):**
- Automated monitoring and quality upgrades
- Metadata and artwork management
- Integration with download clients and indexers

**Books (in development):**
- EPUB, MOBI, PDF quality tracking
- Author and series hierarchy
- Goodreads/Hardcover metadata

**Audiobooks (in development):**
- M4B, MP3, etc. support
- Narrator tracking
- Duration metadata and Audible integration

**General:**
- Usenet and BitTorrent support
- SABnzbd, NZBGet, qBittorrent, Deluge, rTorrent, Transmission integration
- Plex and Kodi integration
- Built-in archive extraction (Unpackerr functionality)

## Privacy

Telemetry and analytics are **disabled by default**:

- No usage analytics or behavior tracking
- No machine fingerprinting
- Error reporting is opt-in

To enable error reporting, toggle Analytics in Settings → General.

## Quick Start

```bash
docker run -d \
  --name=aletheia \
  -e PUID=1000 \
  -e PGID=1000 \
  -p 7878:7878 \
  -v /path/to/config:/config \
  -v /path/to/media:/media \
  --restart unless-stopped \
  ghcr.io/cheir-mneme/aletheia:latest
```

Web interface: `http://localhost:7878`

## Building from Source

Requirements: .NET 8.0 SDK, Node.js 20+, Yarn

```bash
git clone https://github.com/cheir-mneme/aletheia.git
cd aletheia
./build.sh --backend --frontend
dotnet run --project src/Radarr
```

## Roadmap

See [ROADMAP.md](../ROADMAP.md) for detailed phase planning.

**Completed:**
- Phase 0-1: Privacy & security fixes
- Phase 2: Foundation (fork, CI/CD, branding)
- Phase 2.5: Community standards, quality gates, Unpackerr absorption

**Current:**
- Phase 3: Multi-media foundation (database generalization, indexer management)

**Planned:**
- Phase 4: Books & audiobooks support
- Phase 5: TV shows
- Phase 6: Music (with fingerprinting and quality analysis)
- Phase 7: Subtitles (Bazarr replacement), podcasts, comics

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup, code guidelines, and PR process.

**Development standards:**
- Conventional commits (`feat:`, `fix:`, `docs:`, etc.)
- Feature branches + PRs to `develop`
- Pre-commit hooks for linting

## License

[GNU GPL v3](http://www.gnu.org/licenses/gpl.html)

Aletheia is a derivative of [Radarr](https://github.com/Radarr/Radarr). Copyright 2010-2025.
