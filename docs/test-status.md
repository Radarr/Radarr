# Test Status and Review

## Overview
This document summarizes the state of tests in the Aletheia codebase after reviewing for outdated comments, documentation, and test relevance to the fork.

## Test File Statistics
- **Total test files**: 34
- **Commented-out test methods**: 6 instances found

## Commented-Out Tests

### 1. HistorySpecificationFixture.cs
**Location**: `src/NzbDrone.Core.Test/DecisionEngineTests/HistorySpecificationFixture.cs`
**Lines**: 106-157 (5 test methods)
**Reason**: Tests reference obsolete `HistoryEventType` enum and multi-episode scenarios from Sonarr/TV show functionality
**Status**: Should remain commented - not applicable to movie-focused Aletheia
**Context**: These tests are for TV episode matching logic that doesn't apply to the current movie-only implementation

### 2. MatchesFolderSpecificationFixture.cs
**Location**: `src/NzbDrone.Core.Test/MediaFiles/MovieImport/Specifications/MatchesFolderSpecificationFixture.cs`
**Lines**: 28-65 (5 test methods)
**Status**: Has TODO comment "Decide whether to reimplement this!"
**Context**: Tests are for episode/season matching in folder names - not relevant for single-file movies
**Recommendation**: Can be removed or left as-is with TODO since they don't affect functionality

### 3. GetMovieFixture.cs
**Location**: `src/NzbDrone.Core.Test/ParserTests/ParsingServiceTests/GetMovieFixture.cs`
**Lines**: 35-46 (1 test method)
**Status**: Tests fallback behavior for title parsing
**Recommendation**: Could be re-enabled if the fallback logic is still used

### 4. MovieStatisticsFixture.cs
**Location**: `src/NzbDrone.Core.Test/MovieStatsTests/MovieStatisticsFixture.cs`
**Lines**: 91-111 (1 test method)
**Status**: Tests multi-movie file handling
**Recommendation**: Should be reviewed when multi-media support is implemented

### 5. JobRepositoryFixture.cs
**Location**: `src/NzbDrone.Core.Test/JobTests/JobRepositoryFixture.cs`
**Lines**: 165 (1 test method)
**Status**: Incomplete commented block

### 6. NyaaFixture.cs
**Location**: `src/NzbDrone.Core.Test/IndexerTests/NyaaTests/NyaaFixture.cs`
**Lines**: 29 (1 test method)
**Status**: Nyaa indexer test

## Build Status
**Current State**: Build fails due to external dependency issue (FFMpegCore packages from Azure DevOps feed)
**Impact**: Cannot run tests until build dependency issue is resolved
**Note**: This is a pre-existing issue not related to documentation updates

## Test Infrastructure
- **Framework**: NUnit
- **Test runner**: `test.sh` script supports Linux/Windows/Mac platforms
- **Test categories**: Unit, Integration, Automation
- **Coverage requirement**: 80% on new code (per CONTRIBUTING.md)

## Test Namespaces
All test projects still use the `Radarr.*` namespace convention:
- Radarr.Core.Test
- Radarr.Api.Test
- Radarr.Integration.Test
- Radarr.Automation.Test
- etc.

**Status**: This is intentional - the codebase retains Radarr project/namespace structure for compatibility

## Recommendations

### Short-term (completed in this PR)
- ✅ Updated code comments referencing Radarr to Aletheia where appropriate
- ✅ Documented commented-out test status
- ✅ Left historical migration comments unchanged (they document origin)
- ✅ Left `window.Radarr` global namespace unchanged (requires coordinated frontend/backend change)

### Medium-term (future work)
1. **Resolve Build Dependencies**: Fix FFMpegCore package access from Azure DevOps
2. **Re-enable Applicable Tests**: Review and re-enable tests that apply to movie functionality
3. **Remove TV-Specific Tests**: Clean up tests for TV episode/season functionality that don't apply to movies
4. **Test Coverage Audit**: Run coverage reports once build is working

### Long-term (multi-media expansion)
1. **Book/Audiobook Tests**: Add new test fixtures for book and audiobook functionality
2. **Hierarchical Monitoring Tests**: Test author → series → item monitoring when implemented
3. **Multi-Media Tests**: Re-evaluate commented tests for applicability to new media types

## Notes
- Test file references to "Sonarr" in paths/comments reflect the codebase's TV show heritage
- Most commented tests are intentionally disabled due to TV episode logic not applying to movies
- The `window.Radarr` global object is a technical namespace used throughout the application
- Wiki links to `wiki.servarr.com/radarr` are legitimate external documentation references
