# Radarr

[![Build Status](https://dev.azure.com/Radarr/Radarr/_apis/build/status/Radarr.Radarr?branchName=develop)](https://dev.azure.com/Radarr/Radarr/_build/latest?definitionId=1&branchName=develop)
[![Translated](https://translate.servarr.com/widgets/radarr/-/radarr/svg-badge.svg)](https://translate.servarr.com/engage/radarr/?utm_source=widget)
[![Docker Pulls](https://img.shields.io/docker/pulls/linuxserver/radarr.svg)](https://github.com/Radarr/Radarr/wiki/Docker)
![Github Downloads](https://img.shields.io/github/downloads/Radarr/Radarr/total.svg)
[![Backers on Open Collective](https://opencollective.com/Radarr/backers/badge.svg)](#backers) [![Sponsors on Open Collective](https://opencollective.com/Radarr/sponsors/badge.svg)](#sponsors)

Radarr is an __independent__ fork of [Sonarr](https://github.com/Sonarr/Sonarr) reworked for automatically downloading movies via Usenet and BitTorrent.

The project was inspired by other Usenet/BitTorrent movie downloaders such as CouchPotato.

## Getting Started

[![Installation](https://img.shields.io/badge/wiki-installation-brightgreen.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr/wiki/Installation)
[![Docker](https://img.shields.io/badge/wiki-docker-1488C6.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr/wiki/Docker)
[![Setup Guide](https://img.shields.io/badge/wiki-setup_guide-orange.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr/wiki/Setup-Guide)
[![FAQ](https://img.shields.io/badge/wiki-FAQ-BF55EC.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr/wiki/FAQ)

* [Install Radarr for your desired OS](https://github.com/Radarr/Radarr/wiki/Installation) *or* use [Docker](https://github.com/Radarr/Radarr/wiki/Docker)
* *For Linux users*, run `radarr` and *optionally* have [Radarr start automatically](https://github.com/Radarr/Radarr/wiki/Autostart-on-Linux)
* Connect to the UI through <http://localhost:7878> or <http://your-ip:7878> in your web browser
* See the [Setup Guide](https://github.com/Radarr/Radarr/wiki/Setup-Guide) for further configuration

## Downloads

| Release Type    | Branch: develop (stable)                                                                                                                                                     | Branch: nightly (semi-unstable)                                                                                                                                                                | Branch: aphrodite (very-unstable)                                                                                                                                              |
|-----------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Binary Releases | [![GitHub Releases](https://img.shields.io/badge/downloads-releases-brightgreen.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr/releases)                 | [![AppVeyor Builds](https://img.shields.io/badge/downloads-nightly-green.svg?maxAge=60&style=flat-square)](https://ci.appveyor.com/project/galli-leo/radarr-usby1/branch/develop/artifacts)    |                                                                                                                                                                                |
| Docker          | [![Docker release](https://img.shields.io/badge/linuxserver-radarr:latest-blue.svg?colorB=1488C6&maxAge=60&style=flat-square)](https://hub.docker.com/r/linuxserver/radarr)  | [![Docker nightly](https://img.shields.io/badge/linuxserver-radarr:nightly-blue.svg?colorB=1488C6&maxAge=60&style=flat-square)](https://hub.docker.com/r/linuxserver/radarr)                   | [![Docker aphrodite](https://img.shields.io/badge/linuxserver-radarr:preview-blue.svg?colorB=1488C6&maxAge=60&style=flat-square)](https://hub.docker.com/r/linuxserver/radarr) |
| Docker          | [![Docker release](https://img.shields.io/badge/hotio-radarr:latest-blue.svg?colorB=1488C6&maxAge=60&style=flat-square)](https://hub.docker.com/r/hotio/radarr)              | [![Docker nightly](https://img.shields.io/badge/hotio-radarr:unstable-blue.svg?colorB=1488C6&maxAge=60&style=flat-square)](https://hub.docker.com/r/hotio/radarr)                              | [![Docker aphrodite](https://img.shields.io/badge/hotio-radarr:aphrodite-blue.svg?colorB=1488C6&maxAge=60&style=flat-square)](https://hub.docker.com/r/hotio/radarr)           |

## Support

[![Discord](https://img.shields.io/badge/discord-chat-7289DA.svg?maxAge=60&style=flat-square)](https://discord.gg/AD3UP37)
[![Reddit](https://img.shields.io/badge/reddit-discussion-FF4500.svg?maxAge=60&style=flat-square)](https://www.reddit.com/r/radarr)
[![Feathub](https://img.shields.io/badge/feathub-requests-lightgrey.svg?maxAge=60&style=flat-square)](http://feathub.com/Radarr/Radarr)
[![GitHub](https://img.shields.io/badge/github-issues-red.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr/issues)
[![GitHub Wiki](https://img.shields.io/badge/github-wiki-181717.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr/wiki)

## Status

[![GitHub issues](https://img.shields.io/github/issues/radarr/radarr.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr/radarr/radarr.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr/pulls)
[![GNU GPL v3](https://img.shields.io/badge/license-GNU%20GPL%20v3-blue.svg?maxAge=60&style=flat-square)](http://www.gnu.org/licenses/gpl.html)
[![Copyright 2010-2020](https://img.shields.io/badge/copyright-2020-blue.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr)
[![Github Releases](https://img.shields.io/github/downloads/Radarr/Radarr/total.svg?maxAge=60&style=flat-square)](https://github.com/Radarr/Radarr/releases/)
[![Docker Pulls](https://img.shields.io/docker/pulls/linuxserver/radarr.svg?maxAge=60&style=flat-square)](https://hub.docker.com/r/linuxserver/radarr/)
[![Changelog](https://img.shields.io/github/commit-activity/w/radarr/radarr.svg?style=flat-square)](/CHANGELOG.md#unreleased)

### [Site and API Status](https://status.radarr.video)

Radarr is currently undergoing rapid development and pull requests are actively added into the repository.

## Features

### Current Features

* Adding new movies with lots of information, such as trailers, ratings, etc.
* Support for major platforms: Windows, Linux, macOS, Raspberry Pi, etc.
* Can watch for better quality of the movies you have and do an automatic upgrade. *eg. from DVD to Blu-Ray*
* Automatic failed download handling will try another release if one fails
* Manual search so you can pick any release or to see why a release was not downloaded automatically
* Full integration with SABnzbd and NZBGet
* Automatically searching for releases as well as RSS Sync
* Automatically importing downloaded movies
* Recognizing Special Editions, Director's Cut, etc.
* Identifying releases with hardcoded subs
* All indexers supported by Sonarr also supported
* New PassThePopcorn Indexer
* QBittorrent, Deluge, rTorrent, Transmission and uTorrent download client (Other clients are coming)
* New TorrentPotato Indexer
* Torznab Indexer now supports Movies (Works well with [Jackett](https://github.com/Jackett/Jackett))
* Scanning PreDB to know when a new release is available
* Importing movies from various online sources, such as IMDb Watchlists (A complete list can be found [here](https://github.com/Radarr/Radarr/issues/114))
* Full integration with Kodi, Plex (notification, library update)
* And a beautiful UI
* Importing Metadata such as trailers or subtitles
* Adding metadata such as posters and information for Kodi and others to use
* Advanced customization for profiles, such that Radarr will always download the copy you want

### Planned Features

See the [Roadmap blogpost](https://blog.radarr.video/development/update/2018/11/11/roadmap-update.html) for an overview of planned features.

#### [Feature Requests](http://feathub.com/Radarr/Radarr)

## Configuring the Development Environment

### Requirements

* [Visual Studio Community 2019](https://www.visualstudio.com/vs/community/) or [Rider](http://www.jetbrains.com/rider/)
* [Git](https://git-scm.com/downloads)
* [Node.js](https://nodejs.org/en/download/)
* [Yarn](https://yarnpkg.com/)

### Setup

* Make sure all the required software mentioned above are installed
* Clone the repository into your development machine ([*info*](https://help.github.com/desktop/guides/contributing/working-with-your-remote-repository-on-github-or-github-enterprise))
* Install the required Node Packages `yarn install`
* Start gulp to monitor your dev environment for any changes that need post processing using `yarn start` command.

> **Notice**  
> Gulp must be running at all times while you are working with Radarr client source files.

### Build

* To build run `sh build.sh`

**Note:** Windows users must have bash available to do this. If you installed git, you should have a git bash utility that works.

### Development

* Open `Radarr.sln` in Visual Studio 2019 or run the build.sh script, if Mono is installed. Alternatively you can use Jetbrains Rider, since it works on all Platforms.
* Make sure `NzbDrone.Console` is set as the startup project
* Run `build.sh` before running, or build in VS

## Supporters

This project would not be possible without the support by these amazing folks. [**Become a sponsor or backer**](https://opencollective.com/radarr) to help us out!

### Sponsors

[![Sponsors](https://opencollective.com/radarr/tiers/sponsor.svg)](https://opencollective.com/radarr/order/3851)

### Flexible Sponsors

[![Flexible Sponsors](https://opencollective.com/radarr/tiers/flexible-sponsor.svg?avatarHeight=54)](https://opencollective.com/radarr/order/3856)

### Backers

[![Backers](https://opencollective.com/radarr/tiers/backer.svg?avatarHeight=48)](https://opencollective.com/radarr/order/3850)

### JetBrains

Thank you to [<img src="/Logo/jetbrains.svg" alt="JetBrains" width="32"> JetBrains](http://www.jetbrains.com/) for providing us with free licenses to their great tools

* [<img src="/Logo/resharper.svg" alt="ReSharper" width="32"> ReSharper](http://www.jetbrains.com/resharper/)
* [<img src="/Logo/webstorm.svg" alt="WebStorm" width="32"> WebStorm](http://www.jetbrains.com/webstorm/)
* [<img src="/Logo/rider.svg" alt="Rider" width="32"> Rider](http://www.jetbrains.com/rider/)
* [<img src="/Logo/dottrace.svg" alt="dotTrace" width="32"> dotTrace](http://www.jetbrains.com/dottrace/)

## License

* [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
* Copyright 2010-2020
