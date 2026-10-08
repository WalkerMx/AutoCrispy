from __future__ import annotations

import contextlib
import hashlib
import io
import tempfile
import unittest
from pathlib import Path
from unittest import mock

import sys
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import openmodeldb_catalog as catalog


def _resource(
    *,
    model_format: str = "pth",
    url: str = "https://github.com/example/models/releases/download/v1/4x-good.pth",
    size: int = 123,
    digest: str | None = None,
    platform: str = "pytorch",
) -> dict[str, object]:
    return {
        "platform": platform,
        "type": model_format,
        "size": size,
        "sha256": digest or ("a" * 64),
        "urls": [url],
    }


def _model(
    *,
    architecture: str = "esrgan",
    scale: int = 4,
    input_channels: int = 3,
    output_channels: int = 3,
    resources: list[dict[str, object]] | None = None,
) -> dict[str, object]:
    return {
        "name": "Example",
        "author": ["tester", "second-author"],
        "license": "CC-BY-4.0",
        "tags": ["general-upscaler", "texture"],
        "description": "A test model.",
        "architecture": architecture,
        "scale": scale,
        "inputChannels": input_channels,
        "outputChannels": output_channels,
        "resources": resources if resources is not None else [_resource()],
    }


class CompatibleCatalogTests(unittest.TestCase):
    def test_catalog_filters_to_supported_metadata_formats_and_direct_sources(self) -> None:
        entries = {
            "4x-good": _model(),
            "1x-span-safe": _model(
                architecture="span",
                scale=1,
                resources=[
                    _resource(
                        model_format="safetensors",
                        url="https://huggingface.co/example/model/resolve/main/model.safetensors?download=true",
                    )
                ],
            ),
            "2x-not-supported": _model(scale=2),
            "4x-rgba": _model(output_channels=4),
            "4x-video": _model(architecture="sofvsr"),
            "4x-PBRify-map": _model(),
            "4x-map-tag": {**_model(), "tags": ["texture-generation", "PBRify"]},
            "4x-onnx": _model(resources=[_resource(model_format="onnx", platform="onnx")]),
            "4x-drive": _model(
                resources=[
                    _resource(url="https://drive.google.com/file/d/example/view")
                ]
            ),
            "4x-no-hash": _model(resources=[_resource(digest="not-a-sha256")]),
            "4x-no-size": _model(resources=[_resource(size=0)]),
            "4x-too-large": _model(resources=[_resource(size=catalog.MAX_MODEL_BYTES + 1)]),
            "4x-invalid-resources": {**_model(), "resources": {"not": "a list"}},
            "bad/id": _model(),
        }

        result = catalog.compatible_catalog_items(entries)

        self.assertEqual([item["id"] for item in result], ["1x-span-safe", "4x-good"])
        self.assertEqual(result[0]["resources"][0]["format"], "safetensors")
        self.assertEqual(result[1]["author"], "tester, second-author")
        self.assertEqual(result[1]["pageUrl"], "https://openmodeldb.info/models/4x-good")

    def test_supported_download_url_requires_https_direct_model_asset(self) -> None:
        accepted = [
            "https://github.com/org/repo/releases/download/v1/model.pth",
            "https://github.com/org/repo/raw/main/model.pth",
            "https://raw.githubusercontent.com/org/repo/main/model.safetensors",
            "https://huggingface.co/org/repo/resolve/main/model.safetensors?download=true",
        ]
        rejected = [
            ("http://github.com/org/repo/releases/download/v1/model.pth", "pth"),
            ("https://github.com/org/repo/tree/main/model.pth", "pth"),
            ("https://drive.google.com/file/d/model.pth/view", "pth"),
            ("https://huggingface.co/org/repo/blob/main/model.safetensors", "safetensors"),
            ("https://github.com/org/repo/releases/download/v1/model.safetensors", "pth"),
            ("https://user@github.com/org/repo/releases/download/v1/model.pth", "pth"),
            ("https://github.com:8443/org/repo/releases/download/v1/model.pth", "pth"),
        ]
        for url in accepted:
            with self.subTest(url=url):
                model_format = "safetensors" if "safetensors" in url else "pth"
                self.assertTrue(catalog._is_direct_download_url(url, model_format))
        for url, model_format in rejected:
            with self.subTest(url=url, model_format=model_format):
                self.assertFalse(catalog._is_direct_download_url(url, model_format))

    def test_download_writes_only_after_size_and_hash_verification(self) -> None:
        payload = b"verified fake model weights"
        digest = hashlib.sha256(payload).hexdigest()
        resource = {
            "format": "pth",
            "sizeBytes": len(payload),
            "sha256": digest,
            "urls": ["https://github.com/example/model/releases/download/v1/model.pth"],
        }

        class FakeResponse:
            headers = {"Content-Length": str(len(payload))}

            def __enter__(self):
                return self

            def __exit__(self, *_args):
                return None

            def geturl(self):
                return resource["urls"][0]

            def read(self, _size: int) -> bytes:
                nonlocal payload
                result, payload = payload, b""
                return result

        with tempfile.TemporaryDirectory() as temporary:
            destination = Path(temporary) / "verified.pth"
            with mock.patch.object(catalog, "urlopen", return_value=FakeResponse()):
                with contextlib.redirect_stdout(io.StringIO()):
                    catalog._download_resource(resource, destination)
            self.assertEqual(destination.read_bytes(), b"verified fake model weights")
            self.assertFalse(Path(str(destination) + ".part").exists())

    def test_download_rejects_checksum_mismatch_and_removes_partial(self) -> None:
        payload = b"not the expected model"
        resource = {
            "format": "pth",
            "sizeBytes": len(payload),
            "sha256": "0" * 64,
            "urls": ["https://github.com/example/model/releases/download/v1/model.pth"],
        }

        class FakeResponse:
            headers = {"Content-Length": str(len(payload))}

            def __enter__(self):
                return self

            def __exit__(self, *_args):
                return None

            def geturl(self):
                return resource["urls"][0]

            def read(self, _size: int) -> bytes:
                nonlocal payload
                result, payload = payload, b""
                return result

        with tempfile.TemporaryDirectory() as temporary:
            destination = Path(temporary) / "bad.pth"
            with mock.patch.object(catalog, "urlopen", return_value=FakeResponse()):
                with contextlib.redirect_stdout(io.StringIO()):
                    with self.assertRaisesRegex(RuntimeError, "SHA-256 verification failed"):
                        catalog._download_resource(resource, destination)
            self.assertFalse(destination.exists())
            self.assertFalse(Path(str(destination) + ".part").exists())

    def test_download_enforces_size_limit_and_preserves_preexisting_partial(self) -> None:
        oversized = {
            "format": "pth",
            "sizeBytes": catalog.MAX_MODEL_BYTES + 1,
            "sha256": "a" * 64,
            "urls": ["https://github.com/example/model/releases/download/v1/model.pth"],
        }
        with tempfile.TemporaryDirectory() as temporary:
            destination = Path(temporary) / "too-large.pth"
            with mock.patch.object(catalog, "urlopen") as open_url:
                with self.assertRaisesRegex(ValueError, "incomplete or unsafe"):
                    catalog._download_resource(oversized, destination)
                open_url.assert_not_called()

            destination = Path(temporary) / "existing.pth"
            partial_path = Path(str(destination) + ".part")
            partial_path.write_bytes(b"leave this file alone")
            resource_url = "https://github.com/example/model/releases/download/v1/model.pth"
            resource = {
                "format": "pth",
                "sizeBytes": 1,
                "sha256": hashlib.sha256(b"x").hexdigest(),
                "urls": [resource_url],
            }

            class FakeResponse:
                headers = {"Content-Length": "1"}
                remaining = b"x"

                def __enter__(self):
                    return self

                def __exit__(self, *_args):
                    return None

                def geturl(self):
                    return resource_url

                def read(self, _size: int) -> bytes:
                    result, self.remaining = self.remaining, b""
                    return result

            with mock.patch.object(catalog, "urlopen", return_value=FakeResponse()):
                with contextlib.redirect_stdout(io.StringIO()):
                    with self.assertRaisesRegex(RuntimeError, "Could not download and verify"):
                        catalog._download_resource(resource, destination)
            self.assertEqual(partial_path.read_bytes(), b"leave this file alone")


if __name__ == "__main__":
    unittest.main()
