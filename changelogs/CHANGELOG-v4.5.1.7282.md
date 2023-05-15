# New Beta Release

Radarr v4.5.1.7282 has been released on `develop`

- **Users who do not wish to be on the alpha `nightly` testing branch should take advantage of this parity and switch to `develop`**

A reminder about the `develop` and `nightly` branches

- **develop - Current Develop/Beta - (Beta): This is the testing edge. Released after tested in nightly to ensure no immediate issues. New features and bug fixes released here first after nightly. It can be considered semi-stable, but is still beta. This version will receive updates either weekly or biweekly depending on development.**
- **nightly - Current Nightly/Unstable - (Alpha/Unstable) : This is the bleeding edge. It is released as soon as code is committed and passes all automated tests. This build may have not been used by us or other users yet. There is no guarantee that it will even run in some cases. This branch is only recommended for advanced users. Issues and self investigation are expected in this branch. Use this branch only if you know what you are doing and are willing to get your hands dirty to recover a failed update. This version is updated immediately.**

# Announcements

- Radarr Plex Watchlist Improvements
- Parsing Improvements

# Additional Commentary



# Releases

## Native

- [GitHub Releases](https://github.com/Radarr/Radarr/releases)

- [Wiki Installation Instructions](https://wiki.servarr.com/radarr/installation)

## Docker

- [hotio/Radarr:testing](https://hotio.dev/containers/radarr)

- [lscr.io/linuxserver/Radarr:develop](https://docs.linuxserver.io/images/docker-radarr)

## NAS Packages

- Synology - Please ask the SynoCommunity to update the base package; however, you can update in-app normally

- QNAP - Please ask the QNAP to update the base package; however, you should be able to update in-app normally

------------

# Release Notes

## v4.5.1.7282 (changes since v4.5.0.7106)

 - Add back min availability to bulk movie edit

 - Clean up variable name case

 - Fix Radarr import syncing not matching any root folders.

 - Fix MovieFileLanguageConnector to use MovieLanguage

 - Update UI dependencies

 - Add `inset` to stylelintrc

 - Remove unused babel plugins and fix build with profiling

 - Update all relevant dev tool deps

 - Delete various old config files

 - Use `await using` in LocalizationService

 - Fixed: Provider health checks persist after add until next scheduled check

 - Translated using Weblate (Portuguese (Brazil)) [skip ci]

 - New: Log additional information when processing completed torrents from rTorrent

 - Fix function name and use out var for try get in DownloadClientProvider

 - Add Pull Request Labeler

 - API key improvements

 - Translated using Weblate (Chinese (Simplified) (zh_CN)) [skip ci]

 - Automated API Docs update

 - New: Notifications when Manual Interaction is required for importing

 - New: On Health Restored notification

 - Why rename many files when few file do trick

 - GracePeriod not Graceperiod

 - Sort translations alphabetically

 - Move vscode settings to the frontend folder

 - Fixed IsValidPath usages

 - New: Improve path validation when handling paths from different OSes

 - Log invalid config file exceptions

 - Add VSCode extension recommendations

 - Fixed: Ensure indexer errors are handled before processing response

 - Align environment variable setting in ProcessProvider with upstream

 - New: Only add version header for API requests

 - Fixed: RootFolderPath not set for Movies from API

 - Fixed: Index UI crash for movies without files

 - New: Add token authentication for ntfy.sh notifications

 - Fixed: Matching of custom formats during movie file import

 - Revert argument exception swallowing for Plex library update

 - New: Improved Plex library updating

 - New: Add release info to webhook/custom script import events

 - New: Don't import movies that don't match grab history

 - Use string interpolation for Newznab request generation

 - Virtualize movie select for manual import with react-window

 - Convert Manual Import to Typescript

 - New: Log content for invalid torrent files

 - Translated using Weblate (Portuguese (Brazil)) [skip ci]

 - Add `tmdbid` to capabilities check in Newznab/Torznab

 - Remove requirement for imdbtitle and imdbyear in Newznab and Torznab

 - Remove duplicate check in RemotePathMappingCheck

 - Fixed: Movie Status in Table View

 - Automated API Docs update

 - New: Add result to commands to report commands that did not complete successfully

 - Translated using Weblate (French) [skip ci]

 - add trace log checkbox to bug report [common]

 - Migrate to FluentValidation 9

 - Fix downloading releases without an indexer

 - Build download requests from indexer implementation

 - bump `lock threads` github action to latest [skip ci]

 - Fixed some aria violations

 - Fixed: Search Button Display on Movie Index

 - Fixed: Unable to search individual movies from Movie Index

 - Fixed: Upgrades blocked: UpgradeSpecification error

 - Fixed: Cannot Toggle Show Search on Movie Index

 - New: Filter Sonarr synchronization based on Root Folders

 - New: Add Original Language as Filter Option in Discover View

 - New: Handle multi title release names split by slash

 - Translated using Weblate (Chinese (Simplified) (zh_CN)) [skip ci]

 - Fixed: Don't import Custom Format downgrades

 - Fixed: Enable parsing of repacks with revision

 - Fixed: Don't clean Kodi library if video is playing and Always Update is disabled

 - Revert "Build download requests from indexer implementation"

 - Fixed: Movie count incorrect in Movie Editor

 - Fixed: Missing Translates

 - Simplify DatabaseType logic

 - Fixed: (Database) Improve Version detection

 - Fixed: Importing from Manual Import ignoring Analyze video files

 - Extract useSelectState from SelectContext

 - Avoid queue failures due to unknown release language processing

 - Fix default value variable name for ImportListExclusion

 - New: Closing Move Movie modal without selecting will cancel save

 - Use augmented languages for queue items

 - New: Use languages from Torznab/Newznab attributes if given

 - New: Use TmdbId from parsing for mapping

 - Cleanup ParsingService

 - Fixed: Pushed releases should be stored as pushed release

 - New: Don't block imports when release was matched by ID if they were grabbed via interactive search

 - Fixed: Queue not showing items with conflicting titles

 - New: Include Movie Match Type in grab event details

 - Fixed: Automatic import of releases when file is not matched to movie

 - Fixed: Don't automatically import if release title doesn't match movie title

 - Fixed: Throw to manual import if multiple movies found during title mapping

 - Build download requests from indexer implementation

 - New: Updated button and calendar outline colors for dark theme

 - Fix loading eslintrc

 - New: Remember add import list exclusion when removing movie

 - Fixed: Movies table not resizing properly when window size changed

 - Fixed: Movie select not working correctly after stopping/starting or changing sort order

 - Improved UI error messages (stack trace and version)

 - New: Increase clickable area of movie select in poster/overview

 - Remove unused ReactDOM import

 - Fixed: File browser

 - Remove movie editor code

 - New: Mass Editor is now part of movie list

 - Added movie index selection

 - Fixed: Restoring scroll position when going back/forward to series list

 - Refactor Movie index to use react-window

 - Add CSS Typings

 - Add Prettier to format TypeScript files

 - Add typescript

 - New: Parsing of more German WEBDL releases

 - Fixed: Parse 720p Remux as 720p BluRay

 - QualityParser - Simplify new expression (IDE0090)

 - Misc HealthCheck Cleanup and Sonarr Alignment

 - Bump Swashbuckle to 6.5.0

 - Fixed: Ensure first history item when marked as failed is the selected item

 - Fixed: Edit Quality Profile not opening

 - Refactor LanguageParser.ParseLanguageTags() to return List<> instead of IEnumerable. Clean up calls to ParseLanguageTags().

 - Include extra tags from existing subtitles when renaming.

 - Translated using Weblate (French) [skip ci]

 - Use BuildInfo.AppName for RARBG appId instead of hardcoded value

 - New: Updated Rarbg request limits

 - New: Report health error if Recycling Bin folder is not writable

 - Update core-js and use defaults for browserlist

 - Update webpack and webpack-cli

 - Use minified jquery

 - Remove unused gulpFile

 - Fix typo in calendarBackgroundColor CSS variable

 - Fix QualityParser Tests

 - Fixed: Parse DVD with 576p Resolution as DVD

 - Auto-reply for Log Label [common]

 - Bump version to 4.5.1

 - Other bug fixes and improvements, see GitHub history
