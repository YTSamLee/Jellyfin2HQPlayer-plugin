using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Jellyfin2HQPlayer.EntryPoint
{
    /// <summary>
    /// 监听 Jellyfin 媒体库变更事件（ItemAdded / ItemRemoved）
    /// 并通过防抖调度触发 PathIndex 重建
    /// </summary>
    public class LibraryItemEventLoggerService : IHostedService
    {
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger<LibraryItemEventLoggerService> _logger;
        private readonly Services.PathIndexService _pathIndexService;

        // 防抖锁 + token
        private readonly object _debounceLock = new();
        private CancellationTokenSource? _debounceCts;

        // 防抖延时（用于合并多次事件）
        private static readonly TimeSpan DebounceDelay = TimeSpan.FromSeconds(15);

        // 防止并发 rebuild
        private int _isRebuilding;

        public LibraryItemEventLoggerService(
            ILibraryManager libraryManager,
            ILogger<LibraryItemEventLoggerService> logger,
            Services.PathIndexService pathIndexService)
        {
            _libraryManager = libraryManager;
            _logger = logger;
            _pathIndexService = pathIndexService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // 注册事件监听
            _libraryManager.ItemAdded += OnItemAdded;
            _libraryManager.ItemRemoved += OnItemRemoved;

            _logger.LogInformation("[J2H]: Library item event logger started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // 取消事件监听
            _libraryManager.ItemAdded -= OnItemAdded;
            _libraryManager.ItemRemoved -= OnItemRemoved;

            // 清理防抖任务
            lock (_debounceLock)
            {
                _debounceCts?.Cancel();
                _debounceCts?.Dispose();
                _debounceCts = null;
            }

            _logger.LogInformation("[J2H]: Library item event logger stopped.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 任意类型 Item 被添加（Audio / Album / Artist / Folder 等）
        /// 不做类型过滤，统一进入防抖调度
        /// </summary>
        private void OnItemAdded(object? sender, ItemChangeEventArgs e)
        {
            var item = e.Item;
            var type = item?.GetType().Name ?? "null";
            var name = item?.Name ?? "";
            var path = item?.Path ?? "";

            _logger.LogInformation(
                "[J2H][ItemAdded] Type={Type} Name={Name} Path={Path}",
                type,
                name,
                path);

            ScheduleDebounce("ItemAdded:" + type, name, path);
        }

        /// <summary>
        /// 任意类型 Item 被删除
        /// </summary>
        private void OnItemRemoved(object? sender, ItemChangeEventArgs e)
        {
            var item = e.Item;
            var type = item?.GetType().Name ?? "null";
            var name = item?.Name ?? "";
            var path = item?.Path ?? "";

            _logger.LogInformation(
                "[J2H][ItemRemoved] Type={Type} Name={Name} Path={Path}",
                type,
                name,
                path);

            ScheduleDebounce("ItemRemoved:" + type, name, path);
        }

        /// <summary>
        /// 防抖调度：
        /// 1. 取消之前的任务
        /// 2. 延时执行 rebuild
        /// 3. 保证同一时间只有一个 rebuild 在运行
        /// </summary>
        private void ScheduleDebounce(string reason, string name, string path)
        {
            CancellationTokenSource cts;

            lock (_debounceLock)
            {
                // 有旧任务就取消
                if (_debounceCts != null)
                {
                    _debounceCts.Cancel();
                    _debounceCts.Dispose();

                    _logger.LogInformation(
                        "[J2H][DebounceReset] Reason={Reason} Name={Name} Path={Path}",
                        reason,
                        name,
                        path);
                }

                // 创建新的防抖任务
                _debounceCts = new CancellationTokenSource();
                cts = _debounceCts;

                _logger.LogInformation(
                    "[J2H][DebounceScheduled] DelaySeconds={DelaySeconds} Reason={Reason} Name={Name} Path={Path}",
                    DebounceDelay.TotalSeconds,
                    reason,
                    name,
                    path);
            }

            // 后台执行（不阻塞主线程）
            _ = Task.Run(async () =>
            {
                try
                {
                    // 等待防抖时间（期间若有新事件会被取消）
                    await Task.Delay(DebounceDelay, cts.Token).ConfigureAwait(false);

                    // 防止并发 rebuild
                    if (Interlocked.Exchange(ref _isRebuilding, 1) == 1)
                    {
                        _logger.LogInformation("[J2H]: rebuild already running, skip this round.");
                        return;
                    }

                    try
                    {
                        _logger.LogInformation(
                            "[J2H]: rebuilding path to id... Reason={Reason} Name={Name} Path={Path}",
                            reason,
                            name,
                            path);

                        await _pathIndexService.RebuildAsync().ConfigureAwait(false);

                        _logger.LogInformation(
                            "[J2H]: path to id rebuild completed. Reason={Reason}",
                            reason);
                    }
                    finally
                    {
                        // 释放 rebuild 锁
                        Interlocked.Exchange(ref _isRebuilding, 0);
                    }
                }
                catch (TaskCanceledException)
                {
                    // 正常情况：被新的事件打断
                }
            });
        }
    }
}
