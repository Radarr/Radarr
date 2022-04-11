# New Beta Release

Radarr v4.1.0.6122 has been released on `develop`

- **Users who do not wish to be on the alpha `nightly` testing branch should take advantage of this parity and switch to `develop`**

A reminder about the `develop` and `nightly` branches

- **develop - Current Develop/Beta - (Beta): This is the testing edge. Released after tested in nightly to ensure no immediate issues. New features and bug fixes released here first after nightly. It can be considered semi-stable, but is still beta. This version will receive updates either weekly or biweekly depending on development.**
- **nightly - Current Nightly/Unstable - (Alpha/Unstable) : This is the bleeding edge. It is released as soon as code is committed and passes all automated tests. This build may have not been used by us or other users yet. There is no guarantee that it will even run in some cases. This branch is only recommended for advanced users. Issues and self investigation are expected in this branch. Use this branch only if you know what you are doing and are willing to get your hands dirty to recover a failed update. This version is updated immediately.**

# Announcements

- Radarr Postgres Database Support has landed in `nightly` and will be coming to `develop` _soon_
- Radarr Plex Watchlist Support has landed in `nightly` and will be coming to `develop` _soon_

# Additional Commentary

- Lidarr v1 coming to `develop` as beta soon^(tm)
- [Lidarr](https://lidarr.audio/donate), [Prowlarr](https://prowlarr.com/donate), [Radarr](https://radarr.video/donate), [Readarr](https://readarr.com/donate) now accept direct bitcoin donations
- [Readarr official beta on `develop` announced](https://www.reddit.com/r/Readarr/comments/sxvj8y/new_beta_release_develop_v0101248/)
- [Lidarr Postgres Database Support in development (Draft PR#2625)](https://github.com/Lidarr/Lidarr/pull/2625)

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

## v4.1.0.6122 (changes since v4.1.0.6095)

 - Fixed: Loading old commands from database

 - Fixed: Scrolling in Firefox in small window (requires refresh)

 - Don't return early after re-running checks after startup grace period

 - Fixed: Delay health check notifications on startup

 - New: Schedule refresh and process monitored download tasks at high priority

 - Fixed: Use Digital Release in ChangeFileDate if no Physical

 - Fixed: Cleanup Temp files after backup creation

 - Centralise image choice, update to latest images

 - Translated using Weblate (Chinese (Simplified) (zh_CN)) [skip ci]

 - Other bug fixes and improvements, see GitHub history
