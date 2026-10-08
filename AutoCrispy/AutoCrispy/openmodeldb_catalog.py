"""Small, dependency-free OpenModelDB browser/downloader bridge for AutoCrispy.

Only metadata-compatible models backed by core Spandrel architecture families,
PyTorch PTH/SafeTensors resources, RGB channels, and 1x/4x scales are exposed.
Downloads are limited to direct HTTPS GitHub/Hugging Face URLs and are checked
against the catalog's declared byte size and SHA-256 before being kept.
"""

from __future__ import annotations

import argparse
import base64
import hashlib
import json
import os
import re
import sys
import time
from pathlib import Path
from typing import Any
from urllib.parse import quote, urlsplit
from urllib.request import Request, urlopen


CATALOG_URL = "https://openmodeldb.info/api/v1/models.json"
USER_AGENT = "AutoCrispy-OpenModelDB/1.0 (+https://github.com/berlint-hub/AutoCrispy)"
MAX_CATALOG_BYTES = 32 * 1024 * 1024
MAX_MODEL_BYTES = 2 * 1024 * 1024 * 1024
CHUNK_SIZE = 1024 * 1024
MODEL_ID_PATTERN = re.compile(r"^[A-Za-z0-9][A-Za-z0-9_-]{0,159}$")
SHA256_PATTERN = re.compile(r"^[0-9a-fA-F]{64}$")

# Core chaiNNer Spandrel image architectures currently represented in the
# OpenModelDB schema. This is a conservative metadata gate, not proof that a
# particular checkpoint can be loaded; every download is checked by AutoCrispy's
# installed Spandrel ModelLoader before it is moved into the shared models folder.
SUPPORTED_ARCHITECTURES = frozenset(
    {
        "atd",
        "compact",
        "craft",
        "dat",
        "dctlsa",
        "ditn",
        "drct",
        "esrgan",
        "grl",
        "hat",
        "omnisr",
        "rcan",
        "real-cugan",
        "realplksr",
        "realplksr-dysample",
        "rgt",
        "span",
        "swift-srgan",
        "swinir",
    }
)
SUPPORTED_FORMATS = frozenset({"pth", "safetensors"})


def _positive_int(value: Any) -> int | None:
    if isinstance(value, bool):
        return None
    try:
        result = int(value)
    except (TypeError, ValueError, OverflowError):
        return None
    if isinstance(value, float) and not value.is_integer():
        return None
    return result if result > 0 else None


def _is_direct_download_url(url: Any, model_format: str) -> bool:
    if not isinstance(url, str) or not url or len(url) > 4096:
        return False
    try:
        parsed = urlsplit(url)
    except ValueError:
        return False
    if (
        parsed.scheme.casefold() != "https"
        or not parsed.hostname
        or parsed.username is not None
        or parsed.password is not None
    ):
        return False
    try:
        if parsed.port not in (None, 443):
            return False
    except ValueError:
        return False

    host = parsed.hostname.casefold()
    path = parsed.path.casefold()
    extension_ok = path.endswith("." + model_format) or (
        model_format == "pth" and path.endswith(".pth.tar")
    )
    if not extension_ok:
        return False

    if host == "github.com":
        return "/releases/download/" in path or "/raw/" in path
    if host == "raw.githubusercontent.com":
        return True
    if host == "huggingface.co":
        return "/resolve/" in path
    return False


def _normalized_author(value: Any) -> str:
    if isinstance(value, str):
        return value.strip()
    if isinstance(value, list):
        return ", ".join(str(author).strip() for author in value if str(author).strip())
    return ""


def compatible_catalog_items(catalog: Any) -> list[dict[str, Any]]:
    """Return conservative, directly downloadable candidates from the catalog."""
    if not isinstance(catalog, dict):
        raise ValueError("OpenModelDB returned an unexpected catalog format.")

    items: list[dict[str, Any]] = []
    for model_id, model in catalog.items():
        if not isinstance(model_id, str) or not MODEL_ID_PATTERN.fullmatch(model_id):
            continue
        if not isinstance(model, dict):
            continue
        # AutoCrispy intentionally does not expose the separate PBRify/map-generation workflow.
        pbrify_metadata = " ".join(
            str(value)
            for value in (
                model_id,
                model.get("name", ""),
                model.get("description", ""),
                model.get("tags", ""),
            )
        )
        if "pbrify" in pbrify_metadata.casefold():
            continue

        model_resources = model.get("resources", [])
        if not isinstance(model_resources, list):
            continue
        architecture = str(model.get("architecture", "")).strip()
        scale = _positive_int(model.get("scale"))
        input_channels = _positive_int(model.get("inputChannels"))
        output_channels = _positive_int(model.get("outputChannels"))
        if (
            architecture.casefold() not in SUPPORTED_ARCHITECTURES
            or scale not in (1, 4)
            or input_channels != 3
            or output_channels != 3
        ):
            continue

        resources: list[dict[str, Any]] = []
        seen_formats: set[str] = set()
        for resource in model_resources:
            if not isinstance(resource, dict):
                continue
            model_format = str(resource.get("type", "")).casefold().strip()
            if (
                resource.get("platform") != "pytorch"
                or model_format not in SUPPORTED_FORMATS
                or model_format in seen_formats
            ):
                continue
            size_bytes = _positive_int(resource.get("size"))
            sha256 = str(resource.get("sha256", "")).strip()
            urls = resource.get("urls", [])
            if (
                size_bytes is None
                or size_bytes > MAX_MODEL_BYTES
                or not SHA256_PATTERN.fullmatch(sha256)
                or not isinstance(urls, list)
            ):
                continue
            direct_urls = [
                url.strip()
                for url in urls
                if _is_direct_download_url(url, model_format)
            ]
            if not direct_urls:
                continue
            resources.append(
                {
                    "format": model_format,
                    "sizeBytes": size_bytes,
                    "sha256": sha256.lower(),
                    "urls": direct_urls,
                }
            )
            seen_formats.add(model_format)

        if not resources:
            continue
        tags = model.get("tags", [])
        if not isinstance(tags, list):
            tags = []
        items.append(
            {
                "id": model_id,
                "name": str(model.get("name") or model_id),
                "author": _normalized_author(model.get("author")),
                "license": str(model.get("license") or ""),
                "architecture": architecture,
                "scale": scale,
                "description": str(model.get("description") or ""),
                "tags": ", ".join(str(tag) for tag in tags),
                "pageUrl": "https://openmodeldb.info/models/" + quote(model_id, safe="-_"),
                "resources": resources,
            }
        )

    items.sort(key=lambda item: (item["scale"], item["name"].casefold(), item["id"].casefold()))
    return items


def fetch_catalog(timeout: int = 30) -> dict[str, Any]:
    request = Request(
        CATALOG_URL,
        headers={"User-Agent": USER_AGENT, "Accept": "application/json"},
    )
    with urlopen(request, timeout=timeout) as response:
        final_url = urlsplit(response.geturl())
        if final_url.scheme.casefold() != "https":
            raise ValueError("OpenModelDB catalog redirected to a non-HTTPS URL.")
        payload = response.read(MAX_CATALOG_BYTES + 1)
    if len(payload) > MAX_CATALOG_BYTES:
        raise ValueError("OpenModelDB catalog exceeded the 32 MiB safety limit.")
    try:
        data = json.loads(payload.decode("utf-8"))
    except (UnicodeDecodeError, json.JSONDecodeError) as error:
        raise ValueError("OpenModelDB returned invalid JSON.") from error
    if not isinstance(data, dict):
        raise ValueError("OpenModelDB returned an unexpected catalog format.")
    return data


def _find_resource(
    catalog: dict[str, Any], model_id: str, model_format: str
) -> tuple[dict[str, Any], dict[str, Any]]:
    if not MODEL_ID_PATTERN.fullmatch(model_id):
        raise ValueError("Invalid OpenModelDB model ID.")
    if model_format not in SUPPORTED_FORMATS:
        raise ValueError("Unsupported model file format.")
    for item in compatible_catalog_items(catalog):
        if item["id"] != model_id:
            continue
        for resource in item["resources"]:
            if resource["format"] == model_format:
                return item, resource
    raise ValueError("This model/resource is no longer in AutoCrispy's compatible download list.")


def _download_resource(
    resource: dict[str, Any], destination: Path, timeout: int = 60
) -> None:
    expected_size = _positive_int(resource.get("sizeBytes"))
    expected_sha256 = str(resource.get("sha256", "")).casefold()
    model_format = str(resource.get("format", "")).casefold()
    urls = resource.get("urls", [])
    if (
        expected_size is None
        or expected_size > MAX_MODEL_BYTES
        or not SHA256_PATTERN.fullmatch(expected_sha256)
        or model_format not in SUPPORTED_FORMATS
        or not isinstance(urls, list)
    ):
        raise ValueError("OpenModelDB resource metadata is incomplete or unsafe.")

    destination.parent.mkdir(parents=True, exist_ok=True)
    if destination.exists():
        raise FileExistsError("The temporary download destination already exists.")

    failures: list[str] = []
    for url in urls:
        if not _is_direct_download_url(url, model_format):
            continue
        parsed = urlsplit(url)
        partial_path = destination.with_name(destination.name + ".part")
        partial_created = False
        digest = hashlib.sha256()
        received = 0
        last_report = 0.0
        try:
            request = Request(
                url,
                headers={"User-Agent": USER_AGENT, "Accept": "application/octet-stream"},
            )
            with urlopen(request, timeout=timeout) as response:
                final_url = urlsplit(response.geturl())
                if final_url.scheme.casefold() != "https":
                    raise ValueError("The download redirected to a non-HTTPS URL.")
                content_length = response.headers.get("Content-Length")
                if content_length:
                    advertised = _positive_int(content_length)
                    if advertised is not None and advertised != expected_size:
                        raise ValueError("The server's file size does not match OpenModelDB metadata.")
                with partial_path.open("xb") as output:
                    partial_created = True
                    while True:
                        block = response.read(CHUNK_SIZE)
                        if not block:
                            break
                        received += len(block)
                        if received > expected_size:
                            raise ValueError("The downloaded file is larger than its catalog size.")
                        output.write(block)
                        digest.update(block)
                        now = time.monotonic()
                        if now - last_report >= 0.4:
                            print(
                                f"OPENMODELDB_PROGRESS\t{received}\t{expected_size}\t{parsed.hostname}",
                                flush=True,
                            )
                            last_report = now
                    output.flush()
                    os.fsync(output.fileno())
            if received != expected_size:
                raise ValueError(
                    f"Download ended early ({received} of {expected_size} bytes)."
                )
            if digest.hexdigest().casefold() != expected_sha256:
                raise ValueError("SHA-256 verification failed for the downloaded model.")
            os.replace(partial_path, destination)
            partial_created = False
            print(f"OPENMODELDB_DOWNLOAD_COMPLETE\t{expected_sha256}", flush=True)
            return
        except Exception as error:
            failures.append(f"{parsed.hostname}: {error}")
        finally:
            if partial_created:
                try:
                    partial_path.unlink()
                except OSError:
                    pass

    details = " | ".join(failures) if failures else "No supported direct HTTPS URL was listed."
    raise RuntimeError("Could not download and verify this model. " + details)


def _encode_field(value: Any) -> str:
    text = str(value if value is not None else "")
    return base64.b64encode(text.encode("utf-8")).decode("ascii")


def _emit_list(items: list[dict[str, Any]]) -> None:
    for item in items:
        for resource in item["resources"]:
            fields = (
                item["id"],
                item["name"],
                item["author"],
                item["license"],
                item["architecture"],
                str(item["scale"]),
                item["description"],
                item["tags"],
                item["pageUrl"],
                resource["format"],
                str(resource["sizeBytes"]),
            )
            print("OPENMODELDB_RESOURCE\t" + "\t".join(_encode_field(field) for field in fields), flush=True)
    print(f"OPENMODELDB_COUNT\t{len(items)}", flush=True)


def _parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Browse and safely download compatible OpenModelDB models.")
    action = parser.add_mutually_exclusive_group(required=True)
    action.add_argument("--list", action="store_true", help="List compatible catalog entries as machine-readable lines")
    action.add_argument("--download", action="store_true", help="Download one model resource and verify its SHA-256")
    parser.add_argument("--id", default="", help="OpenModelDB model ID for --download")
    parser.add_argument("--format", default="", help="PTH or SafeTensors for --download")
    parser.add_argument("--destination", default="", help="Temporary destination filename for --download")
    return parser.parse_args()


def main() -> int:
    args = _parse_args()
    try:
        catalog = fetch_catalog()
        if args.list:
            _emit_list(compatible_catalog_items(catalog))
            return 0
        item, resource = _find_resource(catalog, args.id, args.format.casefold())
        if not args.destination:
            raise ValueError("A temporary destination path is required.")
        expected_suffix = "." + resource["format"]
        destination = Path(args.destination)
        if destination.suffix.casefold() != expected_suffix:
            raise ValueError("The temporary destination extension does not match the resource type.")
        _download_resource(resource, destination)
        return 0
    except KeyboardInterrupt:
        print("OpenModelDB download cancelled.", file=sys.stderr)
        return 130
    except Exception as error:
        print(f"OpenModelDB: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
