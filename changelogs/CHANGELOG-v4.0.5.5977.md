# New Beta Release

Radarr v4.0.5.5977 has been released on `develop`

A reminder about the `develop` branch

- **develop - Current Develop/Beta - (Beta): This is the testing edge. Released after tested in nightly to ensure no immediate issues. New features and bug fixes released here first. This version will receive updates either weeklyish or bi-weeklyish depending on development.**

# Announcements

- **Due to undocumented and breaking API changes Qbit v4.4.0 is not supported.  It is generally recommended to avoid Qbit .0 releases.** Qbit v4.3.9 is the most recent working version. Qbit v4.4.1 may have issues as well.
- **Radarr v4 no longer supports Linux x86 (x32 bit) systems**
  - x32 Arm is still supported; armv7 is the minimum required architecture
  - Impacted users have been receiving a healthcheck since May 2021 with 3.2.0
- **Radarr v4 no longer builds for mono and mono support has ceased**
  - Impacted users have been receiving a healthcheck since May 2021 with 3.2.0
- **Radarr Breaking API Changes**
  - Radarr v4 no longer supports the legacy (v0.2) API
  - Native ASPCore API Controllers (stricter typing and other small API changes)
  - The json you post needs to actually be strictly valid json now
- **FFProbe has replaced MediaInfo**
- Similarly MediaInfo is no longer a required dependency
- [Jackett `/all` is deprecated and no longer supported. The FAQ has warned about this since May 2021.](https://wiki.servarr.com/radarr/faq#jacketts-all-endpoint)
- Radarr is now on .Net6
- New builds for OSX Arm64 and Linux Musl Arm32
- IMDb Ratings
- **Users who do not wish to be on the alpha `nightly` testing branch should take advantage of this parity and switch to `develop`

# Additional Commentary

- Lidarr v1 coming to `develop` as beta soon^(tm)
- [Readarr official beta on `develop` announced](https://www.reddit.com/r/Readarr/comments/sxvj8y/new_beta_release_develop_v0101248/)
- [Radarr Postgres Database Support coming soon (PR#6873)](https://github.com/radarr/radarr/pull/6873)
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

- QNAP - Please ask the SynoCommunity to update the base package; however, you should be able to update in-app normally

------------

# Release Notes

## v4.0.5.5977 (changes since v4.0.4.5922)

 - Update Synology error codes

 - Fixed: Remove pre-DB from frontend storage

 - Fixed: Removing multiple items from the queue wording

 - Fixed: Improve help text for download client Category

 - New: Update Cert Validation Help Text [common]

 - Fixed: Updated ruTorrent stopped state helptext

 - fixed text box not being uniform to others

 - New: Add backup size information

 - Fix swagger inCinema references

 - Fixed: Recycle bin log message

 - Fix nzbdrone reference

 - additional testcase obfuscation

 - Fixed: IPv4 instead of IP4

 - Report runtime identifier to sentry

 - Update API URL

 - Fixed: No longer require first run as admin on windows

 - Build installer from build.sh

 - Fixed: Enable response compression over https

 - Bump to 4.0.5

- Other bug fixes and improvements, see GitHub history
