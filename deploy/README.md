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

## Quick start (Fedora)

One-time, install Docker + the compose plugin:
```
sudo dnf install -y docker docker-compose-plugin
sudo systemctl enable --now docker
sudo usermod -aG docker $USER       # then log out / back in
```

Then from this `deploy/` folder:
```
python3 run.py
```

That single command:
1. Creates `.env` from `.env.example` if missing.
2. Brings up Radarr (and the importer, once `RADARR_API_KEY` is set).
3. Opens the right URL in your browser.

On first run it opens Radarr at <http://localhost:7878>. Complete the wizard,
copy the API Key (Settings → General) into `.env` as `RADARR_API_KEY=...`,
then re-run `python3 run.py` to bring up the importer at <http://localhost:8080>.

Other commands:
```
python3 run.py drive   # pick which mounted drive (e.g. external USB) holds movies/downloads
python3 run.py down    # stop the stack
python3 run.py logs    # tail logs
```

### Using an external drive

Plug it in and let your file manager mount it (Fedora normally mounts under
`/run/media/$USER/<label>`), then:

```
python3 run.py drive
```

Pick the drive from the list. The launcher creates `<mount>/Radarr/movies`
and `<mount>/Radarr/downloads`, and rewrites `MOVIES_PATH` / `DOWNLOADS_PATH`
in `.env` to point at them. If the stack is already running, restart it:
```
python3 run.py down && python3 run.py
```

If the drive isn't writable by your user (common with NTFS/exFAT formatted
drives), the picker tells you and prints the fix. The cleanest setup is
ext4/btrfs/xfs owned by your user.

### Manual setup (if you prefer)

1. `cp .env.example .env` and edit `MOVIES_PATH` / `DOWNLOADS_PATH` / `TZ`.
2. `docker compose up -d radarr`
3. Configure Radarr at <http://localhost:7878> (admin password, indexer,
   download client, quality profile, root folder = `/movies`).
4. Copy the API key into `.env` as `RADARR_API_KEY=...`. Find your
   quality-profile ID via:
   ```
   curl -H "X-Api-Key: $RADARR_API_KEY" http://localhost:7878/api/v3/qualityprofile
   ```
5. `docker compose up -d` to start the importer on :8080.

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
