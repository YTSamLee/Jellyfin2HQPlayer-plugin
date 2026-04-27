# Jellyfin2HQPlayer Plugin

Jellyfin2HQPlayer Plugin 用于建立：

Audio 文件路径 -> Jellyfin ItemId

映射关系，并提供 REST API 查询路由供外部应用调用。

---

# 功能

- Audio 文件 Path → Jellyfin ItemId 映射
- REST API 查询接口
- 内存索引
- 支持重建索引
- 支持 Jellyfin 标准插件仓库安装

---

# 支持的 Jellyfin 版本

- Jellyfin 10.11.x

---

# 安装方法

## 1. 打开 Jellyfin Dashboard

Dashboard -> Plugins

## 2. 点击右上角

Manage Repositories

## 3. 添加仓库

Repository Name：

Jellyfin2HQPlayer

Repository URL：

https://raw.githubusercontent.com/YTSamLee/Jellyfin2HQPlayer-plugin/main/manifest.json

## 4. 保存后刷新 Jellyfin 页面

## 5. 返回 Plugins 页面

在 Available 中找到：

Jellyfin2HQPlayer

## 6. 点击 Install

## 7. 重启 Jellyfin

---

# 插件设置页面

安装完成后：

Dashboard -> Plugins -> My Plugins -> Jellyfin2HQPlayer

可以看到插件设置页面。

页面会显示：

- 插件 READY 状态
- 已索引 Audio 文件数量
- Total Audio Count
- Last Updated 时间
- Rebuild 重建索引按钮

---

# API 路由

基础路径：

```text
/Plugins/Jellyfin2HQPlayer
```

---

## Path → ItemId 查询

```text
GET /Plugins/Jellyfin2HQPlayer/path2id?path={path}
```

示例：

```text
/Plugins/Jellyfin2HQPlayer/path2id?path=/music/test.flac
```

返回：

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

## 插件状态

```text
GET /Plugins/Jellyfin2HQPlayer/status
```

返回：

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

## 重建索引

```text
POST /Plugins/Jellyfin2HQPlayer/rebuild
```

返回：

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

Build 输出目录：

```text
Jellyfin.Plugin.Jellyfin2HQPlayer/bin/Release/net9.0/
```

---

# License

GPL-3.0
