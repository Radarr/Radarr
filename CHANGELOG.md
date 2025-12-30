# Changelog

All notable changes to Aletheia are documented in this file.

## [Unreleased]

### Added
- **Phase 3: Multi-Media Foundation** (December 2025)
  - ConfigService split into focused services (UIConfig, ProxyConfig, DownloadConfig, ImportConfig)
  - BaseMediaCrudController extracted for all media type controllers
  - BaseMediaEditorController extracted for bulk edit operations
  - IMediaResource interface for resource type consolidation
  - MediaItem base entity with MediaType discriminator
  - Database migrations: 244 (MediaType), 246 (Books/Audiobooks), 250 (Music)

- **Phase 4: Books & Audiobooks** (December 2025)
  - Book entity with Author, ISBN, Publisher, Description fields
  - Audiobook entity with Narrator, Duration, IsAbridged fields
  - Author and Series hierarchical entities for organization
  - Book qualities: EPUB (101), MOBI (102), AZW3 (103), PDF (104), TXT (105), CBR (106), CBZ (107)
  - Audiobook qualities: MP3-128 (201), MP3-320 (202), M4B (203), AudioFLAC (204)
  - BookQualityParser and AudiobookQualityParser with regex timeout protection
  - Full CRUD API controllers for all new media types
  - Frontend pages: Author details, Series details, Book details, Audiobook details
  - OpenLibrary metadata provider (BookInfoProxy)
  - AudiobookInfoProxy with narrator support

- **Phase 5: TV Shows** (December 2025) - PR #149
  - TVShow, Season, Episode entities with hierarchical relationships
  - EpisodeFile entity for downloaded content tracking
  - TVShowStatus and SeriesType enums for series metadata
  - TVDbProxy metadata provider for series/episode lookup
  - TVParser with scene numbering support
  - Full CRUD API controllers (TVShowController, SeasonController, EpisodeController)
  - Frontend pages: TV Shows index, show details with season/episode grids
  - Season/episode-level monitoring with cascade logic
  - Integration with existing quality profiles and indexer system

- **Phase 6: Music Foundation** (December 2025) - PR #147
  - Artist, Album, Track entities with hierarchical relationships
  - 60+ music quality definitions covering:
    - Standard formats: MP3 (128/192/256/320/VBR), AAC, OGG Vorbis
    - Lossless: FLAC, ALAC, WAV, AIFF, APE, WavPack
    - Hi-Res: 24-bit depths (44.1-384 kHz), DSD64/128/256/512
    - Immersive: Dolby Atmos, Sony 360 Reality Audio, DTS:X
    - Special: Vinyl rips, SHM-SACD, MQA
  - MusicQualityParser with comprehensive format detection
  - ArtistRepository, AlbumRepository, TrackRepository
  - ArtistService, AlbumService with hierarchical monitoring
  - Music API layer (ArtistController, AlbumController, TrackController)
  - ArtistLookupController, AlbumLookupController for search
  - MusicBrainzProxy metadata provider

### Changed
- **Database Schema** - MediaType discriminator added to base entities
- **Indexers** - SupportedMediaTypes property enables multi-media indexer filtering
- **Code Quality** - Extracted methods for cognitive complexity, added regex timeouts

### Fixed
- SonarCloud code quality issues (PR #131, #147)
  - Removed 9 unused private fields from service classes
  - Object.assign → spread syntax in Redux actions
  - parseInt → Number.parseInt for consistency
  - Added readonly modifiers to React component props
  - Fixed logging exception parameters
  - S6444: Added 5s regex timeout to MusicQualityParser, BookQualityParser, AudiobookQualityParser
  - S3776: Extracted ParseFormatMatch/ParseBitrateMatch in AudiobookQualityParser
  - S4136: Reordered ToModel overloads in AlbumResource, ArtistResource
  - S1192: Extracted constants in MusicBrainzProxy

### Security
- Fix SQL injection in CleanupUnusedTags.cs - use parameterized Dapper queries
- Fix path traversal in ArchiveService.cs - validate ZIP entries stay within destination
- Fix path traversal in StaticResourceMapper.cs - validate paths stay within UI folder
- Fix path traversal in MediaCoverMapper.cs - validate paths stay within AppData folder
- Fix command injection in ProcessProvider.cs - quote script paths for .bat/.ps1/.py

### Changed (Earlier)
- **UI Branding** - Radarr yellow (#ffc230) → Aletheia teal (#0d9488)
  - Updated dark.js and light.js theme files
  - New logo.svg with teal gradient and lambda/L symbol
  - Generated all PNG logos and favicons
  - Updated manifest.json theme colors
  - Updated page titles, meta descriptions, external links
  - Changed appName token from 'Radarr' to 'Aletheia' in translations

## [0.1.0] - 2024-12-17 - Initial Fork

### Added
- Fork of Radarr v5.x as foundation for unified media manager
- Aletheia branding throughout application
  - BuildInfo.cs AppName property set to "Aletheia"
  - UI localization strings (en.json) updated with Aletheia branding
  - Docker labels and metadata identify as "Aletheia"
- GitHub Actions CI/CD workflow for continuous integration
- Docker configuration with multi-architecture support (amd64, arm64)
- Project documentation structure and contribution guidelines

### Changed
- **Application Identity** - Radarr → Aletheia
  - Before: Application branded as "Radarr" throughout codebase
  - After: Application branded as "Aletheia" (ἀλήθεια - truth, disclosure)
  - Rationale: Fork establishes distinct identity while retaining proven Radarr architecture
  - Gotcha: Docker images and config references still contain "radarr" in internal paths

### Notes
- Movie functionality preserved from Radarr v5.x
- Hierarchical monitoring system (Author → Series → Item) is foundational design goal
- Radarr codebase remains the authoritative upstream reference for inherited functionality
