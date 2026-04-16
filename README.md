# Radarr

### Installation

This fork is designed to be a drop-in replacement for existing Radarr docker installations. Simply replace your radarr docker image with `ghcr.io/realzombee/radarr:develop`

Sample docker compose:
```docker
radarr:
    image: ghcr.io/realzombee/radarr:develop
    container_name: radarr
    environment:
      - PUID=1000
      - PGID=1000
      - TZ=Etc/UTC
      # Add env vars for any tweaks you want to enable
      - IGNORE_MATCH_BY_ID_WARNING=true
    volumes:
      - /path/to/radarr/data:/config
      # Additional volume mounts for your media, etc
    ports:
      - 7878:7878
    restart: unless-stopped
```

### Why this fork?

This fork aims to improve certain aspects of Radarr to make it work better with remote "infinite" library setups (Debrid/Usenet streaming, etc). This fork will be kept up-to-date with the Radarr develop branch and the changes in this fork are fully compatible with the original Radarr configs so you can freely swap back and forth between them.

This fork provides two categories of changes:

### Fixes
These are universal bug-fixes that should be fixed in the original Radarr project. These have been submitted as pull requests but might not make it into a release until a month or two.

- ffprobe issues: There's a bug in VideoFileInfoReader that causes ffprobe to read the entire file during HDR analysis if the video stream is at a non-zero index. This is especially problematic for remote files since it uses bandwidth unnecessarily: https://github.com/Radarr/Radarr/pull/11364
- RefreshMonitoredDownloadsCommand: Tools like decypharr and nzbdav issue this command after processing a download. But when this command is issued through the API or through the UI, it has a `Normal` priority, causing a buildup of queue items if lots of searches are triggered at once: https://github.com/Radarr/Radarr/pull/11365
- Fix a bug where Radarr silently ignores torrents that have been previously imported, deleted and then grabbed again. Without this, repairs from tools like decypharr don't work reliably.

### Tweaks

These are small changes to Radarr behavior to optimize for debrid/usenet streaming setups. These can be turned on through environment variables.

#### IGNORE_MATCH_BY_ID_WARNING

This setting turns off this warning

```
Found matching movie via grab history, but release was matched to movie by ID. Manual Import required
``` 

This generally happens on usenet indexers that include a `tmdbId` in releases that Radarr uses to match against movies while downloading. But during imports, if the files are obfuscated or the file/movie name doesn't match up with Radarr's expectations, it blocks automatic import.

⚠️ Warning: Only use this setting if you trust your indexers to provide the correct tmdbIds when they're present on releases.

### Contributing

Feel free to open issues or pull requests for any changes you'd like to see.

