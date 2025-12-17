# Logarr Development

All-in-one media manager (movies, books, audiobooks). Forked from Radarr.
Greek: λόγος (logos) = "word, reason"

<critical_rules>

CRITICAL: Zero AI indicators in code, commits, docs pushed to GitHub
CRITICAL: Keep Radarr movie functionality working at all times
CRITICAL: Hierarchical monitoring is foundational - design it in, don't bolt on
REQUIRED: Follow global CLAUDE.md coding standards
REQUIRED: Use sub-agents for exploration and research

</critical_rules>

## Project Info

| Item | Value |
|------|-------|
| Fork | github.com/Cody-k/logarr |
| Upstream | github.com/Radarr/Radarr |
| Stack | C# .NET 6.0, React, TypeScript |
| Goal | Unified media manager for movies, books, audiobooks |

## Quick Commands

```bash
# Build
./build.sh --backend --frontend

# Run locally
dotnet run --project src/Radarr

# Frontend dev
cd frontend && yarn install && yarn start

# Docker
docker build -t logarr .
```

## Architecture Goals

### Phase 1: Foundation
- Generalize database schema (Movies → MediaItems)
- Add MediaType discriminator
- Implement hierarchical monitoring (Author → Series → Item)

### Phase 2: Multi-Media
- Add book qualities (EPUB, MOBI, PDF)
- Add audiobook qualities (M4B, MP3)
- Port metadata providers from Akouarr/Bookshelf

### Phase 3: UI
- Unified dashboard with type filters
- Type-specific detail views
- Shared components with media badges

## Key Files

```
src/
├── NzbDrone.Common/EnvironmentInfo/BuildInfo.cs  # App name
├── NzbDrone.Core/
│   ├── Movies/                                    # → MediaItems
│   ├── Qualities/                                 # Quality definitions
│   └── Localization/Core/en.json                  # UI strings
├── Radarr.Api.V3/                                 # REST API
└── frontend/src/                                  # React UI
```

## Migration Notes

### From Akouarr
Features to port:
- Narrator field on editions
- Duration tracking
- Audiobook qualities (MP3, M4B, FLAC)
- Batch query patterns
- Analytics dashboard
- MyAnonaMouse indexer

### From Radarr
Keep intact:
- Movie tracking and monitoring
- TMDB metadata integration
- Quality profile system
- Download client integration
- Indexer framework

## Development Standards

- Match existing codebase patterns
- No placeholder code
- Test changes before committing
- Keep Radarr compatibility during development
