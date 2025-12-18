# Changelog

All notable changes to Aletheia are documented in this file.

## [Unreleased]

### Security
- Fix SQL injection in CleanupUnusedTags.cs - use parameterized Dapper queries
- Fix path traversal in ArchiveService.cs - validate ZIP entries stay within destination
- Fix path traversal in StaticResourceMapper.cs - validate paths stay within UI folder
- Fix path traversal in MediaCoverMapper.cs - validate paths stay within AppData folder
- Fix command injection in ProcessProvider.cs - quote script paths for .bat/.ps1/.py

### Changed
- **UI Branding** - Radarr yellow (#ffc230) → Aletheia teal (#0d9488)
  - Updated dark.js and light.js theme files
  - New logo.svg with teal gradient and lambda/L symbol
  - Generated all PNG logos and favicons
  - Updated manifest.json theme colors
  - Updated page titles, meta descriptions, external links
  - Changed appName token from 'Radarr' to 'Aletheia' in translations

### Planned
- Book management system with hierarchical structure (Authors, Series, Books)
- Audiobook management and integration
- Unified search and collection features across media types
- Enhanced metadata handling for multi-media formats

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
  - Rationale: Fork establishes distinct identity while retaining proven Radarr architecture as foundation for multi-media manager
  - Alternative: Maintained Radarr branding (rejected - clarity and distinctness required)
  - Gotcha: Docker images and configuration references still contain "radarr" in internal paths; changes are UX-facing only to maintain upstream compatibility

### Notes
- Movie functionality preserved from Radarr v5.x
- Book and audiobook support planned for future phases
- Hierarchical monitoring system (Author → Series → Item) is foundational design goal
- Radarr codebase remains the authoritative upstream reference for inherited functionality
