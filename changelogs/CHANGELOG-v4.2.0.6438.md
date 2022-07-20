# New Beta Release

Radarr v4.2.0.6438 has been released on `develop`

- **Users who do not wish to be on the alpha `nightly` testing branch should take advantage of this parity and switch to `develop`**

A reminder about the `develop` and `nightly` branches

- **develop - Current Develop/Beta - (Beta): This is the testing edge. Released after tested in nightly to ensure no immediate issues. New features and bug fixes released here first after nightly. It can be considered semi-stable, but is still beta. This version will receive updates either weekly or biweekly depending on development.**
- **nightly - Current Nightly/Unstable - (Alpha/Unstable) : This is the bleeding edge. It is released as soon as code is committed and passes all automated tests. This build may have not been used by us or other users yet. There is no guarantee that it will even run in some cases. This branch is only recommended for advanced users. Issues and self investigation are expected in this branch. Use this branch only if you know what you are doing and are willing to get your hands dirty to recover a failed update. This version is updated immediately.**

# Announcements

- Radarr Postgres Database Support
- Radarr Plex Watchlist Support
- Radarr Collections Support

# Additional Commentary

- [Lidarr v1 released on `master`](https://www.reddit.com/r/Lidarr/comments/v5fdhi/new_stable_release_master_v1022592/)
- [Lidarr](https://lidarr.audio/donate), [Prowlarr](https://prowlarr.com/donate), [Radarr](https://radarr.video/donate), [Readarr](https://readarr.com/donate) now accept direct bitcoin donations
- [Readarr official beta on `develop` announced](https://www.reddit.com/r/Readarr/comments/sxvj8y/new_beta_release_develop_v0101248/)
- Radarr Postgres Database Support in `nightly` and `develop`
- Prowlarr Postgres Database Support in `nightly` and `develop`
- [Lidarr Postgres Database Support in development (Draft PR#2625)](https://github.com/Lidarr/Lidarr/pull/2625)
- \*Arrs Wiki Contributions welcomed and strongly encouraged, simply auth with GitHub on the wiki and update the page

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

## v4.2.0.6438 (changes since v4.1.0.6175)

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
