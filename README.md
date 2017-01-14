# Radarr

| Service  | Master                      | Develop                      |
|----------|:---------------------------:|:----------------------------:|
| AppVeyor | [![AppVeyor](https://img.shields.io/appveyor/ci/galli-leo/Radarr/master.svg?maxAge=60&style=flat-square)](https://ci.appveyor.com/project/galli-leo/Radarr) | [![AppVeyor](https://img.shields.io/appveyor/ci/galli-leo/Radarr-usby1/develop.svg?maxAge=60&style=flat-square)](https://ci.appveyor.com/project/galli-leo/Radarr-usby1) |
| Travis   | [![Travis](https://img.shields.io/travis/galli-leo/Radarr/master.svg?maxAge=60&style=flat-square)](https://travis-ci.org/galli-leo/Radarr) | [![Travis](https://img.shields.io/travis/galli-leo/Radarr/develop.svg?maxAge=60&style=flat-square)](https://travis-ci.org/galli-leo/Radarr) |

This fork of Sonarr aims to turn it into something like CouchPotato.

## Currently working:

* Adding new movies
* Manually searching for releases of movies.
* Automatically searching for releases.
* Automatically importing downloaded movies.
* Recognizing Special Editions, Director's Cut, etc.
* Identifying releases with hardcoded subs.
* Rarbg.to, Torznab and Newznab Indexer.
* QBittorrent and Deluge download client (Other clients are coming)
* New TorrentPotato Indexer (Works well with [Jackett](https://github.com/Jackett/Jackett))

## Planned Features:

* Scanning PreDB to know when a new release is available.
* Fixing the other Indexers and download clients.
* Importing of Sonarr config.

## Download

Release builds can be found on:
* [GitHub](https://github.com/Radarr/Radarr/releases)

Continuous builds can be found on:
* [AppVeyor](https://ci.appveyor.com/project/galli-leo/radarr-usby1/build/artifacts)

Docker containers from [linuxserver.io](http://tools.linuxserver.io/dockers) can be found here:
* [x64](https://store.docker.com/community/images/linuxserver/radarr)
* [armhf](https://store.docker.com/community/images/lsioarmhf/radarr)
* [aarch64](https://store.docker.com/community/images/lsioarmhf/radarr-aarch64)

To connect to the UI, fire up your browser and open http://localhost:7878 or http://your-ip:7878.

## Major Features Include:

* Support for major platforms: Windows, Linux, macOS, Raspberry Pi, etc.
* Can watch for better quality of the movies you have and do an automatic upgrade. *eg. from DVD to Blu-Ray*
* Automatic failed download handling will try another release if one fails
* Manual search so you can pick any release or to see why a release was not downloaded automatically
* Full integration with SABnzbd and NZBGet
* Full integration with Kodi, Plex (notification, library update, metadata)
* And a beautiful UI

## Configuring Development Environment:

### Requirements

* Visual Studio 2015 [Free Community Edition](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx) or Mono
* [Git](https://git-scm.com/downloads)
* [NodeJS](https://nodejs.org/download/)

### Setup

* Make sure all the required software mentioned above are installed.
* Clone the repository into your development machine. [*info*](https://help.github.com/articles/working-with-repositories)
* Grab the submodules `git submodule init && git submodule update`
* Install the required Node Packages `npm install`
* Start gulp to monitor your dev environment for any changes that need post processing using `npm start` command.

*Please note gulp must be running at all times while you are working with Radarr client source files.*

### Development

* Open `NzbDrone.sln` in Visual Studio or run the build.sh script, if Mono is installed.
* Make sure `NzbDrone.Console` is set as the startup project

### License

* [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
* Copyright 2010-2016

### Sponsors

* [JetBrains](http://www.jetbrains.com/) for providing us with free licenses to their great tools
    * [ReSharper](http://www.jetbrains.com/resharper/)
    * [WebStorm](http://www.jetbrains.com/webstorm/)
    * [TeamCity](http://www.jetbrains.com/teamcity/)
