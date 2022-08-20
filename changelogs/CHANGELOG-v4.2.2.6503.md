# New Beta Release

Radarr v4.2.2.6503 has been released on `develop`

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

## v4.2.2.6503 (changes since [v4.2.1.6478](https://www.reddit.com/r/radarr/comments/woe62q/new_release_develop_v4216478/))

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

 - Other bug fixes and improvements, see GitHub history
