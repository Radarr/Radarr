# Changelog

All notable changes to Aletheia are documented in this file.

## [Unreleased]

### Added
- **Phase 2: Multi-Media Infrastructure** (December 2025)
  - Book and Audiobook entity models with database migrations
  - Author and Series hierarchical entities for book organization
  - Full CRUD API controllers for all new media types
  - Frontend pages: Author details, Series details, Book details, Audiobook details
  - Navigation sidebar entries for new media types
  - Redux store actions and selectors for new entities
  - Lookup and editor modals for books and audiobooks
  - Quality definitions for EPUB, MOBI, PDF, M4B, FLAC

### Changed
- **Database Schema** - MediaType discriminator added to base entities
- **Indexers** - SupportedMediaTypes property enables multi-media indexer filtering
- **Code Quality** - Removed unused private fields, modernized JS patterns

### Fixed
- SonarCloud code quality issues (PR #131)
  - Removed 9 unused private fields from service classes
  - Object.assign → spread syntax in Redux actions
  - parseInt → Number.parseInt for consistency
  - Added readonly modifiers to React component props
  - Fixed logging exception parameters

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
