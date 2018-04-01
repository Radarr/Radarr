# Lidarr

[![Build status](https://ci.appveyor.com/api/projects/status/tpm5mj5milne88nc?svg=true)](https://ci.appveyor.com/project/lidarr/lidarr)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/4e6d014aee9542189b4abb0b1439980f)](https://www.codacy.com/app/Lidarr/Lidarr?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=lidarr/Lidarr&amp;utm_campaign=Badge_Grade)
![Docker Pulls](https://img.shields.io/docker/pulls/linuxserver/lidarr.svg)

Lidarr is a music collection manager for Usenet and BitTorrent users. It can monitor multiple RSS feeds for new tracks from your favorite artists and will grab, sort and rename them. It can also be configured to automatically upgrade the quality of files already downloaded when a better quality format becomes available.

## Major Features Include:

* Support for major platforms: Windows, Linux, macOS, Raspberry Pi, etc.
* Automatically detects new tracks.
* Can scan your existing library and download any missing tracks.
* Can watch for better quality of the tracks you already have and do an automatic upgrade.
* Automatic failed download handling will try another release if one fails
* Manual search so you can pick any release or to see why a release was not downloaded automatically
* Fully configurable track renaming
* Full integration with SABnzbd and NZBGet
* Full integration with Kodi, Plex (notification, library update, metadata)
* Full support for specials and multi-album releases
* And a beautiful UI

## Feature Requests

[![Feature Requests](http://feathub.com/lidarr/Lidarr?format=svg)](http://feathub.com/lidarr/Lidarr)

## Support

[![Discord](https://img.shields.io/badge/discord-chat-7289DA.svg?maxAge=60)](https://discord.gg/8Y7rDc9)
[![Reddit](https://img.shields.io/badge/reddit-discussion-FF4500.svg?maxAge=60)](https://www.reddit.com/r/lidarr)
[![GitHub](https://img.shields.io/badge/github-issues-red.svg?maxAge=60)](https://github.com/Lidarr/Lidarr/issues)
[![GitHub Wiki](https://img.shields.io/badge/github-wiki-181717.svg?maxAge=60)](https://github.com/Lidarr/Lidarr/wiki)

## Configuring Development Environment:

### Requirements

* Visual Studio 2017 or higher (https://www.visualstudio.com/vs/).  The community version is free and works (https://www.visualstudio.com/downloads/).
* [Git](https://git-scm.com/downloads)
* [NodeJS](https://nodejs.org/en/download/) (Node 6.X.X or higher)
* [Yarn](https://yarnpkg.com/)

### Setup

* Make sure all the required software mentioned above are installed.
* Clone the repository into your development machine. [*info*](https://help.github.com/articles/working-with-repositories)
* Grab the submodules `git submodule init && git submodule update`
* Install the required Node Packages `yarn install`
* Start gulp to monitor your dev environment for any changes that need post processing using `yarn start` command.
* Build the project in Visual Studio
* Run the project in Visual Studio
* Open http://localhost:8686

*Please note gulp must be running at all times while you are working with Lidarr client source files.*

### Development

* Open `Lidarr.sln` in Visual Studio
* Make sure `NzbDrone.Console` is set as the startup project
* Change build to 'Debug x86'

### License

* [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
* Copyright 2010-2017
