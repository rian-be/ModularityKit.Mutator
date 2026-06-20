#!/usr/bin/env python3

from __future__ import annotations

import json
import os
import pathlib
import subprocess
import sys
from typing import Iterable


def env(name: str, default: str = "") -> str:
    return os.environ.get(name, default)


def require_env(name: str) -> str:
    value = env(name)
    if not value:
        print(f"Missing required environment variable: {name}", file=sys.stderr)
        raise SystemExit(1)
    return value


def as_bool(value: str) -> bool:
    return value.strip().lower() in {"1", "true", "yes", "on"}


def gh_json(args: list[str]) -> object:
    completed = subprocess.run(
        ["gh", *args],
        check=True,
        capture_output=True,
        text=True,
        env=os.environ.copy(),
    )
    return json.loads(completed.stdout)


def iter_assets(dist_dir: pathlib.Path, patterns: str) -> list[pathlib.Path]:
    assets: list[pathlib.Path] = []
    seen: set[pathlib.Path] = set()

    for raw_pattern in patterns.splitlines():
        pattern = raw_pattern.strip()
        if not pattern:
            continue

        for match in sorted(dist_dir.glob(pattern)):
            if match.is_file() and match not in seen:
                seen.add(match)
                assets.append(match)

    return assets


def find_release(repository: str, release_tag: str, find_draft: bool) -> dict[str, object] | None:
    releases = gh_json(["api", f"repos/{repository}/releases?per_page=100"])
    if not isinstance(releases, list):
        raise RuntimeError("Unexpected GitHub API response for releases")

    if release_tag:
        for release in releases:
            if isinstance(release, dict) and release.get("tag_name") == release_tag:
                return release
        return None

    if find_draft:
        for release in releases:
            if isinstance(release, dict) and release.get("draft"):
                return release
        return None

    return None


def upload_assets(repository: str, tag_name: str, assets: Iterable[pathlib.Path]) -> int:
    asset_args = [str(asset) for asset in assets]
    if not asset_args:
        print("No assets matched the configured patterns.", file=sys.stderr)
        return 1

    subprocess.run(
        [
            "gh",
            "release",
            "upload",
            tag_name,
            *asset_args,
            "--clobber",
            "--repo",
            repository,
        ],
        check=True,
        env=os.environ.copy(),
    )
    return 0


def main() -> int:
    repository = require_env("REPOSITORY")
    dist_dir = pathlib.Path(env("DIST_DIR", "dist"))
    asset_patterns = env("ASSET_PATTERNS", "*")
    release_tag = env("RELEASE_TAG")
    find_draft = as_bool(env("FIND_DRAFT", "false"))
    ensure_draft = as_bool(env("ENSURE_DRAFT", "false"))
    fail_message = env("FAIL_MESSAGE", "No matching release found.")

    if not dist_dir.is_dir():
        print(f"Distribution directory not found: {dist_dir}", file=sys.stderr)
        return 1

    release = find_release(repository, release_tag, find_draft)
    if release is None:
        if ensure_draft or release_tag or find_draft:
            print(fail_message, file=sys.stderr)
            return 1
        print("No release target configured.", file=sys.stderr)
        return 1

    tag_name = release.get("tag_name")
    if not isinstance(tag_name, str) or not tag_name:
        print("Resolved release has no tag name.", file=sys.stderr)
        return 1

    assets = iter_assets(dist_dir, asset_patterns)
    return upload_assets(repository, tag_name, assets)


if __name__ == "__main__":
    raise SystemExit(main())
