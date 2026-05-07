#!/usr/bin/env python3
"""One-shot launcher for the Radarr + importer stack.

Usage:
    python3 run.py            # bring stack up and open browser
    python3 run.py down       # stop the stack
    python3 run.py logs       # tail logs

Fedora prereqs (one-time):
    sudo dnf install -y docker docker-compose-plugin
    sudo systemctl enable --now docker
    sudo usermod -aG docker $USER     # then log out / back in
"""
from __future__ import annotations

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


def main() -> None:
    cmd = sys.argv[1] if len(sys.argv) > 1 else "up"
    {"up": cmd_up, "down": cmd_down, "logs": cmd_logs}.get(cmd, cmd_up)()


if __name__ == "__main__":
    main()
