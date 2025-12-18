# Changelog

All notable changes to Logarr are documented in this file.

## [Unreleased]

### Planned
- Book management system with hierarchical structure (Authors, Series, Books)
- Audiobook management and integration
- Unified search and collection features across media types
- Enhanced metadata handling for multi-media formats

## [0.1.0] - 2024-12-17 - Initial Fork

### Added
- Fork of Radarr v5.x as foundation for unified media manager
- Logarr branding throughout application
  - BuildInfo.cs AppName property set to "Logarr"
  - UI localization strings (en.json) updated with Logarr branding
  - Docker labels and metadata identify as "Logarr"
- GitHub Actions CI/CD workflow for continuous integration
- Docker configuration with multi-architecture support (amd64, arm64)
- Project documentation structure and contribution guidelines

### Changed
- **Application Identity** - Radarr → Logarr
  - Before: Application branded as "Radarr" throughout codebase
  - After: Application branded as "Logarr" (λόγος - word, reason, unity)
  - Rationale: Fork establishes distinct identity while retaining proven Radarr architecture as foundation for multi-media manager
  - Alternative: Maintained Radarr branding (rejected - clarity and distinctness required)
  - Gotcha: Docker images and configuration references still contain "radarr" in internal paths; changes are UX-facing only to maintain upstream compatibility

### Notes
- Movie functionality preserved from Radarr v5.x
- Book and audiobook support planned for future phases
- Hierarchical monitoring system (Author → Series → Item) is foundational design goal
- Radarr codebase remains the authoritative upstream reference for inherited functionality
