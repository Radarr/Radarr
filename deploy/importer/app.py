import json
import os
from typing import Any

import httpx
from fastapi import FastAPI, Request, UploadFile
from fastapi.responses import HTMLResponse, JSONResponse
from fastapi.templating import Jinja2Templates

RADARR_URL = os.environ["RADARR_URL"].rstrip("/")
API_KEY = os.environ["RADARR_API_KEY"]
QUALITY_PROFILE_ID = int(os.environ.get("QUALITY_PROFILE_ID", "1"))
ROOT_FOLDER_PATH = os.environ.get("ROOT_FOLDER_PATH", "/movies")
MIN_AVAILABILITY = os.environ.get("MIN_AVAILABILITY", "released")
SEARCH_ON_ADD = os.environ.get("SEARCH_ON_ADD", "true").lower() == "true"

HEADERS = {"X-Api-Key": API_KEY}

app = FastAPI(title="Radarr Bulk Importer")
templates = Jinja2Templates(directory="templates")


def normalize_entries(payload: Any) -> list[dict]:
    """Accept either a list of entries or {'movies': [...]}; coerce strings to {title: ...}."""
    if isinstance(payload, dict):
        for key in ("movies", "titles", "items"):
            if key in payload and isinstance(payload[key], list):
                payload = payload[key]
                break
        else:
            payload = [payload]
    if not isinstance(payload, list):
        return []
    out = []
    for item in payload:
        if isinstance(item, str):
            out.append({"title": item})
        elif isinstance(item, dict):
            out.append(item)
    return out


def pick_match(results: list[dict], year: int | None) -> dict | None:
    if not results:
        return None
    if year:
        for r in results:
            if r.get("year") == year:
                return r
    return results[0]


async def lookup(client: httpx.AsyncClient, term: str) -> list[dict]:
    r = await client.get(
        f"{RADARR_URL}/api/v3/movie/lookup",
        params={"term": term},
        headers=HEADERS,
        timeout=30.0,
    )
    r.raise_for_status()
    return r.json()


async def add_movie(client: httpx.AsyncClient, match: dict) -> tuple[int, dict]:
    payload = {
        "tmdbId": match["tmdbId"],
        "title": match["title"],
        "year": match.get("year"),
        "qualityProfileId": QUALITY_PROFILE_ID,
        "rootFolderPath": ROOT_FOLDER_PATH,
        "monitored": True,
        "minimumAvailability": MIN_AVAILABILITY,
        "addOptions": {"searchForMovie": SEARCH_ON_ADD},
    }
    r = await client.post(
        f"{RADARR_URL}/api/v3/movie",
        json=payload,
        headers=HEADERS,
        timeout=30.0,
    )
    body = {}
    try:
        body = r.json()
    except ValueError:
        body = {"raw": r.text}
    return r.status_code, body


async def process_entry(client: httpx.AsyncClient, entry: dict) -> dict:
    title = entry.get("title") or entry.get("name") or ""
    year = entry.get("year")
    tmdb_id = entry.get("tmdbId") or entry.get("tmdb_id")

    term = f"tmdb:{tmdb_id}" if tmdb_id else title
    if not term:
        return {"input": entry, "status": "error", "message": "no title or tmdbId"}

    try:
        results = await lookup(client, term)
    except httpx.HTTPError as e:
        return {"input": entry, "status": "error", "message": f"lookup failed: {e}"}

    match = pick_match(results, year)
    if not match:
        return {"input": entry, "status": "not_found", "message": "no match from Radarr lookup"}

    code, body = await add_movie(client, match)
    if code in (200, 201):
        return {
            "input": entry,
            "status": "added",
            "tmdbId": match["tmdbId"],
            "title": match["title"],
            "year": match.get("year"),
        }
    # Radarr returns 400 with a "MovieExistsValidator" error when already present.
    if code == 400 and isinstance(body, list) and any(
        "exist" in (e.get("errorMessage", "") + e.get("propertyName", "")).lower() for e in body
    ):
        return {
            "input": entry,
            "status": "exists",
            "tmdbId": match["tmdbId"],
            "title": match["title"],
            "year": match.get("year"),
        }
    return {
        "input": entry,
        "status": "error",
        "message": f"HTTP {code}: {body}",
        "tmdbId": match.get("tmdbId"),
        "title": match.get("title"),
    }


@app.get("/", response_class=HTMLResponse)
async def index(request: Request):
    return templates.TemplateResponse("index.html", {"request": request})


@app.get("/health")
async def health():
    async with httpx.AsyncClient() as client:
        try:
            r = await client.get(
                f"{RADARR_URL}/api/v3/system/status", headers=HEADERS, timeout=5.0
            )
            return {"importer": "ok", "radarr": r.status_code}
        except httpx.HTTPError as e:
            return JSONResponse({"importer": "ok", "radarr": f"unreachable: {e}"}, status_code=503)


@app.post("/import")
async def import_files(files: list[UploadFile]):
    all_entries: list[dict] = []
    parse_errors: list[dict] = []
    for f in files:
        raw = await f.read()
        try:
            payload = json.loads(raw)
        except json.JSONDecodeError as e:
            parse_errors.append({"file": f.filename, "error": str(e)})
            continue
        for entry in normalize_entries(payload):
            entry.setdefault("_source_file", f.filename)
            all_entries.append(entry)

    results = []
    async with httpx.AsyncClient() as client:
        for entry in all_entries:
            results.append(await process_entry(client, entry))

    summary = {"total": len(results)}
    for r in results:
        summary[r["status"]] = summary.get(r["status"], 0) + 1

    return {"summary": summary, "results": results, "parse_errors": parse_errors}
