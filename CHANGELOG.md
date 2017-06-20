# Changelog

## v0.2.0.778 (2017-06-20)

### **New Features**

- Radarr API url now points to new v2 version. [Leonardo Galli]

- Changed Name of Radarr Lists. [Leonardo Galli]

- More detailed descriptions why a movie was not able to be mapped. (#1696) [Leonardo Galli]

- Options to make parsing more lenient. (Adds support for some german and french releasegroups) (#1692) [Leonardo Galli]

- Bootstrap Tags Input (#1674) [Mitchell Cash]

- Include css files in minification (#1672) [Mitchell Cash]

- Upgrade to Bootstrap 3.3.7 (#1673) [Mitchell Cash]

- Allow minimum seeders to be set on a per indexer basis. Pulled from Sonarr Upstream (#1624) [Leonardo Galli]

- Remove redundant IE meta tag as we use http header instead (#1655) [Mitchell Cash]

- Use cleancss for minification (#1654) [Mitchell Cash]

- Ability to see TMDB and lists going through the Radarr API on the discovery page. [Leonardo Galli]

- Search 5 alternative titles as well. This should help with french as well as movies with very different titles. [Leonardo Galli]

- [Radarr] tag for Twitter Notifications (#1558) [Jason Costomiris]

- Custom Class for Radarr API requests. Also implements new error handling present on staging server. [Leonardo Galli]

- Added HDBits Category, Codec, and Medium Filtering Capability (#1458) [randellhodges]

- Update radarr api url. [Leonardo Galli]

- Update TaskManager.cs. [Leonardo Galli]

- Update LogEntries token again :) [Leonardo Galli]

### **Fixes**

- Fix migration. [Leonardo Galli]

- Redirect calls missing URL Base (#1668) [Mitchell Cash]

- Twitter oAuth callback URL (#1669) [Mitchell Cash]

- Error when processing manual import decisions  (#1670) [Mitchell Cash]

- Create README.md. [Leonardo Galli]

- Add license. [Leonardo Galli]

- Urls missing from multiple indexers after latest nightly update. [Leonardo Galli]

- Follow 301 redirects when fetching torrents (#1653) [Mitchell Cash]

- Ensure an API Key is set when starting Radarr (#1652) [Mitchell Cash]

- Minimum availability is now working similarely to profile when adding a movie. [Leonardo Galli]

- Forgot to include some js files in the last commit. [Leonardo Galli]

- Fix error when we get invalid datetime from our api. [Leonardo Galli]

- Lossless compression of images saved 92KB (#1620) [Fish2]

- Mostly fixes UI glitches for list settings. [Leonardo Galli]

- Refresh IsDuplicate in bulk import when the tmdbId changes (#1570) [Sentir101]

- Encourage Torznab use with Jackett (#1559) [flightlevel]

- Fixed PTP indexer being disabled if no results are found for a movie. [Leonardo Galli]

- Fix basic naming settings. [Leonardo Galli]

- Discovery of upcoming movies points to our server now. [Leonardo Galli]

- Most likely fixed #745 now. [Mike]

- Chmod osx file as executable. (#1539) [Mike]

- Add IMDB URL to notifications (#1531) [tsubus]

- Fixed design calendar css bug (#1527) [Levi Wilcox]

- Correct Program Name (#1524) [Luke Anderson]

- Correct Program Name (#1523) [Luke Anderson]

- Osx updater now updates plist file to point to the correct executable binary. [Leonardo Galli]

- Using our own logentries token now. [Leonardo Galli]

- Fix osx updater failing. [Leonardo Galli]


## v0.2.0.696 (2017-05-12)

### **New Features**

- Update TaskManager.cs. [Leonardo Galli]

### **Fixes**

- Fix test. [Leonardo Galli]

- Movies with same name but different year being downloaded regardlessly is now fixed! [Leonardo Galli]


## v0.2.0.692 (2017-05-11)

### **New Features**

- Added ability to discover new movies based on upcoming blurays as well as popular movies (borrowed from steven lu :)) [Leonardo Galli]

- Update Kodi icon, fixes #1464 (#1492) [hotio]

- Added initial migration. [Leonardo Galli]

- Added trailer links to the discovery page. [Leonardo Galli]

- Added discovery tab based on tmdb recommendations based on your existing movies. (#1450) [Leonardo Galli]

- Change default page size to 250. Should help with safari timeouts. [Leonardo Galli]

- Added multiple new editions such as FanEdit, Anniversary and 2in1. [Leonardo Galli]

### **Fixes**

- Fixed design issue when deleting css bug (#1480) Fixes #1475. [Levi Wilcox]

- 10 Movies are now shown on discover as well as search results. [Leonardo Galli]

- Hotfix for when ignored movies would appear again after clicking on show more. [Leonardo Galli]

- Fix appveyor build. [Leonardo Galli]

- Completely overhauled how import exclusions work. [Leonardo Galli]

- Hopefully more logging to catch errors better. [Leonardo Galli]

- Fix: A small bug fix for items loading as undefined in organize modal. Movie titles should now show up correctly. (#1424) [PatrickGHanna]

- Fixed error when language is present in title, but has dots instead of spaces. For example The.Danish.Girl.2015. [Leonardo Galli]

- Fixed Final in titles parsing as an edition. [Leonardo Galli]

- Radarr not importing torrents in Vuze if the torrent already finished seeding and was stopped (#1471) [Mitchell Cash]

- Incorrect imports with Vuze when torrent contains a single file. (#1470) [Mitchell Cash]

- Sonarr UI Authentication cookie should be placed on path (UrlBase) instead of domain alone. Fixes ##1451. [Mitchell Cash]

- Use Post for tmdbids request, to avoid too long URIs. [Leonardo Galli]

- Tidy up layout of buttons on the Add Movies page for mobile/tablet (#1454) [David Pooley]

- Rename Sonarr to Radarr for OSX App. [morberg]

- Minor text fixes. [Leonardo Galli]

- Enable automatic renaming, according to naming scheme, of movie folder after creation of the movie. (#1349) [Leonardo Galli]

- Fix for error when clicking Rescan Drone Folder. [Leonardo Galli]

- Fix for error when trying to manually import. [Leonardo Galli]


## v0.2.0.654 (2017-04-18)

### **New Features**

- Change smtp.google.com to smtp.gmail.com (#1410) [Donald Webster]

- Updated debug movie title to include Year. [Leonardo Galli]

- Update Series reference to Movies, should fix #1399 (#1402) [hotio]

- Added test for fix in last commit. [Leonardo Galli]

- Update branch. [Leonardo Galli]

- Update packages.sh some more. [Leonardo Galli]

- Update package.sh script. [Leonardo Galli]

### **Fixes**

- Fix PTP_Approved turning into HDBits Internal. [Leonardo Galli]

- Fix ptp tests. [Leonardo Galli]

- AHD, PTP and HDB support the new indexer flags too now! Indexer flags can be preferred over other releases. [Leonardo Galli]

- Movies with Umlauts are now correctly matched and have correct CleanTitles. [Leonardo Galli]

- Minor Text fixes. [Leonardo Galli]

- Fix error when MinimumAvailability was Announced and Delay was negative. [Leonardo Galli]

- Disable PreDB sync for now. [Leonardo Galli]

- Stats are now sent to our server instead of Sonarr's :) [Leonardo Galli]

- Fix for sql error. Did not think everything through exactly. [Leonardo Galli]

- Fix when MovieTitle is the empty string (should not occur, but what evs) [Leonardo Galli]

- Fixes Movie Size not showing correctly. [Leonardo Galli]

- Fixed an issue where movies which were labelled with an alternative title could not be found. [Leonardo Galli]

- Indexer flags implementation. (#1377) Will be further finalized over the next few weeks with Freelech, preferring of certain flags, etc. [Leonardo Galli]

- Add default runtime when runtime info of tmdb says 0. [Leonardo Galli]

- Fixes an issue where the semicolon and space afterwards was replaced. [Leonardo Galli]

- Final tweak for package.sh. [Leonardo Galli]

- This should finally fix all packaging stuff. [Leonardo Galli]

- Test fixes. [Leonardo Galli]

- More test debugging. [Leonardo Galli]

- Remote Test debugging yey! [Leonardo Galli]

- Remove unecessary test. [Leonardo Galli]

- Using NUnit.Runners so that teamcity build works. [Leonardo Galli]

- Turn installer back on. [Leonardo Galli]

- Disabled installer being picked up, causes error with update api. [Leonardo Galli]

- Now artifacts get pushed even if tests fail. [Leonardo Galli]

- Fixed package script for Teamcity. [Leonardo Galli]

- Log if ParsedMovieInfo is NULL. [Leonardo Galli]

- Catching predb.me errors hopefully. [Leonardo Galli]


## v0.2.0.596 (2017-04-10)

### **New Features**

- Update nzbdrone.iss. [Leonardo Galli]

- Update appveyor.yml. [Leonardo Galli]

- Update build-appveyor.cake. [Leonardo Galli]

- Update appveyor.yml. [Leonardo Galli]

- Update appveyor.yml. [Leonardo Galli]

- Update nzbdrone.iss. [Leonardo Galli]

- Update build-appveyor.cake. [Leonardo Galli]

- Update build-appveyor.cake. [Leonardo Galli]

- Update appveyor.yml. [Leonardo Galli]

- Update build-appveyor.cake. [Leonardo Galli]

- Update appveyor.yml. [Leonardo Galli]

- Update nzbdrone.iss. [Leonardo Galli]

- Update nzbdrone.iss. [Leonardo Galli]

- Update README.md. [Leonardo Galli]

- Added "Additional Parameters Field" for Trakt RSS Feed (#1308) [rmangahas-coupa]

- Update ISSUE_TEMPLATE.md. [Devin Buhl]

- Update ISSUE_TEMPLATE.md. [Devin Buhl]

- Update ISSUE_TEMPLATE.md. [Devin Buhl]

### **Fixes**

- Just getting Appveyor to build. [Leonardo Galli]

- Installer should be built too now. [Leonardo Galli]

- Text fixes and got pending releases finally fully working. [Leonardo Galli]

- Fixed searching for movie after it is added from a list. [Leonardo Galli]

- Specific Subtitle tags (such as nlsub) can now be whitelisted and will be downloaded. [Leonardo Galli]

- Allow Hardcoded subs to be downloaded still. [Leonardo Galli]

- Catching HTTP Errors when adding movies from a list. [Leonardo Galli]

- SABnzbd 2.0 API compatibility (#1339) [Mitchell Cash]

- Zero length file causes MediaInfo hanging in 100% cpu load. (#1340) [Mitchell Cash]

- Newznab default capabilities erroneously cached if indexer is unavailable. (#1341) [Mitchell Cash]

- Cleanup on mapping logic. Movies with up to 4500 parts are now supported! [Rusk85]

- Released icon is back. [geogolem]

- Fixed spelling mistake. [Leonardo Galli]

- Fixed an error when searching for movies with no imdbid. [Leonardo Galli]

- DownloadStation api client for DSM 5.x. (#1259) [Marcelo Castagna]

- Should fix covers not being local. [Leonardo Galli]

- Fixed error when downloading a movie. [Leonardo Galli]

- Fixed only one movie appearing when list does not give us a tmdbid. [Leonardo Galli]

- This should fix all imdbid problems with indexers. [Leonardo Galli]

- Revert "Move up IMDB logic in ParsingService, should help with the mismatched movies" [Devin Buhl]

- Move up IMDB logic in ParsingService, should help with the mismatched movies. [Devin Buhl]

- Clean up jsHint warnings (#1225) [Zach]

- New movie search (#1212) [thejacer87]

- Fix pending release service, HDBits, also the release deduper. Clean up housekeeping (#1211) [Devin Buhl]

- Patch/onedr0p 3 16 17 (#1200) [Devin Buhl]

- Revert "Small changes to list sync (#1179)" [Devin Buhl]

- Small changes to list sync (#1179) [Devin Buhl]

- Patch/onedr0p 3 14 17 (#1171) [Devin Buhl]

- Maybe fix PTP? Don't have an account, so cannot test. [Leonardo Galli]

- Fix for editing quality of movie files. [Leonardo Galli]

- Patch/onedr0p 3 13 17 (#1166) [Devin Buhl]

- Fix issue where 1080p Telesyncs get tagged as 1080p Blurays. [Leonardo Galli]


## v0.2.0.535 (2017-03-12)

### **New Features**

- Update blacklist to work with movies (#1089) [Devin Buhl]

- Update README.md. [Leonardo Galli]

- Update error to include Radarr instead of Sonarr (#1069) [flightlevel]

- Update wiki link for sorting and renaming (#1045) [aptalca]

### **Fixes**

- Grammar check HelpText for CouchPotato lists (#1142) [James White]

- Preliminary Fix for downloaded error in Wanted section. [Leonardo Galli]

- Fixes banners when searching for new movies. [Leonardo Galli]

- Fix issue where searching for new movies is not possible. [Leonardo Galli]

- Add helptext for Jackett API key (#1121) [Mathew Giljum]

- Better method to obtain the folderName. [geogolem]

- Keep the current page the same after clicking Save. [geogolem]

- Parsing headers that have a trailing semi-colon (#1117) [Mitchell Cash]

- PreDB Integration. Update Library is advisable. [Leonardo Galli]

- QOL changes to PTP logic (#1114) [Devin Buhl]

- Fix for VS for Mac. [Leonardo Galli]

- Ammend to previous commit. [Leonardo Galli]

- Hopefully fix all issues with unlinked movie files. [Leonardo Galli]

- This needs to match with the property forclient mode. [geogolem]

- CP list hotfix. [Devin Buhl]

- Incorrect check for imdbId prefix. [geogolem]

- Fix regression for missing libgdiplus (#1073) [SWu]

- Refactor so that filteringExpressions are constructed in one place less code duplication, easier to manage moving forward. [geogolem]

- New filters were added, but they werent being handled via the API. [geogolem]

- Allow larger trakt lists than 500. [geogolem]

- Restructeured readme and added a new logo asset (#1088) [Matthew Treadwell]

- Onedr0p/3 8 17 (#1087) [Devin Buhl]

- Ensure drone factory runs on its specificed interval (#1067) [Tim Turner]

- Add hotio's nightly docker image. (#1084) [Donald Webster]

- Add Installation, Docker and Setup Guide to new Install section and add Feathub and Wiki to Support (#1083) [Donald Webster]

- Fixed the parser for movies with A. [Leonardo Galli]

- Loads only request movie first into full collection. Should fix things. (#1046) [Leonardo Galli]

- Addressing jshint warnings (#1050) [Bill Szeliga]

- Correct DownloadDescisionMaker to use ImdbId, and update the ui a little. (#1068) [Devin Buhl]

- Deluge 1.3.14 API support due to changed json-rpc checks. [Devin Buhl]

- Reverting a change made yesterday regarding sorting the change fixed sorting titles of newly added movies without a refresh however, people have noticed it broke sorting of "In Cinemas" column in general. i commented out the change; but also added a special case in the comment, that would fix the case in question, without breaking the others; however, more investigating is needed because there is an issue with sorting newly added movies in general and the fix this reverts was never good enough anyway. [geogolem]

- Oops -- this was a fix from the last merge - sorry. [geogolem]

- URLEncode the string for searching (#1055) [Mihai Blaga]

- Fix client mode fetching.. only setPageSize when necessary. [geogolem]

- Fix error with weirdly formatted audioChannelPositions on MediaInfo. [Leonardo Galli]

- Fix  a couple typos (#1049) [Greg Fitzgerald]

- Fix tests. [Devin Buhl]

- Patch/onedr0p (#1048) [Devin Buhl]

- Fixed all tests and even added some new ones :) (#835) [Leonardo Galli]

- Fixes issue where quality settings wont save due to no pagesize. [Leonardo Galli]

- Fixes sorting after adding movies. [geogolem]

- Fix most paging issues on first load. [Leonardo Galli]

- /movies without pagesize or page gives back the old format. [Leonardo Galli]

- This seems to make it more stable. [geogolem]

- Im not too sure why this fixes the problem but now the filterState is respected when returning from another page. [geogolem]

- Use href instead of hostname+port. [geogolem]

- Improve RSS parsing for movies without year. [Devin Buhl]

- Add ReplaceGermanUmlauts method. [Devin Buhl]


## v0.2.0.453 (2017-03-05)

### **New Features**

- Added new TestCase for Parser and fixed spelling error. [Devin Buhl]

- Added FindByAlternativeTitle in MovieRepo. [Devin Buhl]

- Added debug messages to check quality. [Leonardo Galli]

- Added more filters to the movie editor (#905) [geogolem]

- Update parsing french movies (#899) [Devin Buhl]

### **Fixes**

- Try to add year to release titles that have no year (foriegn release groups) (#1028) [Devin Buhl]

- Delay profiles are no longer hidden under advanced settings (#1019) [Mitchell Cash]

- Revert "Added FindByAlternativeTitle in MovieRepo." [Devin Buhl]

- Use http request builder (aided by onedrop) [geogolem]

- Improve indexer health check messages (#1015) [Mitchell Cash]

- Clean RSS feed before detecting type (#1014) [Mitchell Cash]

- Store titleSlug in tags for exclusions and always use TMDBID. [geogolem]

- Also use TMDBID on list sync. [geogolem]

- Always check exclusions with tmdbid. [geogolem]

- An updated radarrAPI has been deployed --> this commit makes trakt authentication ready to be merged to the develop branch. [geogolem]

- Fully functional traktAuthentication using api.couchpota.to with comments for when updated RadarrAPI is deployed. [geogolem]

- Fix error with null dates. [Devin Buhl]

- Patch/more updates (#1009) [Devin Buhl]

- Revert.. [Devin Buhl]

- Fixed "wrong" quality being detected. Scan will be slower though. [Leonardo Galli]

- Fix for wrong qualities showing up. Will be slower to load though. [Leonardo Galli]

- Patch/onedr0p 3 4 2017 (#1006) [Devin Buhl]

- Respect the page when initializing the layout. [geogolem]

- Patch/onedr0p updates (#998) [Devin Buhl]

- The movie was not being printed correctly, and i believe this was also causing movies to be added when they shouldnt have been... [geogolem]

- Clean up the fetching on loading of MovieEditor and MovieIndex once and for all. [geogolem]

- I dont know why i was doing this inside the for loop... It did not scale well ! fixed. [geogolem]

- Use clone so that we only detect empty collection when collectio is empty.. not when current filter is empty but collectionis not. [geogolem]

- I believe these are old code that is not needed since pagination.. [geogolem]

- Default Wanted and Cutoff to be 50 movies per page, added filtering options to Cutoff and a Search all (#984) [Devin Buhl]

- Empty string case should not be only for the contains case. [geogolem]

- Needed to pass the filterType, received the filterType and handle the filterType. [geogolem]

- Reset filters on save.. [geogolem]

- Possible fix for Custom script (#973) [Devin Buhl]

- Hotfix when importing movie (#971) [Devin Buhl]

- Fixed infinite loop. Added default destination test when adding client (#968) [Marcelo Castagna]

- Date added in Movie List & Possible Fix for Importing Movies. (#969) [Devin Buhl]

- Ensure collection is synced before opening movieDetails. [Tim Turner]

- Revert some changes -- use FullCollection (maybe just for now) [geogolem]

- Just show imdbid or tmdbid for now in exclusions. [geogolem]

- MovieIndexPage Stability + MovieEditor fix (#925) [geogolem]

- Patch/galileo fixes (#951) [Devin Buhl]

- Patch/updates onedr0p (#946) [Devin Buhl]

- Fixed problem with TMDb list when Year is null, Revert using UrlPathEncode on newznab requests (#937) [Devin Buhl]

- Expose more information to the Webhook notification (#935) [Ross Valler]

- Fix/implement Webhook notifications (#901) [Ross Valler]

- Add remux 1080p and 2160p as qualities (#900) [Devin Buhl]

- NZBGet delete:scan treated as failure (#898) [Mitchell Cash]

- Small changes. [Devin Buhl]

- Hotfix. [Devin Buhl]

- List sync with removal (#656) [geogolem]

- Fix the footer to show correct information and refresh when FullCollection changes (#893) [geogolem]

- Increase fullCollection page size, update Refresh Library command. [Tim Turner]

- Patch/updates (#887) [Devin Buhl]

- Fix poster placeholder height on small screens (#883) [hotio]

- Small UI fixes (#882) [hotio]

- Me = idiot. [Leonardo Galli]

- Fixed an issue where an unloaded movie could case linking to fail. [Leonardo Galli]

- Maybe fix issue with imported files not being linked to the movie? [Leonardo Galli]

- Search is now fixed too. [Leonardo Galli]

- Should fix most issues with paging. [Leonardo Galli]

- Add first steps of paging to movie editor. [Leonardo Galli]

- HDBits prefer/require internal release (#584) (#881) [Devin Buhl]

- Ignore Deleted Movies (#755) (#879) [Devin Buhl]

- First fixes for Movie Editor. Testing to see if this approach could work. [Leonardo Galli]

- Fix missing showing downloaded instead. [Leonardo Galli]

- Fix issue where details page wont load. [Leonardo Galli]

- Paging for movies :) (#861) [Leonardo Galli]

- Bug fixes (#874) [Devin Buhl]

- The Search All Missing button (#860) [geogolem]

- Cleanup min availability (#846) [geogolem]

- Some minor cleanup + changed filter on wanted/missing (#845) [geogolem]

- Min availability (#816) [geogolem]

- Add NZB Station for Synology (#841) [Devin Buhl]

- Patch/filter trakt (#838) [Devin Buhl]

- Fixed language parsing of movies with language in movie name. [Leonardo Galli]


## v0.2.0.375 (2017-02-22)

### **New Features**

- Update .travis.yml. [Leonardo Galli]

- Update notification logos (#804) [hotio]

- Update ISSUE_TEMPLATE.md. [Devin Buhl]

- Update PULL_REQUEST_TEMPLATE.md. [Devin Buhl]

- Update dl-clients (#732) [Devin Buhl]

- Update ISSUE_TEMPLATE.md. [Devin Buhl]

- Changed sort options to match UI (#707) [zductiv]

- Added test for ! [Leonardo Galli]

- Update parser tests. [Leonardo Galli]

### **Fixes**

- Patch/bulk import tests (#833) [Tim Turner]

- Patch/bulk import qol (#785) [Tim Turner]

- URL Encode for newznab query strings, closes #818 (#819) [Mihai Blaga]

- Rename Sonarr to Radarr in DownloadStation client (#812) [Mitchell Cash]

- Fixes error message for MovieExistsValidator to state the movie doesn't exist (#723) (#808) [Ryan Matthews]

- Set PROWL application to Radarr (#770) (#807) [Ryan Matthews]

- TMDb Lists should be working now :) (#775) [Devin Buhl]

- Roll back some code on Net Import (#772) [Devin Buhl]

- Check to see if output path is right when DownloadClient.Test is invoked (#768) [Marcelo Castagna]

- TMDb Filtering Options: Rating, Min Votes, Min Vote Ave, Original Language, TMDb Genre Ids (CSV), (#765) [Devin Buhl]

- Small consistancy updates to PTP and AwesomeHD (#758) [Devin Buhl]

- Patch/onedr0p (#757) [Devin Buhl]

- Handle download data diskstation (#744) [Marcelo Castagna]

- When refreshing movie, refresh Files tab. [Tim Turner]

- Feature/Add TMDb Functionality (#739) [Devin Buhl]

- Add downloaded quality column to movie editor (#738) [zductiv]

- Clean up Trakt a little (#735) [Devin Buhl]

- Add Synology Download Station (#725) [Devin Buhl]

- Fix pending release table. [Leonardo Galli]

- Fix Hardcoded .DKSubs. (#726) [Devin Buhl]

- NetImport - Do not allow TV Series / Mini-Series (works with IMDb) #699 (#727) [Devin Buhl]

- Patch/re add ghost migrations (#724) [Devin Buhl]

- Patch/onedr0p (#716) [Devin Buhl]

- Increase timeout when waiting for rTorrent to finish adding torrent (#721) [Mitchell Cash]

- Fix RescanMovie command for single movie. [Tim Turner]

- Hopefully fixes a lot of null reference bugs in BulkImport. [Leonardo Galli]

- Should fix blacklist items disappearing. [Leonardo Galli]

- Fix manual import for when downloaded movies are in a folder. [Leonardo Galli]

- Search all missing movie works - missing tab only (#710) [zductiv]

- Limit TMDb requests when importing via IMDBid (#703) [Devin Buhl]

- Fix parsing with lower bluray qualities. [Leonardo Galli]

- Fixes issue with movies with same name but different years being downloaded. [Leonardo Galli]

- Fixed a few parser issues. Also added some tests. [Leonardo Galli]

- Cutoff tab actually working now. [Leonardo Galli]

- Fix trakt links for movies (like sonarr for shows) (#690) [geogolem]

- Fixed Sorting In Wanted and Cutoff (#693) [Devin Buhl]

- Pass at seeing if this works on linux now (#692) [Devin Buhl]

- Small UI changes (#691) [zductiv]

- Add required flag for PTP (#688) [Devin Buhl]

- Wanted & Missing (#687) [Devin Buhl]

- * Make Missing/Wanted Work again (#686) [Devin Buhl]

- Fixed MovieMissingModule failed while processing [MovieDownloadedEvent] [Devin Buhl]

- UI Enhancements for Manual Import (#681) [Tim Turner]

- May be fix loading view? Idk. [Leonardo Galli]

- Display loading view when changing page size. [Tim Turner]

- Fix ordering in PTP, should prefer GP releases (#667) [Devin Buhl]

- Patch/onedr0p updates (#664) [Devin Buhl]

- Make Movie Title and Status sortable on Wanted tab (#662) [schumi2004]

- Fix paging breaking in bulk import. [Leonardo Galli]

- Bulk Import. (#583) [Leonardo Galli]


## v0.2.0.299 (2017-02-07)

### **New Features**

- Update notif list warning when importing from a list (#648) [Devin Buhl]

### **Fixes**

- Wait 5 seconds before getting the next 35 movies from TMDb using X-RateLimit-Remaining (#647) [Devin Buhl]

- Correct the Kickass migration (#649) [Devin Buhl]

- Fix movies not showing up in Queue when downloading (#640) [Devin Buhl]

- Fixed Movie link in history tab (#637) [Devin Buhl]

- Clean up download clients to use radarr as label, fix hoduken, and blackhole. (#635) [Devin Buhl]

- Use Movie Name-TmdbId for slug, update toUrlSlug (#629) [Devin Buhl]

- Removed Wombles and Kickass, updated torrentpotato and torznab (#625) [Devin Buhl]

- Various ui text fixes (#620) [Abzie]

- Delay Profile: Fix for when preferred words is null. (#618) [vertigo235]


## v0.2.0.288 (2017-02-05)

### **New Features**

- Added 'Case Insensitive.' to preferred tags info to help-inline. [Devin Buhl]

- Added more options to trakt, popular movies, upcoming, anticipated etc.. [Devin Buhl]

### **Fixes**

- Preferredcount -> preferredCount. [vertigo235]

- Delay Profile: Require preferred word to skip delay. [vertigo235]

- Delay Profile: Delay for at least 1 preferred word. [vertigo235]

- Delay Profile: Upgradable Check Fix. [vertigo235]

- Fix ical ics file (#603) [schumi2004]

- Fixed issue where quality weight was mapped wrongly. Fixes #597. [Leonardo Galli]

- Runtime error fix. [Leonardo Galli]

- Fix runtime issues. [Leonardo Galli]

- Fix glaringly obvious mistake that caused RSS Sync to fail. [Leonardo Galli]

- Add warning for docker users when switching branch. [Devin Buhl]

- Add plain "ES" audioProfile. (#569) [Chris Allen]

- Fix delay specification when delay is not set to zero. [Leonardo Galli]

- Use shorter format Profile string. (#561) [Chris Allen]

- Use movieFile instead of episodeFile. (#560) [Chris Allen]

- Add expanded DTS audio codecs to FileNameBuilder and fix up Atmos TrueHD audioCodec string. (#559) [Chris Allen]

- Don't display mapped movies in import list. [Tim Turner]

- Fix Delete modal when adding movie. [Tim Turner]

- Delete files now works. Fixes #127. [Leonardo Galli]

- First pass regarding delete. [Leonardo Galli]

- Fix error for movies with less than 4 characters. Fixes #507. [Leonardo Galli]


## v0.2.0.267 (2017-01-30)

### **New Features**

- Updates to ptp, and using caching for cookie. [Devin Buhl]

- Update the regex in Parser, Add workprint and telesync, change R5 to regional allow for R[0-9]{1}, changed the weights. [Devin Buhl]

- Update weights. [Devin Buhl]

- Added new qualities, added new qualities to profile class. Left to do: write migration, and tests. [Devin Buhl]

- Update Fetch List button style. [Tim Turner]

- Added options for watched, and watchlist, and customlist to trakt. [Devin Buhl]

- Update HDBits to work with Radarr. [Devin Buhl]

- Update taskscheduler when config is saved with netimportsynccommand. [Devin Buhl]

- Update Synology Indexer For Movies (#486) [vertigo235]

- Added option to specify preferred words in quality profile. (#462) [Leonardo Galli]

- Update Files tab when movie renamed. [Tim Turner]

- Update Rename Preview to support folder renaming. [Tim Turner]

- Added trakt user list importing. [Devin Buhl]

- Added easy to use List Selection for manual import use later. The place where this resides will change. [Leonardo Galli]

- Added Base URL. [Leonardo Galli]

- Added couchpotato, and added a test. [Devin Buhl]

- Updated HttpNetImporterBase. Still needs work to correctly handle failures. [Leonardo Galli]

- Added some abstraction for settings. [Leonardo Galli]

- Added Qualties to Settings. [Devin Buhl]

- Updates and compile-able. [Devin Buhl]

- Updates. [Devin Buhl]

### **Fixes**

- Add importfromlist abck. [Devin Buhl]

- Fix the filter modes on the movie list xD. [Devin Buhl]

- Fix issues with different languages than english when adding alternative titles. [Leonardo Galli]

- Use username, password and passkey for passthepopcorn. [Devin Buhl]

- Migration migraine-tion. [Devin Buhl]

- Ensure qualities don't overflow profile card. [Tim Turner]

- Migration. [Devin Buhl]

- Migration. [Devin Buhl]

- Make DVDR not unlimited. [Devin Buhl]

- Set Drone Factory Interval default to 0 (#515) [Tim Turner]

- Make year nullable for trakt. [Devin Buhl]

- Make year nullable, and rmember the profileid. [Devin Buhl]

- Proper port validation for download clients and connections. [Mark McDowall]

- Make NetImport sync interval work (needs some testing) [Devin Buhl]

- Allow Duplicate Preferred Words (#484) [vertigo235]

- Fix for movies without an imdbid. Fixes 176. [Leonardo Galli]

- Quality of an existing movie file can now be edited. [Leonardo Galli]

- Fix recognition of 4k Movies upon import. [Leonardo Galli]

- This should hopefully fix the error that decisions were not ordered correctly and therefore just the first release was grabbed. [Leonardo Galli]

- Remove confusing warning about file not being loaded. [Leonardo Galli]

- Add movie year to NotificationService (#496) [Tim Turner]

- Revert "Merge branch 'rename-existing-folder' into develop" [Tim Turner]

- Revert "Add movie year to NotificationService (#489)" [Tim Turner]

- Revert "Ensure the movie isn't delete when the folder isn't renamed (#491)" [Tim Turner]

- Ensure the movie isn't delete when the folder isn't renamed (#491) [Tim Turner]

- Add movie year to NotificationService (#489) [Tim Turner]

- Kodi Update Fix: OldFiles -> OldMovieFiles (#483) [vertigo235]

- More Notification Updates (#482) [vertigo235]

- Remove old folder and all contents. [Tim Turner]

- Movie reference properly updates UI now. [Tim Turner]

- Be more proper about Ensuring the folder exists. [Tim Turner]

- Undo unecessary changes. [Tim Turner]

- Move folder on rename; event doesn't fire yet. [Tim Turner]

- Net Import UI Updates. [Tim Turner]

- Only show "Display Existing Movies" toggle after selecting a folder. [Tim Turner]

- Clean up settings UI. [Tim Turner]

- Manual Import works now! [Leonardo Galli]

- Only wanted is default for CP. [Devin Buhl]

- Manual importing almost done. Needs fixing for mapping movies. [Leonardo Galli]

- Nullable all the fields.. [Devin Buhl]

- Rephrase wording. [Devin Buhl]

- Monitored to false for movies already downloaded on CP. [Devin Buhl]

- Allow null value for seed time. [Devin Buhl]

- Add basic ui of manual import. [Leonardo Galli]

- Fix importing for StevenLu. [Devin Buhl]

- Add StevenLu to csproj. [Devin Buhl]

- First pass at ui for manually importing from lists. [Leonardo Galli]

- Add import from http://movies.stevenlu.com/ [Devin Buhl]

- Movies can now be added monitored or unmonitored. [Leonardo Galli]

- Add Ability to set RootFolderPath for Net Import List. [Leonardo Galli]

- Fix netimport search and add NetImportSyncCommand. [Leonardo Galli]

- Remove duplicate code. [Devin Buhl]

- Fix movies being clobbered when a new list is sent thru. [Devin Buhl]

- Implement NetImportSearchService. [Devin Buhl]

- Add urlBase option to CP settings. [Devin Buhl]

- Fix media info parsing of multiple audio channels. Fixes #315 Fixes #294. [Leonardo Galli]

- Fixed styling. Fixed definitions not being returned. [Leonardo Galli]

- Rethought about where certain things are stored. [Leonardo Galli]

- Fix stuff regarding the ordering of Fields. [Leonardo Galli]

- Fix migration to include ConfigContract and EnableAuto. Also fixed redirects on lists. [Leonardo Galli]

- Second UI Pass, Testing now works and other little things. [Leonardo Galli]

- Fix up presets. [Leonardo Galli]

- Make presets work for RSS Import :) [Leonardo Galli]

- Add CP list class. [Leonardo Galli]

- Migration migrainetion. [Devin Buhl]

- Couchpotato API classes. [Devin Buhl]

- WIP UI Update for adding lists. [Leonardo Galli]

- Basis of UI Update. [Leonardo Galli]

- Add base for netimport api. Still nothing on the UI side. [Leonardo Galli]

- Imdbid parsing works now from url. [Leonardo Galli]

- Big Abstraction for IMDBWatchlist -> RSSImport (With a test) [Leonardo Galli]

- Whoops, only parse title once. [Devin Buhl]

- Few changes. [Devin Buhl]

- Initial autoimporter commit. [Devin Buhl]


## v0.2.0.238 (2017-01-26)

### **New Features**

- Update GeneralViewTemplate.hbs. [Jordan]

- Change lang in UI to what profile / lang they choose when they add a movie. [Devin Buhl]

- Update JoinProxy.cs. [hotio]

- Update Plex Movie Sections. [vertigo235]

- Update slack for movies. [vertigo235]

### **Fixes**

- Moviefile, what movie file? (#466) [vertigo235]

- Remove mofilefile id for now (#464) [vertigo235]

- Download Movie Quality & Formatting. [vertigo235]

- Custom Script Fix: Parse movie not episode. [vertigo235]

- Fixes issue #447 (Notification Icon for Join) [hotio]

- Only use internal for RSS Sync. [Devin Buhl]

- Include only internal for AHD. [Devin Buhl]

- Fix new rss-sync threshold. [schumi2004]


## v0.2.0.226 (2017-01-24)

### **New Features**

- Update README.md. [Leonardo Galli]

- Update to favicon section, according to (#416) [hotio]

- Update default sort order (#429) [Devin Buhl]

- Updated ico files. [hotio]

- Update UI logos. [hotio]

### **Fixes**

- "fixed" error message. [Devin Buhl]

- Add link to Activity -> History Tab (#408) [Tim Turner]

- Allow renaming of movies that don't have an "Edition" (#432) [Tim Turner]

- #292 - Allow longer threshold for RSS Sync (#428) [Devin Buhl]

- Add year to search (#425) [Devin Buhl]

- Initial Notification Updates and Support (#401) [vertigo235]

- Fixes an issue where movies with (year) at the beginning were recognized with a title of "(" [Leonardo Galli]

- Blind fix to support seperator in movie tags. [Leonardo Galli]

- Fix issue with certain audio streams. Should fix #404. [Leonardo Galli]

- Add {Tags} to renaming options. [Leonardo Galli]

- Fix when libgdiplus isn't present. [Leonardo Galli]

- Proper ico and favicon. [hotio]

- Fix issue where monitored movies were still downloaded. Fixes #326. [Leonardo Galli]


## v0.2.0.210 (2017-01-22)

### **New Features**

- Update localstorage key prefixes. [Tim Turner]

- Change Forms Auth Cookie. Fixes #285. [Leonardo Galli]

- Update README.md. [Leonardo Galli]

### **Fixes**

- Fixes issue when multiple audio channels are present. Fixes #315 Fixes #294. [Leonardo Galli]

- Fix duplicate key prefixing. [Tim Turner]

- Prefix localstorage keys with "Radarr" [Tim Turner]

- Optimized logo (#375) [hotio]

- Set update interval to 30 minutes if on nightly. [Leonardo Galli]

- Prefix Keys with "Radarr" [Tim Turner]

- Add more filter options to movie list. [Devin Buhl]

- Search selected button in wanted tab works. [Vlad Ilies]

- Fix #228 - Fix Drone Factory interval input not saving. [Tim Turner]

- Fix Corruped Media Cover Images. [Leonardo Galli]


## v0.2.0.196 (2017-01-20)

### **New Features**

- Update MovieModule. [Leonardo Galli]

- Update ISSUE_TEMPLATE.md. [Leonardo Galli]

- Update sizing information in settings tab. [Leonardo Galli]

### **Fixes**

- Should fix 4K releases not getting parsed. [Leonardo Galli]

- Adds 'Movie Title, The' filename option (#359) [Krystian Charubin]

- Fix issue when movie file is null. [Leonardo Galli]

- Should fix upgrading of existing movie files. [Leonardo Galli]

- Add tests for 4K quality. [Leonardo Galli]

- Hopefully a fix for corrupt media covers. [Leonardo Galli]

- Fixed blacklist being ignored by download decision maker. [Leonardo Galli]

- Add helptext to nzbget "add paused" settings. (#363) [vertigo235]

- Add year to quick search results. [Devin Buhl]

- Fix issue with reimporting on movie fresh (#357) [Tim Turner]

- Fix MediaCoversUpdatedEvent broadcast. [Tim Turner]

- Bug fix for 15 movie wanted tab (#348) [Vlad Ilies]

- Blacklisting works now. [Leonardo Galli]


## v0.2.0.182 (2017-01-18)

### **New Features**

- Update height of posters to accomodate additional labels. [Tim Turner]

- Update SkyHookProxy.cs. [Leonardo Galli]

- Update Test Files for AddPaused to NZBGET. [vertigo235]

### **Fixes**

- Fix pushover priority values. [vertigo235]

- Hopefully fix issue when importing. [Leonardo Galli]

- Add download status to poster view. [Tim Turner]

- Add IMDb ID to file naming. [Devin Buhl]

- Fixed build. [Vlad Ilies]

- Basic implementation of the wanted tab (#31) [Vlad Ilies]

- Revert DownloadedMovieScanCommand to DownloadedEpisodesScanCommand. [Devin Buhl]

- Turn off scene mapping task #329, update TaskManager to use 'DownloadedMovieScanCommand' [Devin Buhl]

- Revert "Sonarr/sqlite updates" [Devin Buhl]

- Add "Add Paused" option for NZBGET downloader. [vertigo235]

- Upgraded System.Data.SQLite to 1.0.104.0. [Keivan Beigi]

- Revert "Upgraded System.Data.SQLite to 1.0.104.0" [Keivan Beigi]

- New: Upgraded SQLite binares for macOS. [Keivan]

- New: Upgraded SQLite binaries for Windows (3.16.0) [Keivan Beigi]

- Remove series references. [Leonardo Galli]

- Hopefully fix download ordering. [Leonardo Galli]

- Maybe this will solve the error. [Devin Buhl]


## v0.2.0.166 (2017-01-17)

### **New Features**

- Updated website and donation links. [Leonardo Galli]

- Change Scheduled Refresh Series to Refresh Movie. Fixes #301. [Leonardo Galli]

### **Fixes**

- Fix Issue when adding some movies. [Devin Buhl]

- Hopefully fix RSSSync. [Leonardo Galli]

- Fix publish date #239. [Devin Buhl]

- Fix: Issue #91 - "Search All Missing" wording. [Aenima99x]

- Add Support for changing file date to either cinema or physical release. [Leonardo Galli]

- Fix for movies with . in title when importing them. Fixes #268. [Leonardo Galli]

- Remove - as replacement for : [Leonardo Galli]

- Fix only one movie showing. Fix more button not showing up. [Leonardo Galli]

- Fix Audiochannels just being added together. [Leonardo Galli]

- Clean up rename preview & organize. [Tim Turner]

- Disambiguate Movie from Episode Renaming. [Tim Turner]


## v0.2.0.152 (2017-01-16)

### **New Features**

- Added movie studio to movie details page (#262) [Vlad Ilies]

- Update NewznabRequestGenerator.cs. [Leonardo Galli]

- Update README.md. [Leonardo Galli]

- Added trailer link to movie links (#255) (#282) [Vlad Ilies]

- Update README.md. [Leonardo Galli]

- Update .gitignore and remove Thumbs.db files (#276) [hotio]

- Update README.md (#271) [hotio]

- Update README.md. [hotio]

- Update README.md. [hotio]

### **Fixes**

- Fix for hardcoded subs regex. [Leonardo Galli]

- Add Calendar Tab back. Fixes #32. [Leonardo Galli]

- Removed duplicate PublishDate. [Devin Buhl]

- Add support section to README (#281) [hotio]

- First pass at hiding existing movies upon import. [Tim Turner]

- Reworked README (#280) [hotio]

- Move Travis builds to container-based infrastructure (#273) [Mitchell Cash]

- Adding only original title is now allowed. Fixes #272. [Leonardo Galli]

- Fix for special characters when searching with title in Newznab. Fixes #97. [Leonardo Galli]

- Add {Original Title} to FileNameBuilder. Fixes #103. [Leonardo Galli]

- Release Group should now be available for renamer to use. [Leonardo Galli]

- 95% done with hiding existing movies. [Tim Turner]

- Cleanup README. [Mitchell Cash]


## v0.2.0.134 (2017-01-14)

### **New Features**

- Update CompletedDownloadService.cs. [Devin Buhl]

- Added more checks when tracking downloads. It should work now, even if history was not present. [Leonardo Galli]

- Update uTorrent to be able to use it as download client. [Leonardo Galli]

- Update Torznab to work with movies. [Devin Buhl]

- Update movie monitor tooltip (#223) [vertigo235]

- Update readme.md. [Leonardo Galli]

### **Fixes**

- Add in theaters to 1st coumn in movie list. [Devin Buhl]

- Simply completed download service. [Devin Buhl]

- Fixed TitleSlug For Realz! [Devin Buhl]

- Torpotato username regression. [Devin Buhl]

- Fixed exception when Quality meets cutoff. [Leonardo Galli]

- Fix history items getting deleted because they do not have a series id. [Leonardo Galli]

- If this does not fix stuff with no history, I have no clue anymore. [Leonardo Galli]

- Use MediaInfo to correctly identify quality when scanning disk as some file names may not contain the real quality. [Leonardo Galli]

- Should help identify problem with queue trying to reimport stuff. [Leonardo Galli]

- Should fix issue when history fails to capture a download item. [Leonardo Galli]

- Use DOGnzb name as the default rather than the URL (#250) [Mitchell Cash]

- Disable migration 117, takes too long to complete. [Leonardo Galli]

- Remove file count, unecessary after the file info tab was added. [Leonardo Galli]

- Fix epic fail on migration 117. [Leonardo Galli]

- Parsing of SABnzbd develop version. [Mark McDowall]

- Add rss sync to awesome-hd. [Devin Buhl]

- Files tab is now present. (#245) [Leonardo Galli]

- Revert "Fix movie title slugs" [Devin Buhl]

- Revert TMDBResources. [Devin Buhl]

- Fix Movie Title Slugs #233. [Devin Buhl]

- This conditional makes more sense. [Devin Buhl]

- #236 #239 - Fixed user being needed, fixed age on torrentpotato. [Devin Buhl]

- Add Missing Filter (#237) [William Comartin]

- Finally fix for sorting title (hopefully) [Devin Buhl]

- Clean up QBitTorrent. [Devin Buhl]

- Clean up rTorrent. [Devin Buhl]

- Clean up Transmission. [Devin Buhl]

- Clean up Deluge Settings. [Devin Buhl]

- Initial awesomeHD support. [Devin Buhl]

- Queue Service should now work properly again. [Leonardo Galli]

- DownloadMonitoringService should now not care about deleted movies. Fixes #131. [Leonardo Galli]

- Downloaded column should now use the correct quality name.                   Fixes #210. [Leonardo Galli]

- Stop incrementing version for pull requests. [Mike]

- Omgwtfnzbs: fixed parsing of GetInfoUrl and updated tests. [Tim Schindler]

- Improved categories, added Nzb-Tortuga as a preset. [Devin Buhl]


## v0.2.0.99 (2017-01-12)

### **New Features**

- Update UserAgentBuilder.cs. [Devin Buhl]

- Update Parser to support large array of Extended, Director, Collectors, ... Cut, Edition, etc. [Leonardo Galli]

- Change Sonarr to Radarr in CLA.md and CONTRIBUTING.md. [William Comartin]

- Change Sonarr to Radarr in Help Text, and in Notification Text Change sonarr log files to radarr log files. [William Comartin]

- Update sortValue when selecting movie for manual import. [Tim Turner]

### **Fixes**

- Fixed sorting in movie list view. Also added new downloaded quality column. [Leonardo Galli]

- Should fix ordering of releases. Fixes #147 (hopefully) [Leonardo Galli]

- Should fix queueService failed while processing. [Leonardo Galli]

- Add UHD to default movie categories for newsnab providers. [Devin Buhl]

- Movies in list don't sort correctly #174. [Devin Buhl]

- Replace Sonarr With Radarr in UI Directory. [William Comartin]


## v0.2.0.85 (2017-01-11)

### **New Features**

- Update parser to recognize [] and year at the beginning. Fixes #155, fixes #137 and fixes #136. [Leonardo Galli]

- Update plex movie libraries instead of series. [Devin Buhl]

- Update readme.md. [Neil]

- Update readme.md. [Leonardo Galli]

### **Fixes**

- Now hidden files are ignored :). Fixes #166. [Leonardo Galli]

- Fix sorting of unkown release date. [Leonardo Galli]

- Sorting now working according to quality in release collection. Fixes #85. [Leonardo Galli]

- Correctly check if inCinemas date is present. Creates issue with sorting, but eh. Fixes 140. [Leonardo Galli]

- Problem with Avatar (2009) #168. [Devin Buhl]

- Clean up basic movie naming. [Tim Turner]

- Fix some spelling mistakes and update the newznab api 'imdbid' [Devin Buhl]

- Fixes Manual Import and DroneFactory. [Tim Turner]

- Manual Import works. [Tim Turner]

- Aarch64 docker container added to readme. [Neil]

- Removed indexer Fanzub - site shutdown. [Devin Buhl]

- #146 search imdbid for usenet indexers that support it. [Devin Buhl]

- Get rid of unnecessary AppVeyor builds. [Mike]

- Add category 2035 to Newznab providers for WEB-DL search support. #123. [Devin Buhl]

- Fix transmission. [Devin Buhl]


## v0.2.0.61 (2017-01-10)

### **New Features**

- Update SystemLayout.js. [lxh87]

- Update readme.md. [Leonardo Galli]

- Update Info page. [Tim Turner]

- Added MovieFileResource. This allows the UI to interact with movie files better. Downloaded Quality is now shown in the table. [Leonardo Galli]

### **Fixes**

- Fix Wombles for movies. [Devin Buhl]

- Clean up Feature Requests. [Tim Turner]

- Fix #108 - Links to IMDB not working when searching for movies. [Devin Buhl]

- Fix download rejections being ignored. [Leonardo Galli]

- Fixes #104 - Backup/update fail Access to the path "/tmp/nzbdrone_backup/config.xml" is denied. [Devin Buhl]

- Fixes #100 - When adding a movie, monitored toggle doesn't apply and always defaults to being monitored. [Devin Buhl]


## v0.2.0.45 (2017-01-10)

### **New Features**

- Updated legend with number of movies. [Leonardo Galli]

- Update legend for missing status colors. [Leonardo Galli]

- Update sample detection runtime minutes. Some trailers can be long. [Leonardo Galli]

### **Fixes**

- Fix issues with media managment config not getting saved. [Leonardo Galli]

- Movie Editor works now. Fixes #99. [Leonardo Galli]

- Fixes a few things with importing: Sample check is done even when file is already in movie folder. Fixed importing of movies with "DC". [Leonardo Galli]

- Fix queue specification. [Leonardo Galli]

- Movie search should now work, even when titles returned from the TMDB do not have a release date set. Fixes #27. [Leonardo Galli]

- History now correctly shows movie title. Fixes #92. [Leonardo Galli]

- Redownloading failed downloads works again. Fixes #89. [Leonardo Galli]

- Use correct Modal for editing movies in table view. Fixes #90. [Leonardo Galli]

- Replace Sonarr with Radarr in Test notification messages. [schumi2004]


## v0.2.0.32 (2017-01-09)

### **New Features**

- Update Parser to support 576p movies, fixes #67. [Leonardo Galli]

- Update rss sync and fix search for omgwtfnzbs indexer. [Tim Schindler]

- Added PassThePopcorn indexer (#64) [Devin Buhl]

- Update SkyHookProxy.cs. [Leonardo Galli]

- Update SonarrCloudRequestBuilder.cs. [Leonardo Galli]

### **Fixes**

- Fixes an issue with movies not being added with same title slug as existing movies. [Leonardo Galli]

- Fix some links under status. Needs further changing further down the line. [Leonardo Galli]

- Organize & Rename work. [Tim Turner]

- Fix for importing movie folders with the at the end. [Leonardo Galli]

- Remove some indexers and fixed HDBits (#79) [Devin Buhl]

- Fixes Parser to match ImdbId as well as (year). [Leonardo Galli]

- Fixes movies not being able to be searched for. [Leonardo Galli]

- Taking another pass at organization/renaming. [Tim Turner]

- Unable to properly parse many movie titles. [Tim Turner]

- Second Pass at rename/organize. [Tim Turner]

- Display UI for MovieEditor, remove reference to SeasonPass. [Tim Turner]


