# Radarr + Bulk JSON Importer (local deployment)

A docker-compose stack that runs Radarr plus a small FastAPI web UI for
bulk-adding movies from JSON files.

## Layout

```
deploy/
├── docker-compose.yml
├── .env.example          # copy to .env and fill in
└── importer/
    ├── Dockerfile
    ├── requirements.txt
    ├── app.py            # FastAPI service on :8080
    └── templates/
        └── index.html    # upload form
```

## First-time setup

1. `cp .env.example .env` and edit `MOVIES_PATH` / `DOWNLOADS_PATH` / `TZ`.
   Leave `RADARR_API_KEY` blank for now.
2. `docker compose up -d radarr`
3. Open <http://localhost:7878>, complete the setup wizard:
   - Set an admin password.
   - Settings → General → copy the **API Key**.
   - Settings → Indexers → add at least one (Prowlarr recommended).
   - Settings → Download Clients → add one.
   - Settings → Profiles → create/select a Quality Profile (e.g. Remux-2160p cutoff).
   - Settings → Media Management → confirm the root folder is `/movies`.
4. Put the API key into `.env` as `RADARR_API_KEY=...`.
5. Find your quality-profile ID:
   ```
   curl -H "X-Api-Key: $RADARR_API_KEY" http://localhost:7878/api/v3/qualityprofile
   ```
   Put the chosen `id` in `.env` as `QUALITY_PROFILE_ID=...`.
6. `docker compose up -d` (starts the importer on :8080).

## Using the importer

Open <http://localhost:8080>, drop one or more JSON files, click **Import**.

Accepted JSON shapes:

```json
["The Matrix", "Inception"]
```
```json
[{"title": "The Matrix", "year": 1999}, {"title": "Inception"}]
```
```json
{"movies": [{"tmdbId": 27205}, {"title": "The Dark Knight", "year": 2008}]}
```

For each entry the importer:
1. Calls `GET /api/v3/movie/lookup?term=...` on Radarr.
2. Picks the result matching `year` if provided, otherwise the first match.
3. `POST /api/v3/movie` with `addOptions.searchForMovie=true` so Radarr starts
   searching immediately.

Per-row results show `added`, `exists`, `not_found`, or `error`. Track the
actual download progress in Radarr's own UI (Activity → Queue).

## Notes

- The importer talks to Radarr at `http://radarr:7878` over the compose
  network, so the API key never leaves your host.
- `SEARCH_ON_ADD=false` in `.env` if you want to bulk-add without immediately
  hammering your indexers; you can run a search later from Radarr's UI.
- For "best quality + full content" set the Quality Profile cutoff to your
  top tier (e.g. Remux-2160p) and leave "Upgrades Allowed" on if you want
  Radarr to keep upgrading until cutoff is met.
- The `data/` directory created by compose holds Radarr's config DB and your
  media. Back it up.
