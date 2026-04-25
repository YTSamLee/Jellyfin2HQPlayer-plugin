using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Jellyfin.Data.Enums;

using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.Jellyfin2HQPlayer.Services
{
    public class PathIndexService
    {
        private readonly ILibraryManager _libraryManager;
        private readonly Dictionary<string, Guid> _map = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _lock = new();

        // 新增 LastUpdated 字段
        public DateTime LastUpdated { get; private set; }

        // 存储总的音频数量
        public int TotalAudioCount { get; private set; }

        public bool Ready { get; private set; }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _map.Count;
                }
            }
        }

        // 构造函数，传入 ILibraryManager
        public PathIndexService(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        // 异步重建 Path -> ItemId 映射
        public async Task RebuildAsync(CancellationToken cancellationToken = default)
        {
            var newMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var totalAudioCount = 0;

            await Task.Run(() =>
            {
                var items = _libraryManager.GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = [BaseItemKind.Audio],
                    Recursive = true
                });

                foreach (var item in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (item is not Audio audio)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(audio.Path))
                    {
                        continue;
                    }

                    totalAudioCount++;

                    var path = NormalizePath(audio.Path);
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    newMap[path] = audio.Id;
                }
            }, cancellationToken);

            lock (_lock)
            {
                _map.Clear();
                foreach (var kv in newMap)
                {
                    _map[kv.Key] = kv.Value;
                }

                TotalAudioCount = totalAudioCount;
                Ready = true;

                // 更新 LastUpdated 时间
                LastUpdated = DateTime.UtcNow;
            }
        }

        // 查询路径对应的 ItemId
        public string? Lookup(string? path)
        {
            var normalized = NormalizePath(path);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            lock (_lock)
            {
                return _map.TryGetValue(normalized, out var id)
                    ? id.ToString("N")
                    : null;
            }
        }

        // 规范化路径
        public static string NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var s = path.Trim();

            if (s.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring("file://".Length);
            }

            try
            {
                s = Uri.UnescapeDataString(s);
            }
            catch
            {
            }

            s = s.Replace('\\', '/');
            return s;
        }
    }
}
