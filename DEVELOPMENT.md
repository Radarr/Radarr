# New UI Development

This document should provide an overview of current UI development, progress and blockers.

## Current Focus

Our current focus is creating a foundation for the UI, so that everything can be built upon it.

We are trialing the Sonarr V3 UI as our foundation. So far it has been working great and we already have a somewhat working build running.
However, the Sonarr V3 UI is still very slow, especially with 40k movies. Which is why, our current focus is to find out why the UI is slow and fix that.

## Performance Issues

You can download a database with 40k movies here: https://radarr.video/dev/radarr.db (Version where the next refresh movie scan is in a year. The refresh movie scan will lag the UI and other stuff. https://radarr.video/dev/radarr_no_scan.db). Just place it in your AppData Directory while Radarr is not running and make sure it's named radarr.db (https://github.com/Radarr/Radarr/wiki/AppData-Directory).
You will have to message me (@galli-leo) via Discord or Reddit for the username and password (just as a precaution).

Warning: The loading of the page may take long, ideally you do not want Radarr to run in debug mode and use a localhost client.

I have already tried my hand at finding some bottlenecks and have seen the following problems:

1. It seems like sorting is done for all movies on every change to the movies. In other words, even if nothing changes for a movie, we still sort all movies again. This creates slowdowns, mostly when updating the info for all movies or adding a lot of new ones.
2. For most views, we check whether the movies have changed, when we receive and update movie event in order to determine if we should e.g. add more letters to the sidebar. This also creates slowdowns, mostly when updating the info for all movies.
3. The searchbar is also pretty slow, probably because we do not filter it in anyway and do not have a limitation of the amount of rows we render. In other words, if you type "a", all movies with an "a" in the title will have a row rendered. Ideally we would virtualize this, but we could also do maybe some other optimizations.
4. The virtualization for the movie index page is not fast enough. It seems, that if you scroll up, the renderer cannot keep up and takes ~200-500ms to render the table. This is not the most pressing issue, but should still be fixed.
5. There seems to be a few javascript "spots" that cause slowdowns, but I have far too little experience to really find out what's going on there.


## Tasks

The actual tasks that are not related to the foundation of the new UI are all listed here https://github.com/Radarr/Radarr/projects/4. They are sorted according to different priorities. Some issues are also issues that shouldn't need anything extra, just a correct implementation in the new UI.