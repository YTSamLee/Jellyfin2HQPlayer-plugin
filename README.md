# Jellyfin2HQPlayer Plugin

Jellyfin2HQPlayer Plugin builds:

Audio file path -> Jellyfin ItemId

mapping for Jellyfin audio libraries and provides REST API query routes for external applications.

---

# Features

- Audio file Path → Jellyfin ItemId mapping
- REST API query routes
- In-memory index
- Rebuild index support
- Standard Jellyfin plugin repository installation

---

# Supported Jellyfin Version

- Jellyfin 10.11.x

---

# Installation

## 1. Open Jellyfin Dashboard

Dashboard -> Plugins

## 2. Click the top-right button

Manage Repositories

## 3. Add Repository

Repository Name:

Jellyfin2HQPlayer

Repository URL:

https://raw.githubusercontent.com/YTSamLee/Jellyfin2HQPlayer-plugin/main/manifest.json

## 4. Save and refresh the Jellyfin web page

## 5. Return to Plugins page

Find:

Jellyfin2HQPlayer

under Available plugins.

## 6. Click Install

## 7. Restart Jellyfin


---

# Plugin Settings Page

After installation:

Dashboard -> Plugins -> My Plugins -> Jellyfin2HQPlayer

The settings page shows:

- Plugin READY status
- Indexed audio file count
- Total Audio Count
- Last Updated time
- Rebuild index button

---

# API Routes

Base route:

```text
/Plugins/Jellyfin2HQPlayer
```

---

## Path → ItemId Query

```text
GET /Plugins/Jellyfin2HQPlayer/path2id?path={path}
```

Example:

```text
/Plugins/Jellyfin2HQPlayer/path2id?path=/music/test.flac
```

Response:

```json
{
  "ok": true,
  "ready": true,
  "path": "/music/test.flac",
  "normalizedPath": "/music/test.flac",
  "found": true,
  "id": "1234567890abcdef"
}
```

---

## Plugin Status

```text
GET /Plugins/Jellyfin2HQPlayer/status
```

Response:

```json
{
  "ok": true,
  "ready": true,
  "count": 1000,
  "totalAudioCount": 1000,
  "lastUpdated": "2026-04-25T10:00:00Z"
}
```

---

## Rebuild Index

```text
POST /Plugins/Jellyfin2HQPlayer/rebuild
```

Response:

```json
{
  "ok": true,
  "ready": true,
  "count": 1000,
  "totalAudioCount": 1000,
  "lastUpdated": "2026-04-25T10:00:00Z"
}
```

---

# GitHub

https://github.com/YTSamLee/Jellyfin2HQPlayer-plugin

---

# Build

```bash
dotnet build Jellyfin.Plugin.Jellyfin2HQPlayer -c Release
```

Build output directory:

```text
Jellyfin.Plugin.Jellyfin2HQPlayer/bin/Release/net9.0/
```

---

# License

GPL-3.0
