# Technical Debt Tracking

Comprehensive tracking of all identified issues in the Aletheia codebase.
Last updated: 2025-12-18

---

## Critical Bugs (Immediate Attention)

### Bug-001: Null Dereference in TorrentRssParser.cs
- **Location**: `TorrentRssParser.cs:170-171, 139-142, 182-186`
- **Issue**: `.SingleOrDefault()` returns null, then `.Value` accessed without null check
- **Impact**: NullReferenceException in production
- **Fix**: Add null checks before accessing `.Value`

### Bug-002: Unsafe LINQ First() in DownloadStationTaskProxyV2.cs
- **Location**: `DownloadStationTaskProxyV2.cs:112`
- **Issue**: `.First()` on filtered collection that may be empty
- **Impact**: InvalidOperationException
- **Fix**: Use `.FirstOrDefault()` with null check

### Bug-003: Blocking Async Pattern (GetAwaiter().GetResult())
- **Location**: `HttpClient.cs:128, 316, 327, 341, 352, 363, 377`
- **Issue**: Defeats async purpose, risks deadlocks
- **Impact**: Thread starvation under load
- **Fix**: Propagate async/await through call chain

### Bug-004: Deadlock Risk in ReleasePushController.cs
- **Location**: `ReleasePushController.cs:74`
- **Issue**: `GetAwaiter().GetResult()` inside a lock
- **Impact**: Extreme deadlock potential
- **Fix**: Use async lock (SemaphoreSlim) or restructure

### Bug-005: Blocking Task.Result in Proxies
- **Locations**:
  - `HadoukenProxy.cs:107`
  - `DelugeProxy.cs:246, 257`
  - `FreeboxDownloadProxy.cs:106, 113`
  - `NzbgetProxy.cs:261`
- **Issue**: Direct `.Result` access causes thread blocking
- **Fix**: Convert to async/await

---

## High Severity Bugs

### Bug-006: Generic Exception in OsPath/IMDbList
- **Locations**: `OsPath.cs:424`, `IMDbListRequestGenerator.cs:17`
- **Issue**: Throwing generic `Exception` instead of domain-specific types
- **Fix**: Create and use specific exception types

### Bug-007: Inverted Exception Logic in TorrentClientBase.cs
- **Location**: `TorrentClientBase.cs:80-88`
- **Issue**: Exception only re-thrown when magnet URL exists - silent failures otherwise
- **Fix**: Review and correct exception handling logic

### Bug-008: Stale State Closure in MovieSearchInput.tsx
- **Location**: `MovieSearchInput.tsx:197`
- **Issue**: `requestLoading` captured in stale closure - race condition bug
- **Fix**: Use ref or useCallback with proper dependencies

### Bug-009: parseInt Without Validation in getSelectedIds.ts
- **Location**: `getSelectedIds.ts:9`
- **Issue**: Can return NaN for invalid input, corrupting selection state
- **Fix**: Add validation or use Number() with fallback

---

## Medium Severity Bugs

### Bug-010: Regex DoS Vulnerability in RegexReplace.cs
- **Location**: `RegexReplace.cs:13, 19`
- **Issue**: No timeout on regex - vulnerable to ReDoS attacks
- **Fix**: Add `RegexOptions.Compiled` and timeout

### Bug-011: Missing Null Checks in ConfigFileProvider.cs
- **Locations**: `ConfigFileProvider.cs:329, 337, 355, 367, 411`
- **Issue**: `.Single()` on XML queries crashes when schema unexpected
- **Fix**: Use `.SingleOrDefault()` with null handling

### Bug-012: Empty Touch Handler in MovieDetails.tsx
- **Location**: `MovieDetails.tsx:420-424`
- **Issue**: `handleTouchMove` does nothing - likely incomplete implementation
- **Fix**: Implement or remove

### Bug-013: Unsafe Path Check in InstallUpdateService.cs
- **Location**: `InstallUpdateService.cs:102`
- **Issue**: `.EndsWith("_output")` doesn't account for path separators
- **Fix**: Use `Path.GetFileName()` or proper path comparison

### Bug-014: Inconsistent HTTP Status in MovieController.cs
- **Location**: `MovieController.cs:276`
- **Issue**: PUT returns 202 Accepted instead of OK for sync update
- **Fix**: Return appropriate status code

### Bug-015: Blocking Semaphore in MediaCoverService.cs
- **Location**: `MediaCoverService.cs:187`
- **Issue**: `_semaphore.Wait()` blocks thread
- **Fix**: Use `WaitAsync()`

---

## Low Severity Bugs

### Bug-016: Typo in Log Message
- **Location**: `InstallUpdateService.cs:184`
- **Issue**: Double closing bracket `]]` in format string
- **Fix**: Remove duplicate bracket

### Bug-017: Silent Error Swallowing in MovieTagInput.tsx
- **Location**: `MovieTagInput.tsx:26-28`
- **Issue**: Empty catch block hides exceptions
- **Fix**: Log error or handle appropriately

### Bug-018: Console Statements in Production (12 instances)
- **Locations**:
  - `ClipboardButton.tsx:50` (console.error)
  - `CollectionFooter.tsx:160` (console.warn)
  - `ManageImportListsEditModalContent.tsx:128`
  - `ManageIndexersEditModalContent.tsx:116`
  - `EditMoviesModalContent.tsx:123`
  - 7 more similar...
- **Fix**: Replace with proper error handling/logging

### Bug-019: Commented-Out Regex in Parser.cs
- **Location**: `Parser.cs:46-48`
- **Issue**: TODO indicates parser incompleteness
- **Fix**: Implement or document limitation

---

## Performance Issues

### Perf-001: O(n*m) Contains Pattern (10+ locations)
- **Example**: `MoviesSearchService.cs:65-66`
  ```csharp
  var queue = _queueService.GetQueue().Where(...).Select(q => q.Movie.Id);
  var missing = movies.Where(e => !queue.Contains(e.Id)).ToList();
  ```
- **Fix**: Convert to `HashSet<int>` for O(1) lookups
- **Other locations**:
  - `ReleaseSearchService.cs:88`
  - `FileNameBuilder.cs:451, 456`
  - `NewznabCategoryFieldOptionsConverter.cs:40`
  - `PendingReleaseService.cs:170`

### Perf-002: Regex Instantiation in Methods (66 instances)
- **Example**: `SkyHookProxy.cs:409, 417`
  ```csharp
  var match = new Regex(@"\bimdb\.com/title/(tt\d{7,})\b", ...).Match(title);
  ```
- **Fix**: Move to `private static readonly Regex` with `RegexOptions.Compiled`

### Perf-003: Uncached Computed Properties
- **Location**: `SearchCriteriaBase.cs:22`
- **Issue**: `CleanSceneTitles` recalculates on every access
- **Fix**: Cache result with lazy initialization

### Perf-004: Unnecessary .ToList() Before Foreach
- **Locations**: `ConfigFileProvider.cs:416`, `Cached.cs:121`
- **Fix**: Remove `.ToList()` - foreach works on IEnumerable

### Perf-005: GroupBy().First() Instead of DistinctBy
- **Location**: `MovieStatisticsService.cs:25`
- **Fix**: Use `.DistinctBy()` for cleaner LINQ

### Perf-006: Count() Instead of Any()
- **Locations**: `Torznab.cs:128`, `QualityProfileService.cs:222`
- **Fix**: Use `.Any()` for existence check (short-circuits)

---

## Code Quality Issues

### Quality-001: God Functions (Must Refactor)

| File | Method | Lines | Issue |
|------|--------|-------|-------|
| `QualityParser.cs` | `ParseQualityName` | 197 | 5+ nesting levels |
| `Parser.cs` | `ParseMovieTitle` | 140+ | Complex regex without decomposition |
| `FileNameBuilder.cs` | Entire class | 781 | 6 dependencies, multiple responsibilities |
| `Discord.cs` | `OnGrab` | 102 | Complex field building mixed with payload |

### Quality-002: God Components (Frontend)

| File | Lines | Recommendation |
|------|-------|----------------|
| `MovieDetails.tsx` | 978 | Split into 5-6 smaller components |
| `InteractiveImportModalContent.tsx` | 866 | Extract table, selection, modals |

### Quality-003: Deep Nesting (4-5+ Levels)
- `QualityParser.cs:138-158`
- `FileNameBuilder.cs:269-282`
- `QBittorrent.cs:86-128`
- **Fix**: Use early returns to flatten

### Quality-004: Copy-Paste Code
- `ConfigService.cs:82-399` - 60+ property boilerplate (~300 LOC)
- `QBittorrent.cs:93-127` - 3 identical try-catch blocks (~40 LOC)
- `QBittorrent.cs:71-150` - AddFromMagnet/AddFromTorrent 80% duplicate (~60 LOC)

### Quality-005: Magic Numbers/Strings
- `FileNameBuilder.cs` - `title.Substring(0, 1)`, "755", date formats
- `Discord.cs:44` - Hardcoded URL
- `ConfigService.cs` - Default values inline instead of constants

---

## TypeScript/Frontend Issues

### TS-001: TypeScript `any` Usage (Should be 0)

| File | Line | Issue |
|------|------|-------|
| `SelectContext.tsx` | 29 | `children: any` |
| `selectSettings.ts` | 73 | `[id: string]: any` |
| `index.ts` | 23 | `...parameters: any[]` |
| `OverlayScroller.tsx` | 102 | `(props: any)` |
| `Tooltip.tsx` | 100 | `(props: any)` |
| `AutoSuggestInput.tsx` | 94 | `(data: any)` |
| `EnhancedSelectInput.tsx` | 193 | `(data: any)` |

### TS-002: Index as Key in Lists (25+ instances - CRITICAL)
- Causes state corruption when list changes
- **Locations**:
  - `ExtraFileDetailsPopover.tsx:41`
  - `Form.tsx:26`
  - `KeyValueListInput.tsx:87`
  - `InteractiveImportRow.tsx:380`
  - `InteractiveSearchRow.tsx:306, 323`
  - `FormInputGroup.tsx:272, 290, 299, 310, 319`
  - `UpdateChanges.tsx:33`

### TS-003: Inline Objects in JSX (40+ instances)
- Creates new object every render, breaks memoization
- **Examples**:
  - `MovieDetails.tsx:660` - `style={{ width: marqueeWidth }}`
  - `AutoSuggestInput.tsx:143-150` - `modifiers={{ ... }}`
- **Fix**: Use `useMemo` for object props

### TS-004: Type Assertions Overuse (40+ `as` casts)
- Indicates weak type inference
- **Examples**:
  - `InteractiveImportModalContent.tsx:381-382` - `id as number`
  - `MovieCollectionLabel.tsx:21` - `{} as MovieCollection`
  - `SelectQualityModalContent.tsx:38, 110` - `as Quality`

### TS-005: Non-null Assertions (9+ `!` operators)
- Can cause runtime errors if assumption wrong
- **Examples**:
  - `DeleteMovieModalContent.tsx:38` - `useMovie(movieId)!`
  - `OrganizePreviewModalContent.tsx:61` - `useMovie(movieId)!`
  - `createProviderSettingsSelector.ts` - `section.items.find(...)!`

---

## Comment & Documentation Issues

### Doc-001: Stale/Dangerous Comments
- `RTorrent.cs:229` - "XXX: This function's correctness has not been considered" (actually fine)
- `MatchesFolderSpecification.cs:31-37` - TODO with commented-out code, returns Accept blindly

### Doc-002: TODO/FIXME Statistics
- 55+ files with TODO comments (no owner, timeline, or priority)
- 5+ FIXME comments (untracked technical debt)
- 10+ blocks of commented-out code

### Doc-003: Undocumented Complex Regex (20+ patterns)
- `QualityParser.cs:17-77` - No explanation of capture groups
- Many more throughout codebase

### Doc-004: Redundant XML Documentation (100+ lines)
- `LimitedConcurrencyLevelTaskScheduler.cs` - 24 lines restating variable names
- `RomanNumeral.cs` - 34+ lines copying .NET Framework doc style
- `OAuthTools.cs` - 107 lines with redundant XML doc on every method

---

## Formatting & Syntax Issues

### Format-001: Block Body vs Expression Body (77 instances)
```csharp
// Current (77 places)
public int Age { get { return DateTime.UtcNow.Subtract(PublishDate).Days; } }

// Preferred
public int Age => DateTime.UtcNow.Subtract(PublishDate).Days;
```
- Files: `ReleaseInfo.cs:47-72`, `WebParameterCollection.cs`, `ConfigService.cs`

### Format-002: String Concatenation vs Interpolation (20+ instances)
```csharp
// Current
stringBuilder.AppendLine("Guid: " + Guid ?? "Empty");

// Preferred
stringBuilder.AppendLine($"Guid: {Guid ?? "Empty"}");
```
- Files: `ReleaseInfo.cs:85-95`, `TorrentInfo.cs`, `OAuthTools.cs`

### Format-003: Line Length Violations (30+ files exceed 120 chars)
- `HtmlMapperBase.cs:14` - 237 characters
- Many regex patterns exceed limit

### Format-004: Multiple Classes Per File (442 files - 17.8%)
- Often intentional for related small classes
- Example: `ManualImportResource.cs` - 4 classes in one file

---

## Modern Syntax Opportunities (Low Priority)

### Modern-001: Target-typed `new()` (1,946 instances)
```csharp
// Current
var list = new List<string>();

// Modern
List<string> list = new();
```
- Very low priority, pure style

### Modern-002: Null-coalescing Operators (372 instances)
```csharp
// Current
if (value != null) return value; else return default;

// Modern
return value ?? default;
```

---

## Security Issues (From CodeQL)

### Sec-001: Log Forging (59 instances)
- User-controlled data logged without sanitization
- **Locations**: Throughout logging middleware, controllers, services
- **Risk**: Attackers can inject fake log entries
- **Fix**: Sanitize user input before logging or use structured logging

---

## Won't Fix (With Justification)

### WontFix-001: S1481 - Remove Unused Variables
- **Reason**: StyleCop SA1312 conflicts with C# discard pattern `var _ = x`

### WontFix-002: S7763 - Re-export Style in icons.ts
- **Reason**: Current pattern groups imports by icon library (solid/regular/brands) for readability

### WontFix-003: S1192 - Extract Duplicate Strings in CustomScript.cs
- **Reason**: Environment variable names are API contracts - clarity over constants

### WontFix-004: ModalContent Factory Pattern
- **Reason**: Only 53 components (not 113), each has domain-specific logic

### WontFix-005: React.memo on All 469 Components
- **Reason**: Uses react-window which handles memoization internally

---

## Completed Fixes (PR #31)

- [x] S2325: Make methods static (~263 issues)
- [x] S3260: Seal private classes (~63 issues)
- [x] S6610: Use char overloads for StartsWith/EndsWith (~32 issues)
- [x] S7773: Use Number.parseInt/parseFloat/isNaN (~93 issues)
- [x] S1125: Remove redundant boolean literals (~14 issues)
- [x] S6759: Mark React props as Readonly (~268 issues)
- [x] Cognitive complexity reductions (3 critical)
- [x] Domain-specific exceptions (5 new classes)
