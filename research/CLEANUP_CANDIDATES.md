# Cleanup Candidates

Comprehensive analysis of files and patterns that should be addressed in future maintenance.

## Summary

| Category | Count | Action |
|----------|-------|--------|
| Obsolete CI config | 1 | Delete |
| Tracked IDE files | 10 | `git rm --cached` |
| Empty localization files | 6 | Delete or populate |
| Outdated documentation | 6 | Update for Logarr branding |
| Unused methods | 3 | Remove |
| Commented code blocks | 5+ | Remove or uncomment |
| Commented test methods | 5+ | Remove or re-enable |
| Redundant migrations | 5-10 | Consider squashing |
| Duplicate CSS files | 30+ | Consolidate into shared modules |
| Redundant polyfills | 1 | Remove manual polyfills |

**Total cleanup candidates: ~70+ files/issues**

---

## Files to Delete

### Obsolete CI/CD Configuration
| File | Reason |
|------|--------|
| `/azure-pipelines.yml` | GitHub Actions (`.github/workflows/build.yml`) has replaced Azure Pipelines |

### IDE Configuration Files (Tracked but Should Be Ignored)
These 10 files are in `.gitignore` but were committed and are now cached in git:

| File | Reason |
|------|--------|
| `.vscode/extensions.json` | IDE-specific |
| `.vscode/launch.json` | IDE-specific |
| `.vscode/tasks.json` | IDE-specific |
| `frontend/.vscode/extensions.json` | IDE-specific |
| `frontend/.vscode/settings.json` | IDE-specific |
| `src/.idea/.idea.NzbDrone/.idea/.name` | JetBrains IDE config |
| `src/.idea/.idea.NzbDrone/.idea/encodings.xml` | JetBrains IDE config |
| `src/.idea/.idea.NzbDrone/.idea/indexLayout.xml` | JetBrains IDE config |
| `src/.idea/.idea.NzbDrone/.idea/misc.xml` | JetBrains IDE config |
| `src/.idea/.idea.NzbDrone/.idea/vcs.xml` | JetBrains IDE config |

**Fix:** `git rm --cached` for all 10 files

### Empty Localization Placeholder Files
These contain only `{}` and serve no purpose:

| File | Language |
|------|----------|
| `src/NzbDrone.Core/Localization/Core/bs.json` | Bosnian |
| `src/NzbDrone.Core/Localization/Core/ta.json` | Tamil |
| `src/NzbDrone.Core/Localization/Core/et.json` | Estonian |
| `src/NzbDrone.Core/Localization/Core/es_MX.json` | Spanish (Mexico) |
| `src/NzbDrone.Core/Localization/Core/lt.json` | Lithuanian |
| `src/NzbDrone.Core/Localization/Core/sr.json` | Serbian |

---

## Documentation Requiring Updates

| File | Issue |
|------|-------|
| `README.md` | Still says "Radarr" - needs Logarr branding |
| `CONTRIBUTING.md` | Outdated .NET version info, Radarr references |
| `CODE_OF_CONDUCT.md` | Uses `development@radarr.video` email |
| `CLA.md` | Generic template, not customized for Logarr |
| `.github/PULL_REQUEST_TEMPLATE.md` | References `Radarr.Console` and Radarr paths |
| `.editorconfig` (line 281) | References non-existent `.travis.yml` |

---

## Dead Code to Remove

### Unused Methods
| File | Line | Method |
|------|------|--------|
| `src/NzbDrone.Core/Movies/AlternativeTitles/AlternativeTitleService.cs` | 58-61 | `RemoveTitle()` - never called |
| `src/NzbDrone.Core/Movies/Translations/MovieTranslationService.cs` | 39-42 | `RemoveTitle()` - never called |
| `src/NzbDrone.Core/Movies/Credits/CreditService.cs` | 58-61 | `RemoveTitle()` - misnamed, never called |

### Large Commented-Out Code Blocks
| File | Lines | Description |
|------|-------|-------------|
| `src/NzbDrone.Core/Housekeeping/Housekeepers/FixWronglyMatchedMovieFiles.cs` | 16-22 | Commented SQL code |
| `src/NzbDrone.Core/Housekeeping/Housekeepers/UpdateCleanTitleForMovies.cs` | 16-22 | Commented cleanup loop |
| `src/NzbDrone.Core/Download/Clients/Deluge/DelugeTorrent.cs` | 18-23 | Commented properties |
| `src/NzbDrone.Core/Messaging/Events/EventAggregator.cs` | 56-71 | Commented debug code |
| `src/NzbDrone.Core/Parser/Parser.cs` | 47-48 | Commented regex (marked slow) |

### TODO Comments Suggesting Removal
| File | Line | Note |
|------|------|------|
| `src/NzbDrone.Core/Download/Clients/Sabnzbd/Sabnzbd.cs` | 559 | "Remove checks once SABnzbd < 4.3 support removed" |
| `src/Radarr.Api.V3/MediaCovers/MediaCoverController.cs` | 36 | "Code can be removed once everyone had update" |

---

## Test File Cleanup

### Commented-Out Test Methods
| File | Lines | Description |
|------|-------|-------------|
| `src/NzbDrone.Core.Test/MovieStatsTests/MovieStatisticsFixture.cs` | 91-111 | Entire test method commented |
| `src/NzbDrone.Core.Test/DecisionEngineTests/HistorySpecificationFixture.cs` | 106-157 | 5+ test methods commented, references obsolete `HistoryEventType` |
| `src/NzbDrone.Core.Test/OrganizerTests/FileNameBuilderTests/FileNameBuilderFixture.cs` | 349-358 | Commented test with TODO |
| `src/NzbDrone.Core.Test/ParserTests/ParserFixture.cs` | 14-26 | "Fucked-up hall of shame" commented block |
| `src/NzbDrone.Core.Test/MediaFiles/MovieImport/Specifications/MatchesFolderSpecificationFixture.cs` | 26-65 | Disabled tests with "TODO: Decide whether to reimplement" |

### Missing Test Coverage
| File | Line | Issue |
|------|------|-------|
| `src/NzbDrone.Core.Test/MediaFiles/MovieImport/ImportDecisionMakerFixture.cs` | 23 | TODO: Add tests for augmenter helpers |

---

## Redundant Database Migrations

### Duplicate/Redundant Migrations
| Migration | Issue |
|-----------|-------|
| 115 & 116 | Nearly identical SortTitle updates |
| 130 | Redundantly removes indexers already removed in 127 & 128 |
| 190 | Updates AwesomeHD URLs right before 191 removes the indexer entirely |

### Add-Then-Remove Patterns (Could Squash)
| Added | Removed | Feature |
|-------|---------|---------|
| 135 | 202 | HasPreDBEntry column |
| 136 | 167 | PathState column |

### Consolidation Candidates
| Migrations | Description |
|------------|-------------|
| 111, 112, 113, 114 | 4 individual indexer removals (14 lines each) - could be one migration |
| 127, 128 | Individual Wombles/Kickass removals |

---

## Duplicate CSS Files (30+ files)

### Exact Duplicates (Should Consolidate)
| Pattern | Copies | Files |
|---------|--------|-------|
| `TagsModalContent.css` | 4 | ImportLists, Indexers, DownloadClients, Movie/Index |
| `ManageXyzModalContent.css` | 4 | CustomFormats, ImportLists, Indexers, DownloadClients |
| `EditSpecificationModalContent.css` | 4 | AutoTagging, CustomFormats, Indexers, DelayProfiles |
| `ManageXyzEditModalContent.css` | 5 | CustomFormats, ImportLists, Indexers, DownloadClients, Movies |
| `NoXyz.css` | 3 | NoMovie, NoMovieCollections, NoDiscoverMovie |
| Rating CSS files | 4 | ImdbRating, TraktRating, TmdbRating, RottenTomatoRating |

### Additional Identical Pairs
- `Parse.css` ↔ `ParseModalContent.css`
- `FavoriteFolderRow.css` ↔ `RecentFolderRow.css`
- `CollectionFooter.css` ↔ `DiscoverMovieFooter.css`
- `VirtualTableHeaderCell.css` ↔ `TableHeaderCell.css`
- `EditImportListExclusionModalContent.css` ↔ `EditRemotePathMappingModalContent.css`

---

## Redundant Polyfills

| File | Issue |
|------|-------|
| `frontend/src/polyfills.js` | Contains manual polyfills for `String.prototype.startsWith/endsWith/contains` but project includes `core-js@3.42.0` which handles these |

---

## Future Hardening Phase

A comprehensive security and code quality hardening phase should address:

1. **Input validation** - Review all user input paths for injection vulnerabilities
2. **Path traversal** - Audit file system operations (identified in SECURITY_AUDIT.md)
3. **Certificate handling** - Default to secure, document self-signed cert setup
4. **Async patterns** - Convert blocking calls to proper async/await
5. **Dependency updates** - Address deprecated packages (moment.js, jQuery, etc.)
6. **Code cleanup** - Remove dead code, consolidate duplicates, fix TODOs
7. **Test coverage** - Re-enable or remove commented tests, add missing coverage

See `research/SECURITY_AUDIT.md` for security-specific findings.
