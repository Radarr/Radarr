# New UI Development

This document should provide an overview of current UI development, progress and blockers.

## Current Focus

Our current focus is creating a foundation for the UI, so that everything can be built upon it.

We are trialing the Sonarr V3 UI as our foundation. So far it has been working great and we already have a working build running.

## Performance Issues

You can download a database with 40k movies here: https://radarr.video/dev/radarr.db (Version where the next refresh movie scan is in a year. The refresh movie scan will lag the UI and other stuff. https://radarr.video/dev/radarr_no_scan.db). Just place it in your AppData Directory while Radarr is not running and make sure it's named radarr.db (https://github.com/Radarr/Radarr/wiki/AppData-Directory).
You will have to message me (@galli-leo) via Discord or Reddit for the username and password (just as a precaution).

## Tasks

The actual tasks that are not related to the foundation of the new UI are all listed here https://github.com/Radarr/Radarr/projects/4. They are sorted according to different priorities. Some issues are also issues that shouldn't need anything extra, just a correct implementation in the new UI.
