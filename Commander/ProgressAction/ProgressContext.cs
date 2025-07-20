using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Commander.ProgressAction;

static class ProgressContext
{
    public static bool IsRunning { get => isRunning || UacServer.IsRunning; }

    public static CopyProgress? CopyProgress
    {
        get => field;
        set
        {
            if (field != value)
            {
                if (value != null)
                    Events.SendCopyProgress(value);
                field = value;
            }
        }
    }

    public static CancellationToken Start(long totalSize, int count, bool move)
    {
        cts?.Cancel();
        startTime = DateTime.Now;
        cts = new CancellationTokenSource();
        CopyProgress = new CopyProgress(
            move,
            "",
            count,
            0,
            totalSize,
            0,
            0,
            0,
            0,
            true,
            0
        );
        return cts.Token;
    }

    public static void SetRunning(bool running = true) => isRunning = running;

    public static bool CanClose()
    {
        if (!IsRunning)
            return true;
        else
        {
            Events.SendMenuCommand("SHOW_PROGRESS");
            return false;
        }
    }

    public static void SetNewFileProgress(string name, long size, int index)
    {
        var currentSize = CopyProgress?.PreviousTotalBytes ?? 0;
        if (CopyProgress != null)
        {
            CopyProgress = CopyProgress with
            {
                Name = name,
                CurrentCount = index,
                TotalBytes = currentSize,
                CurrentMaxBytes = size,
                PreviousTotalBytes = currentSize + size,
                CurrentBytes = 0,
                Duration = (int)(DateTime.Now - startTime).TotalSeconds
            };
        }
    }

    public static void Stop()
    {
        if (CopyProgress != null)
            Events.SendCopyProgress(CopyProgress with
            {
                IsRunning = false,
                Duration = (int)(DateTime.Now - startTime).TotalSeconds
            });
        CopyProgress = null;
    }

    public static void Cancel()
    {
        cts?.Cancel();
        if (UacServer.IsRunning)
        {
            
        }
        CopyProgress = null;
    }

    public static void SetProgress(long totalSize, long size)
    {
        if (CopyProgress != null)
        {
            var newVal = CopyProgress with
            {
                CurrentBytes = size,
                Duration = (int)(DateTime.Now - startTime).TotalSeconds
            };

            if (size < totalSize)
                progressSubject.OnNext(newVal);
            else
                CopyProgress = newVal;
        }
    }

    static ProgressContext()
    {
        progressSubject = new();
        progressSubject
            .Sample(TimeSpan.FromMilliseconds(80))
            .Subscribe(value =>
            {
                if (value.CurrentCount == CopyProgress?.CurrentCount)
                    CopyProgress = value;
            });
    }

    static readonly Subject<CopyProgress> progressSubject;
    static CancellationTokenSource? cts;
    static DateTime startTime = DateTime.Now;
    static bool isRunning;
}

record CopyProgress(
    bool Move,
    string Name,
    int TotalCount,
    int CurrentCount,
    long TotalMaxBytes,
    long TotalBytes,
    long PreviousTotalBytes,
    long CurrentMaxBytes,
    long CurrentBytes,
    bool IsRunning,
    int Duration
);