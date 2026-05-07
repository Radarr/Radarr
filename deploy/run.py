#!/usr/bin/env python3
"""One-shot launcher for the Radarr + importer stack.

Usage:
    python3 run.py            # bring stack up and open browser
    python3 run.py drive      # pick which mounted drive holds movies/downloads
    python3 run.py down       # stop the stack
    python3 run.py logs       # tail logs

Fedora prereqs (one-time):
    sudo dnf install -y docker docker-compose-plugin
    sudo systemctl enable --now docker
    sudo usermod -aG docker $USER     # then log out / back in
"""
from __future__ import annotations

import json
import os
import shutil
import socket
import subprocess
import sys
import time
import urllib.request
import webbrowser
from pathlib import Path

DEPLOY_DIR = Path(__file__).resolve().parent
ENV_FILE = DEPLOY_DIR / ".env"
ENV_EXAMPLE = DEPLOY_DIR / ".env.example"

RADARR_URL = "http://localhost:7878"
IMPORTER_URL = "http://localhost:8080"


def detect_compose() -> list[str]:
    """Return the compose command prefix, or exit with guidance."""
    if shutil.which("docker"):
        try:
            subprocess.run(
                ["docker", "compose", "version"],
                check=True, capture_output=True,
            )
            return ["docker", "compose"]
        except (subprocess.CalledProcessError, FileNotFoundError):
            pass
    if shutil.which("podman"):
        try:
            subprocess.run(
                ["podman", "compose", "version"],
                check=True, capture_output=True,
            )
            return ["podman", "compose"]
        except (subprocess.CalledProcessError, FileNotFoundError):
            pass
    sys.exit(
        "No compose runtime found.\n"
        "On Fedora:\n"
        "  sudo dnf install -y docker docker-compose-plugin\n"
        "  sudo systemctl enable --now docker\n"
        "  sudo usermod -aG docker $USER   # then log out / back in"
    )


def ensure_env() -> dict[str, str]:
    if not ENV_FILE.exists():
        if not ENV_EXAMPLE.exists():
            sys.exit(f"Missing {ENV_EXAMPLE}; cannot bootstrap .env")
        ENV_FILE.write_text(ENV_EXAMPLE.read_text())
        print(f"[+] Created {ENV_FILE} from template.")
    env: dict[str, str] = {}
    for line in ENV_FILE.read_text().splitlines():
        line = line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        k, v = line.split("=", 1)
        env[k.strip()] = v.strip()
    return env


def wait_for_http(url: str, timeout: int = 60) -> bool:
    deadline = time.time() + timeout
    host, port = ("localhost", int(url.rsplit(":", 1)[1].split("/")[0]))
    while time.time() < deadline:
        try:
            with socket.create_connection((host, port), timeout=2):
                pass
            try:
                urllib.request.urlopen(url, timeout=3)
                return True
            except urllib.error.HTTPError:
                return True
            except Exception:
                pass
        except OSError:
            pass
        time.sleep(1)
    return False


def cmd_up() -> None:
    compose = detect_compose()
    env = ensure_env()
    has_key = bool(env.get("RADARR_API_KEY"))

    services = ["radarr"] if not has_key else []
    print(f"[+] Starting stack via: {' '.join(compose)} up -d {' '.join(services)}".rstrip())
    subprocess.run(compose + ["up", "-d", *services], cwd=DEPLOY_DIR, check=True)

    print("[+] Waiting for Radarr on", RADARR_URL)
    if not wait_for_http(RADARR_URL, timeout=90):
        print("[!] Radarr didn't respond within 90s. Check `python3 run.py logs`.")
        return

    if not has_key:
        print()
        print("=" * 60)
        print("First-time setup:")
        print(f"  1. Browser opens to {RADARR_URL}")
        print("  2. Set admin password, add an indexer + download client.")
        print("  3. Settings -> General -> copy the API Key.")
        print(f"  4. Paste it into {ENV_FILE} as RADARR_API_KEY=...")
        print("  5. Re-run: python3 run.py")
        print("=" * 60)
        webbrowser.open(RADARR_URL)
        return

    print("[+] Waiting for importer on", IMPORTER_URL)
    if not wait_for_http(IMPORTER_URL, timeout=60):
        print("[!] Importer didn't respond within 60s. Check `python3 run.py logs`.")
        return

    print(f"[+] Opening {IMPORTER_URL}")
    webbrowser.open(IMPORTER_URL)


def cmd_down() -> None:
    compose = detect_compose()
    subprocess.run(compose + ["down"], cwd=DEPLOY_DIR, check=True)


def cmd_logs() -> None:
    compose = detect_compose()
    subprocess.run(compose + ["logs", "-f", "--tail=100"], cwd=DEPLOY_DIR)


# ---------- drive picker ----------

SYSTEM_MOUNTS = {"/", "/boot", "/boot/efi", "/home", "/var", "/tmp"}


def list_mounted_drives() -> list[dict]:
    """Return mounted block-device partitions usable for media storage.

    Uses `lsblk -J` so we get structured data. Each entry has:
      name, size, mountpoint, fstype, label, removable (bool)
    """
    if not shutil.which("lsblk"):
        sys.exit("`lsblk` not found; can't enumerate drives.")
    out = subprocess.run(
        ["lsblk", "-J", "-b", "-o", "NAME,SIZE,MOUNTPOINT,MOUNTPOINTS,TYPE,RM,LABEL,FSTYPE"],
        capture_output=True, text=True, check=True,
    ).stdout
    data = json.loads(out)

    drives: list[dict] = []

    def visit(node: dict, parent_removable: bool = False) -> None:
        rm = bool(node.get("rm")) or parent_removable
        mp = node.get("mountpoint")
        mps = node.get("mountpoints") or ([mp] if mp else [])
        mps = [m for m in mps if m]
        for m in mps:
            if m in SYSTEM_MOUNTS or m.startswith("/boot") or m.startswith("[SWAP]"):
                continue
            drives.append({
                "name": node.get("name", ""),
                "size_bytes": node.get("size") or 0,
                "mountpoint": m,
                "fstype": node.get("fstype") or "",
                "label": node.get("label") or "",
                "removable": rm,
            })
        for child in node.get("children") or []:
            visit(child, rm)

    for top in data.get("blockdevices", []):
        visit(top)

    seen = set()
    unique = []
    for d in drives:
        if d["mountpoint"] in seen:
            continue
        seen.add(d["mountpoint"])
        unique.append(d)
    return unique


def human_size(n: int) -> str:
    n = int(n or 0)
    for unit in ("B", "K", "M", "G", "T", "P"):
        if n < 1024:
            return f"{n:.1f}{unit}"
        n /= 1024
    return f"{n:.1f}E"


def write_env_paths(movies: str, downloads: str) -> None:
    """Update MOVIES_PATH / DOWNLOADS_PATH in .env, preserving everything else."""
    if not ENV_FILE.exists():
        ENV_FILE.write_text(ENV_EXAMPLE.read_text())
    lines = ENV_FILE.read_text().splitlines()
    updates = {"MOVIES_PATH": movies, "DOWNLOADS_PATH": downloads}
    seen = set()
    new_lines: list[str] = []
    for line in lines:
        stripped = line.strip()
        if stripped and not stripped.startswith("#") and "=" in stripped:
            key = stripped.split("=", 1)[0].strip()
            if key in updates:
                new_lines.append(f"{key}={updates[key]}")
                seen.add(key)
                continue
        new_lines.append(line)
    for k, v in updates.items():
        if k not in seen:
            new_lines.append(f"{k}={v}")
    ENV_FILE.write_text("\n".join(new_lines) + "\n")


def cmd_drive() -> None:
    drives = list_mounted_drives()
    if not drives:
        sys.exit(
            "No mounted non-system drives found.\n"
            "Plug in your external drive and make sure it's mounted (Files app or `udisksctl mount`)."
        )

    print("Mounted drives available for media storage:\n")
    print(f"  {'#':<3} {'Size':>8}  {'FS':<8} {'Removable':<10} {'Label':<16} Mountpoint")
    print(f"  {'-'*3} {'-'*8}  {'-'*8} {'-'*10} {'-'*16} {'-'*40}")
    for i, d in enumerate(drives, 1):
        print(
            f"  {i:<3} {human_size(d['size_bytes']):>8}  "
            f"{d['fstype']:<8} {('yes' if d['removable'] else 'no'):<10} "
            f"{(d['label'] or '-')[:16]:<16} {d['mountpoint']}"
        )
    print()
    choice = input(f"Pick a drive [1-{len(drives)}] (or 'q' to cancel): ").strip()
    if choice.lower() in ("q", ""):
        print("Cancelled.")
        return
    try:
        idx = int(choice) - 1
        chosen = drives[idx]
    except (ValueError, IndexError):
        sys.exit(f"Invalid choice: {choice!r}")

    base = Path(chosen["mountpoint"]) / "Radarr"
    movies = base / "movies"
    downloads = base / "downloads"

    if not os.access(chosen["mountpoint"], os.W_OK):
        print(f"[!] {chosen['mountpoint']} isn't writable by you ({os.environ.get('USER')}).")
        print("    Common fixes:")
        print(f"      sudo chown -R $USER:$USER {chosen['mountpoint']}")
        print("    or for an NTFS/exFAT drive, remount with uid=$(id -u),gid=$(id -g).")
        if input("Continue anyway? [y/N] ").strip().lower() != "y":
            return

    try:
        movies.mkdir(parents=True, exist_ok=True)
        downloads.mkdir(parents=True, exist_ok=True)
    except PermissionError as e:
        sys.exit(f"Could not create {base}: {e}")

    write_env_paths(str(movies), str(downloads))
    print(f"[+] Updated {ENV_FILE}:")
    print(f"      MOVIES_PATH={movies}")
    print(f"      DOWNLOADS_PATH={downloads}")
    print()
    print("Now run `python3 run.py down && python3 run.py` to apply the new mounts.")


def main() -> None:
    cmd = sys.argv[1] if len(sys.argv) > 1 else "up"
    {
        "up": cmd_up,
        "down": cmd_down,
        "logs": cmd_logs,
        "drive": cmd_drive,
    }.get(cmd, cmd_up)()


if __name__ == "__main__":
    main()
