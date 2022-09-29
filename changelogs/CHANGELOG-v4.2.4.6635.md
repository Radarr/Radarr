# New Stable Release

Radarr v4.2.4.6635 has been released on `master`

- **Users who do not wish to be on the alpha `nightly` or beta `develop` testing branches should take advantage of this parity and switch to `master`

A reminder about the `develop` and `nightly` branches

- **develop - Current Develop/Beta - (Beta): This is the testing edge. Released after tested in nightly to ensure no immediate issues. New features and bug fixes released here first after nightly. It can be considered semi-stable, but is still beta. This version will receive updates either weekly or biweekly depending on development.**
- **nightly - Current Nightly/Unstable - (Alpha/Unstable) : This is the bleeding edge. It is released as soon as code is committed and passes all automated tests. This build may have not been used by us or other users yet. There is no guarantee that it will even run in some cases. This branch is only recommended for advanced users. Issues and self investigation are expected in this branch. Use this branch only if you know what you are doing and are willing to get your hands dirty to recover a failed update. This version is updated immediately.**

# Announcements

- Radarr Postgres Database Support
- Radarr Plex Watchlist Support
- Radarr Collections Support
  - Existing Collection Lists have been migrated
- Some users may experience `Database Malformed` or other migration errors
  - This is caused by the database having existing corruption.
  - The solution is to follow the instructions noted on the FAQ for a malformed database. <https://wiki.servarr.com/radarr/faq#i-am-getting-an-error-database-disk-image-is-malformed>
  - Given this just occurred after an update then if the post-migrated database will not open or cannot be recovered then make a copy of the database from a recent backup and apply the database recovery process to that file then try starting Radarr with the recovered backup file.  It should then migrate without issues then.

# Additional Commentary

- Radarr Postgres Database Support in `master`
- Prowlarr Postgres Database Support in `nightly` and `develop`
- [Lidarr Postgres Database Support in development (Draft PR#2625)](https://github.com/Lidarr/Lidarr/pull/2625)
- \*Arrs Wiki Contributions welcomed and strongly encouraged, simply auth with GitHub on the wiki and update the page

# Releases

## Native

- [GitHub Releases](https://github.com/Radarr/Radarr/releases)

- [Wiki Installation Instructions](https://wiki.servarr.com/radarr/installation)

## Docker

- [hotio/Radarr:release](https://hotio.dev/containers/radarr)

- [lscr.io/linuxserver/Radarr:latest](https://docs.linuxserver.io/images/docker-radarr)

## NAS Packages

- Synology - Please ask the SynoCommunity to update the base package; however, you can update in-app normally

- QNAP - Please ask the QNAP to update the base package; however, you should be able to update in-app normally

------------

# Release Notes

## v4.2.4.6635 (changes since v4.1.0.6175)

 - Ignore SQLiteException tests on Azure

 - Correct SQLiteException Sentry filtering

 - Fix TagDetails sql for PG, add test

 - Fixed: Add YTS.AG to the exception Release Groups (#7627)

 - Fixed: Improve RarBG Error Handling

 - fix typo in MovieRepository

 - Fixed: Repack Preference Ignored

 - Fixed: Ignore Movies with null tags when pulling AllMovieTags

 - New: Torrent Seed Ratio no longer advance settings

 - Translated using Weblate (Dutch) [skip ci]

 - Remove unused package 'react-slick'

 - Fixed: Collection Carousel Improvements

 - Translated using Weblate (Portuguese (Brazil)) [skip ci]

 - Clarify Folder as Root Folder (#7598)

 - Fixed: Toolbar Button labels overlap

 - Fixed: Series list jump bar click issues

 - Fixed: Use translated title for sorttitle in Kodi nfo

 - Handle redirects for 308 redirects

 - Fixed: Improve Radarr List help text

 - Fixed: Improve Quality Profile in-use helptext

 - Bump version to 4.2.4

 - FileNameBuilderFixture tests should run on Windows

 - New: Add Latvian language

 - Fixed: Defaults for Trakt Popular List

 - Fixed: Strip additional domains out of release prefix

 - Fixed: Collections not sorting properly on Index

 - Update Bug Report Template

 - Update Bug Report Template [skip ci] [common]

 - Translated using Weblate (Portuguese (Brazil)) [skip ci]

 - Fix: Trace logging postgres cleanse for large json files.

 - Update src/NzbDrone.Core/CustomFormats/Specifications/RegexSpecificationBase.cs

 - New: (UI) Indicate Custom Formats are Case Insensitive

 - Automated API Docs update

 - New: Add application URL to host configuration settings

 - New: Setting to add Collection to NFO files

 - Really fix UI Error on Collection Filter

 - New: Preserve language tags when importing subtitle files

 - Fixed: Skip extras in 'Extras' subfolder

 - New: Import subtitles from sub folders

 - Bump version to 4.2.3

 - Translated using Weblate (German) [skip ci]

 - Automated API Docs update

 - New: (API) Get Collection by TmdbId

 - Added: Ntfy provider for notifications. (#7455)

 - Fixed: Postgres secret regex now less greedy

 - Fixed: Regex in log cleanser taking 10+ minutes on messages longer than 100k. (#7481)

 - New: Add support for Plex Edition tags in naming

 - New: Make Plex imdb tags conditional

 - Fixed: Correctly map movie by original title on import

 - Fixed: UI Error on Collection Filter

 - Fixed: Allow 0 Min on Size CustomFormat Condition

 - New: Add Slovak Language

 - Bump version to 4.2.2

 - Translated using Weblate (Spanish) [skip ci]

 - Regenerate yarn.lock

 - Bump Sentry to 3.20.1

 - Bump dotnet to 6.0.8

 - Changed: Removed Tigole from ExceptionRelease match as is checked in ExceptionReleaseExact.

 - Fixed: Tigole release group not being parsed and matched correctly, requiring manual import.

 - Fixed: Configured recycle bin is excluded from import.

 - Really fix Original Language in Language CF Specification

 - Better Sentry Filtering for AggregateException children

 - Run Postgres tests on 20.04

 - Fixed: Blank Collection on MovieDetails when no Collection for Movie

 - Remove non-functional filters for Trakt Lists

 - Fixed: Original CF shouldn't need to be named "Original"

 - Fixed NullRef in Skyhook Proxy during List Sync

 - Fixed: Remove Notifiarr Environment Option

 - Fixed: Trakt list request now uses correct rules for generating slug (#7449)

 - Fixed: Allow blank ReleaseGroup and Edition from MovieFile edit

 - Fixed: Don't process files that don't have a supported media file extension

 - Fixed: Avoid failure if list contains same movie but without tmdbid

 - Fixed: Log correct path when moving movies (#7439)

 - Fixed: Watch state not preserved on metadata rewrite (#7436)

 - Fixed: NullRefException in TorrentRssParser

 - Bump Version to 4.2.1

 - Fixed: Parse Group ZØNEHD

 - New: Parse Group HONE

 - New: (Discord) Include Custom Formats & Score On Grab

 - Translated using Weblate (Catalan) [skip ci]

 - Fixed: User Triggered Auto Searches now ignores monitored status (#7422)

 - Fixed: Postgres timezone issues (#7183)

 - Speed up and reduce meta calls for Imdb Lists when mapping

 - Fixed: ImportListMovies not saved if from a list without TMDBIds

 - Match 'HQCAM' as CAM source (#7412)

 - Fix RefreshMovieServiceFixture folder service mock

 - Fixed: Collections not deleted on Movie Delete

 - Fixed: Bulk Collection RootFolder change failure

 - New: Collection Folder, Genre, QualityProfile Filters

 - Fixed: Trim RootFolderPath on Migration

 - Avoid multiple metadata DB calls on list mapping

 - Fixed: Prevent excluded movies from being added by collections

 - Fixed: Avoid NullRef in MapMovieToTmdbMovie

 - Fixed: Notifiarr - Better HTTP Error Handling

 - Fix Nullref on Collection delete

 - New: (Notifiarr) Custom Formats in OnGrab

 - Automated API Docs update

 - New: Custom Format Spec Validation

 - Fixed: Don't fail on single failure for Discover bulk add

 - Remove general yarn restore key to avoid cross OS conflict

 - Translated using Weblate (Portuguese (Brazil)) [skip ci]

 - Fixed: Don't call for server notifications on event driven check

 - Rename MovieImportedEvent to MovieFileImportedEvent

 - Fixed: Improved parsing WebDL Releases

 - New: adding a link to tmdb in the import combobox movie search results (#7352)

 - Fixed: Housekeeper doesn't remove collections that have MovieMeta from lists

 - Fixed: Notify on Bulk Adds (Lists, Collections, Imports)

 - Updated NLog Version (#7365)

 - Translated using Weblate (Portuguese (Brazil)) [skip ci]

 - Fixed: Migration 208 fails when collection doesn't have name

 - Fixed: Don't call AddMovies if no movies to add from Collection

 - New: Default to IMDb Ratings in Kodi Metadata

 - Translated using Weblate (Slovak) [skip ci]

 - New: Separate Ratings Columns

 - Fixed: Add support for more Anime release formats

 - Translated using Weblate (Portuguese) [skip ci]

 - Automated API Docs update

 - New: Bulk Edit Collections Profile, Root, Availability

 - Automated API Docs update

 - Fixed: Collections Improvements

 - Add back Movie Credits and Alt Titles Indexes

 - Fixed: Validate if equals or child for startup folder

 - New: Notifiarr include Media Info in Download Notifications

 - New: Notifiarr moved from webhook to API

 - Translated using Weblate (German) [skip ci]

 - Use DryIoc for Automoqer, drop Unity dependency

 - Additional logging for partial Plex path scan

 - Translated using Weblate (Chinese (Simplified) (zh_CN)) [skip ci]

 - Fixed: Improved empty root folder failsafe logging (#7341)

 - Fixed: Register PostgresOptions when running in utility mode

 - Fixed: Clarified genre filtering helptext on Trakt lists

 - Fixed: Lithuanian media info parsing

 - Translated using Weblate (Portuguese (Brazil)) [skip ci]

 - Automated API Docs update

 - Fixed: MovieAdded trigger not available in UI

 - New: Movie Added Notification

 - Cleanup Collections UI Options

 - Fixed: Speed up Collections API Endpoint

 - New: Add DB Indexes for MovieMetadata

 - New: .NET 6.0.5

 - Translated using Weblate (Polish) [skip ci]

 - Fixed: Remove Collection on last Movie delete

 - Fixed: Correctly use loadash in FE Migrations

 - Fixed: Partial Revert CF Validation for more robust solution

 - Ensure .Mono and .Windows projects have all dependencies in build output

 - Fix frontend monitor migration

 - Try to fix CF null error for imported movie files

 - Tweak monitor migration to avoid overwrites of valid settings

 - Fixed: Run Frontend Migration for MonitorType

 - New: Improve validation errors for Custom Formats

 - Fixed: Don't Import Files with lower CF Score

 - Fixed: Parse UHD2BD as BluRay instead of HDTV

 - Fixed: Bluray 576p parsing

 - New: Release Group Custom Format (#7154)

 - Added term "brazilian" to Brazilian Portuguese parsing (#7296)

 - Automated API Docs update

 - New: Don't default manual import to move

 - Fixed: Cutoff Unmet showing items above lowest accepted quality when upgrades are disabled

 - New: Collections View

 - Translated using Weblate (Portuguese (Brazil)) [skip ci]

 - Translated using Weblate (Portuguese (Brazil)) [skip ci]

 - New: Parse QxR Group r00t

 - Automated API Docs update

 - New: Instance name in System/Status API endpoint

 - New: Instance name for Page Title

 - New: Instance Name used for Syslog

 - New: Set Instance Name

 - New: Add optional Source Title column to history

 - New: Support for new Nyaa RSS Feed format

 - Fixed: Don't try to add MovieMeta if mapping fails for list items

 - Fixed: Importing file from UNC shared folder without job folder

 - Fixed: No restart requirement for Refresh Monitored interval change

 - Fixed: Correct User-Agent api logging

 - Delete nan.json

 - Delete zh_Hans.json

 - Translated using Weblate (Chinese (Simplified)) [skip ci]

 - Fixed: Wrong translation mapping can be used for file naming and metadata

 - Fixed: Translated fields are mapped incorrectly for existing search results

 - Fixed: UI hiding search results with duplicate GUIDs

 - Fixed: QBittorrent unknown download state: forcedMetaDL

 - Fix migration 207 distinct on tmdbid only for list movie insert

 - Fix metadata migration

 - Automated API Docs update

 - Rework Movie Metadata data model

 - Temporarily ignore update tests until linux-x86 released

 - New: Add linux-x86 builds

 - New: Support Plex API Path Scan (Similar to autoscan)

 - Fixed: Interactive Search Filter not filtering multiple qualities in the same filter row

 - Added padding to search tab to maintain visual consistancy

 - Fixed: Update ScheduledTask cache LastStartTime on command execution

 - Bump Version to 4.2

 - Bump webpack packages

 - Remove old DotNetVersion method and dep

 - Bump Monotorrent to 2.0.5

 - Fixed: Don't die if Plex watchlist guid node is missing or null

 - Automated API Docs update

 - New: Add support for Plex Watchlist importing (#5707)

 - New: Add date picker for custom filter dates

 - Make postgres integration tests actually use postgres

 - Fixed: Clarify Qbit Content Path Error

 - Fixed: Use Movie Original Language for Custom Format Original Language (#6882)

 - Fix .editorconfig to disallow `this`

 - FFMpeg 5.0.1

 - Fixed: Properly handle 119 error code from Synology Download Station

 - Translated using Weblate (Hungarian) [skip ci]

 - Fixed: FFprobe failing on MacOS and AV1 streams

 - add 576 resolution back to simple title regex

 - Translated using Weblate (Ukrainian) [skip ci]

 - Set up tests on postgres

 - Allow configuring postgres with environment variables

 - New: Postgres Support

 - Other bug fixes and improvements, see GitHub history
