# Architectural Decisions

Log of key architectural decisions for the Aletheia project.

---

## ADR-001: Multi-Media Type Indexers

**Date:** December 2024
**Status:** Implemented

### Context

Aletheia aims to unify media management across movies, books, and audiobooks. Indexers need to support multiple media types rather than being movie-specific.

### Decision

Added `SupportedMediaTypes` property to indexer interface and definitions:
- `IIndexer.SupportedMediaTypes` - array of supported `MediaType` enum values
- Database migration 243 adds column to IndexerDefinitions table
- Default indexers support `Movie` type, new indexers can declare multiple types

### Consequences

- Enables MyAnonamouse integration (books + audiobooks)
- Existing movie-only indexers work unchanged
- Future indexers can declare any combination of media types
- UI can filter indexers by media type

---

## ADR-002: Hierarchical Monitoring

**Date:** December 2024
**Status:** Planned

### Context

Books and audiobooks have hierarchical relationships (Author → Series → Work) that movies lack. Monitoring at series level should cascade to individual items.

### Decision

Design monitoring as hierarchical from the start:
- Author-level monitoring applies to all works
- Series-level monitoring overrides author defaults
- Item-level monitoring provides granular control
- Cascade logic handles inheritance

### Consequences

- More complex monitoring state management
- Better user experience for series-based content
- Aligns with how users think about book collections

---

## ADR-003: Narrator-Aware Audiobooks

**Date:** December 2024
**Status:** Planned

### Context

Audiobook quality depends heavily on narrator. Same book can have multiple editions with different narrators, and users often prefer specific narrators.

### Decision

First-class narrator support:
- Narrator field on edition model (from Akouarr port)
- Narrator preferences in quality profiles
- Narrator-based duplicate detection
- Search filters by narrator

### Consequences

- Competitive differentiator from generic audiobook managers
- Additional metadata complexity
- Requires narrator-aware metadata providers

---

## ADR-004: Notification Provider Consolidation

**Date:** December 2024
**Status:** Implemented

### Context

Multiple notification providers (Apprise, Pushcut) had duplicate helper methods like `GetPosterUrl`, contributing to code duplication flagged by SonarCloud.

### Decision

Moved common utility methods to `NotificationBase<T>` base class:
- `GetPosterUrl(Movie)` - null-safe poster URL extraction
- Protected static method accessible to all notification providers

### Consequences

- Reduced code duplication (~8 lines per provider)
- Single point of maintenance for poster URL logic
- Consistent null-safety across providers

---

## ADR-005: GitHub Actions for CI (Not Azure Pipelines)

**Date:** December 2024
**Status:** Under Evaluation

### Context

Upstream Radarr uses Azure Pipelines for multi-platform builds. Aletheia initially adopted GitHub Actions for simplicity.

### Decision

Pending evaluation (Issue #80):
- Currently using GitHub Actions for basic CI
- Multi-platform builds may require Azure Pipelines or QEMU/Docker
- Need to assess build requirements for Windows, macOS, Linux ARM

### Consequences

- Simpler initial setup with GitHub Actions
- May need pipeline additions for full platform coverage
- Should match upstream patterns where practical
