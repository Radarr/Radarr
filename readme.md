# Radarr [![Build Status](https://travis-ci.org/galli-leo/Radarr.svg?branch=develop)](https://travis-ci.org/galli-leo/Radarr)#

This fork of Sonarr aims to turn it into something like Couchpotato.

## Currently working:
* Adding new movies
* Manually searching for releases of movies.
* Automatically searching for releases.
* Rarbg.to indexer (Other indexers are coming, I just need to find the right categories)
* QBittorrent download client (Other clients are coming)

## Planned Features:
* Scanning PreDB to know when a new release is available.
* Fixing the other Indexers and download clients.
* Fixing how movies are parsed.
* Fixing movie import.
* Importing of Sonarr config.
* New TorrentPotato Indexer.

## Download
The latest precompiled binary versions can be found here: https://github.com/galli-leo/Radarr/releases.

For more up to date versions (but also sometimes broken), daily builds can be found here:
* [OSX](https://leonardogalli.ch/radarr/builds/latest.php?os=osx)
* [Windows](https://leonardogalli.ch/radarr/builds/latest.php?os=windows)
* [Linux](https://leonardogalli.ch/radarr/builds/latest.php?os=mono)

## Major Features Include: ##

* Support for major platforms: Windows, Linux, OSX, Raspberry Pi, etc.
* Automatically detects new episodes
* Can scan your existing library and download any missing episodes
* Can watch for better quality of the episodes you already have and do an automatic upgrade. *eg. from DVD to Blu-Ray*
* Automatic failed download handling will try another release if one fails
* Manual search so you can pick any release or to see why a release was not downloaded automatically
* Fully configurable episode renaming
* Full integration with SABNzbd and NzbGet
* Full integration with XBMC, Plex (notification, library update, metadata)
* Full support for specials and multi-episode releases
* And a beautiful UI

## Configuring Development Environment: ##

### Requirements ###
- Visual Studio 2015 [Free Community Edition](https://www.visualstudio.com/en-us/products/visual-studio-community-vs.aspx) or Mono
- [Git](http://git-scm.com/downloads)
- [NodeJS](http://nodejs.org/download/)

### Setup ###

- Make sure all the required software mentioned above are installed.
- Clone the repository into your development machine. [*info*](https://help.github.com/articles/working-with-repositories)
- Grab the submodules `git submodule init && git submodule update`
- install the required Node Packages `npm install`
- start gulp to monitor your dev environment for any changes that need post processing using `npm start` command.

*Please note gulp must be running at all times while you are working with Sonarr client source files.*


### Development ###
- Open `NzbDrone.sln` in Visual Studio or run the build.sh script, if Mono is installed.
- Make sure `NzbDrone.Console` is set as the startup project


### License ###
* [GNU GPL v3](http://www.gnu.org/licenses/gpl.html)
Copyright 2010-2016


### Sponsors ###
- [JetBrains](http://www.jetbrains.com/) for providing us with free licenses to their great tools
    - [ReSharper](http://www.jetbrains.com/resharper/)
    - [WebStorm](http://www.jetbrains.com/webstorm/)
    - [TeamCity](http://www.jetbrains.com/teamcity/)
