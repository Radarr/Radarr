# Radarr

### Why this fork?

This fork aims to improve certain aspects of Radarr to make it work better with remote "infinite" library setups (Debrid/Usenet streaming, etc). This fork will be kept up-to-date with the Radarr develop branch and is designed to be a drop-in replacement if you're using [linuxserver's docker images](https://hub.docker.com/r/linuxserver/radarr). The changes in this fork are fully compatible with the original Radarr configs so you can freely swap back and forth between them.

This fork provides two categories of changes:

### Fixes
These are universal bug-fixes that should be fixed in the original Radarr project. These have been submitted as pull requests but might not make it into a release until a month or two.

- ffprobe issues: There's a bug in VideoFileInfoReader that causes ffprobe to read the entire file during HDR analysis if the video stream is at a non-zero index. This is especially problematic for remote files since it uses bandwidth unnecessarily: https://github.com/Radarr/Radarr/pull/11364
- RefreshMonitoredDownloadsCommand: Tools like decypharr and nzbdav issue this command after processing a download. But when this command is issued through the API or through the UI, it has a `Normal` priority, causing a buildup of queue items if lots of searches are triggered at once: https://github.com/Radarr/Radarr/pull/11365

### Tweaks (coming soon)

These are small changes to Radarr behavior to optimize for debrid/usenet streaming setups. These can be turned on/off through environment variables.


