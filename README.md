# Lidarr

[![Build status](https://ci.appveyor.com/api/projects/status/tpm5mj5milne88nc?svg=true)](https://ci.appveyor.com/project/lidarr/lidarr)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/4e6d014aee9542189b4abb0b1439980f)](https://www.codacy.com/app/Lidarr/Lidarr?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=lidarr/Lidarr&amp;utm_campaign=Badge_Grade)
![Docker Pulls](https://img.shields.io/docker/pulls/linuxserver/lidarr.svg)
[![Backers on Open Collective](https://opencollective.com/lidarr/backers/badge.svg)](#backers) [![Sponsors on Open Collective](https://opencollective.com/lidarr/sponsors/badge.svg)](#sponsors)

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

## Contributors

This project exists thanks to all the people who contribute. [[Contribute](CONTRIBUTING.md)].
<a href="https://github.com/lidarr/Lidarr/graphs/contributors"><img src="https://opencollective.com/lidarr/contributors.svg?width=890&button=false" /></a>


## Backers

Thank you to all our backers! 🙏 [[Become a backer](https://opencollective.com/lidarr#backer)]

<a href="https://opencollective.com/lidarr#backers" target="_blank"><img src="https://opencollective.com/lidarr/backers.svg?width=890"></a>


## Sponsors

Support this project by becoming a sponsor. Your logo will show up here with a link to your website. [[Become a sponsor](https://opencollective.com/lidarr#sponsor)]

<a href="https://opencollective.com/lidarr/sponsor/0/website" target="_blank"><img src="https://opencollective.com/lidarr/sponsor/0/avatar.svg"></a>
<a href="https://opencollective.com/lidarr/sponsor/1/website" target="_blank"><img src="https://opencollective.com/lidarr/sponsor/1/avatar.svg"></a>
<a href="https://opencollective.com/lidarr/sponsor/2/website" target="_blank"><img src="https://opencollective.com/lidarr/sponsor/2/avatar.svg"></a>
<a href="https://opencollective.com/lidarr/sponsor/3/website" target="_blank"><img src="https://opencollective.com/lidarr/sponsor/3/avatar.svg"></a>
<a href="https://opencollective.com/lidarr/sponsor/4/website" target="_blank"><img src="https://opencollective.com/lidarr/sponsor/4/avatar.svg"></a>
<a href="https://opencollective.com/lidarr/sponsor/5/website" target="_blank"><img src="https://opencollective.com/lidarr/sponsor/5/avatar.svg"></a>
<a href="https://opencollective.com/lidarr/sponsor/6/website" target="_blank"><img src="https://opencollective.com/lidarr/sponsor/6/avatar.svg"></a>
<a href="https://opencollective.com/lidarr/sponsor/7/website" target="_blank"><img src="https://opencollective.com/lidarr/sponsor/7/avatar.svg"></a>
<a href="https://opencollective.com/lidarr/sponsor/8/website" target="_blank"><img src="https://opencollective.com/lidarr/sponsor/8/avatar.svg"></a>
<a href="https://opencollective.com/lidarr/sponsor/9/website" target="_blank"><img src="https://opencollective.com/lidarr/sponsor/9/avatar.svg"></a>

### License

* [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
* Copyright 2010-2017
