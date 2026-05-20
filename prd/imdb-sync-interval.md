# PRD: Lower IMDb list minimum refresh interval from 12 hours to 30 minutes

## Problem Statement

As a user of this Radarr fork who relies on IMDb lists (watchlists, public lists, charts) to drive my movie library, I have to wait up to 12 hours between syncs. When I add a movie to a watched IMDb list, I expect Radarr to pick it up promptly — not half a day later. The 12-hour floor is the upstream Radarr default and it is more conservative than this fork needs.

## Solution

Lower the minimum refresh interval enforced for IMDb-backed import lists from **12 hours to 30 minutes** so that the scheduled `ImportListSyncCommand` (which already runs every 5 minutes) will refresh IMDb lists up to roughly twice per hour instead of twice per day. The change is scoped to IMDb lists only — other list types (Trakt, RSS, TMDb, Plex, etc.) keep their existing intervals.

No new settings, no UI changes, no migration. The existing UI alert that reads "List will refresh every [interval]" will automatically display the new value because it is sourced from the backend's `MinRefreshInterval` field on each list definition.

## User Stories

1. As a user with an IMDb watchlist connected to Radarr, I want new entries on my watchlist to be picked up within ~30 minutes, so that movies I just added are queued for search promptly instead of waiting half a day.
2. As a user who curates a public IMDb list of upcoming releases, I want my list-driven Radarr library to feel near-real-time, so that the moment I publish an update my home automation / downstream systems begin acting on it.
3. As a user who only uses non-IMDb lists (Trakt, Plex, TMDb), I want the behavior of those lists unchanged after this update, so that I don't see unexpected refresh rates or load against unrelated services.
4. As a user editing an IMDb import list in the UI, I want the "List will refresh every X" alert to correctly show "30 minutes" instead of "12 hours", so that the displayed expectation matches actual behavior.
5. As a user with multiple IMDb lists configured, I want each list to respect the same 30-minute minimum independently, so that one list refreshing does not block or starve another.
6. As an operator running this fork, I want IMDb sync to remain cheap and respectful — using the same IMDb endpoint, same caching, same single-fetch-per-list semantics — just more frequently, so that I do not become a bad actor against IMDb's CDN.
7. As an operator who pulls updates from upstream Radarr, I am OK with this one-line divergence creating a merge conflict the next time IMDb-specific list code changes upstream, because the conflict is trivial to resolve.
8. As a user, I do not want to configure this per-list — a hardcoded fork-level default of 30 minutes is acceptable and matches how every other list type's minimum interval is set today.

## Implementation Decisions

- **Single point of change**: `IMDbListImport` (the IMDb-specific subclass of `ImportListBase` under `RadarrList2`) overrides the abstract `MinRefreshInterval` property. The override value is changed from `TimeSpan.FromHours(12)` to `TimeSpan.FromMinutes(30)`. This is the only code edit required.
- **Scope is IMDb only**: We do not touch `RSSImport`, `TraktImportBase`, `TMDbImportBase`, `RadarrListImport`, `StevenLu2Import`, `PlexRssImport`, `CouchPotatoImport`, `RadarrImport`, `SimklImportBase`, or `PlexImport`. Each retains its current minimum.
- **No new module or abstraction**: The existing per-list-type `MinRefreshInterval` property on `ImportListBase` is already the cleanest possible shape — a single `TimeSpan` per implementation. Extracting it further (e.g., into a configuration or settings module) would be premature.
- **No DB migration**: `ImportListDefinition.MinRefreshInterval` is repopulated from the live provider via `ImportListFactory` whenever definitions are loaded, so the new value will propagate to stored definitions and to the API response without manual intervention.
- **Scheduled task untouched**: `ImportListSyncCommand` already runs every 5 minutes via `TaskManager`. Because 30 minutes is a clean multiple of 5, sync passes will trigger on the first 5-minute tick after each 30-minute window elapses. No scheduler change required.
- **Enforcement path untouched**: `FetchAndParseImportListService` already compares `lastSync + provider.MinRefreshInterval` against `UtcNow` and skips the refresh if the window hasn't elapsed. It will pick up the new 30-minute value automatically because it reads from the live provider instance, not a cached/persisted copy.
- **No new API contract**: The existing API response field `minRefreshInterval` on the import-list resource will simply return `00:30:00` instead of `12:00:00` for IMDb lists. No new fields, no breaking shape changes.
- **No frontend code change**: The "List will refresh every X" alert in the edit-list modal reads `minRefreshInterval` from the backend resource and formats it with `formatShortTimeSpan`, so the displayed text updates with no frontend work.
- **Fork-divergence stance**: We are not isolating the value into a separate constants file or marking it specially for upstream merges. The diff is one line; if upstream later changes IMDb list code in the same place, we will resolve the conflict at merge time.

## Testing Decisions

- **A good test in this repo, generally**: asserts externally observable behavior (e.g., that the service skips a list whose last sync is within the minimum window, that the API returns the right shape, that the parser handles a real response correctly) and avoids coupling to internal implementation details like specific constant values.
- **No automated test will be added for this change.** Rationale:
  - The repo does not currently have a test asserting `MinRefreshInterval` for *any* list type (IMDb, Trakt, Plex, RSS, etc.). Adding one only for IMDb would create a pattern that does not exist elsewhere.
  - The change is a single-constant edit on a `TimeSpan` literal; there is no logic to exercise.
  - Existing `FetchAndParseImportListServiceFixture` tests cover the enforcement *logic* generically — they would all still pass and continue to protect us against regressions in the *behavior* (which is the thing worth testing) rather than the value.
- **Manual verification plan**:
  1. After the change, configure or open an existing IMDb list in the Radarr UI and confirm the alert reads "List will refresh every 30 minutes".
  2. Confirm the API response for the import-list resource contains `minRefreshInterval: "00:30:00"` for an IMDb list.
  3. Observe logs from `FetchAndParseImportListService` over a ~1-hour window and confirm at least one IMDb-list refresh occurs that previously would have been skipped under the 12-hour rule, and that no errors are produced against IMDb's endpoint.

## Out of Scope

- Lowering the minimum interval for any non-IMDb list type (Trakt, RSS, TMDb, Plex, etc.).
- Making the minimum interval user-configurable per list via a UI field or settings option.
- Changing the cadence of the `ImportListSyncCommand` scheduled task itself (it stays at 5 minutes).
- Caching, rate-limiting, or backoff logic for IMDb requests. We are not adding new protection on top of IMDb's existing CDN behavior.
- Any DB schema change, migration, or one-off script.
- Frontend translation/copy changes — the alert string is dynamic and will reflect the new value automatically.
- Bringing this change upstream to the main Radarr project.

## Further Notes

- The scheduled `ImportListSyncCommand` running every 5 minutes is what bounds the *upper* responsiveness of any list, regardless of `MinRefreshInterval`. With a 30-minute minimum, IMDb lists will in practice refresh on the first 5-minute tick after the 30-minute window elapses, i.e. roughly every 30–35 minutes — which is the behavior the user expects from this change.
- If real-world testing surfaces IMDb rate-limiting or CDN pushback at this cadence, a sensible follow-up is to raise the value (e.g., to 60 minutes) rather than to introduce per-user configuration. Configurability should only be added if multiple users in this fork actually want different values.
- This change makes the fork measurably more aggressive than upstream Radarr. That is the intended deviation and is the entire point of maintaining a fork.
