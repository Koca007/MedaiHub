namespace MediaHub.Scanner.Pipeline;

public sealed class ScanControl
{
    private readonly object _sync = new();
    private TaskCompletionSource<bool> _resumeSignal = CompletedSignal();
    private bool _isPaused;

    public bool IsPaused
    {
        get
        {
            lock (_sync)
            {
                return _isPaused;
            }
        }
    }

    public void Pause()
    {
        lock (_sync)
        {
            if (_isPaused)
            {
                return;
            }

            _isPaused = true;
            _resumeSignal = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    public void Resume()
    {
        TaskCompletionSource<bool>? signal = null;
        lock (_sync)
        {
            if (!_isPaused)
            {
                return;
            }

            _isPaused = false;
            signal = _resumeSignal;
        }

        signal.TrySetResult(true);
    }

    public async ValueTask WaitIfPausedAsync(CancellationToken cancellationToken)
    {
        Task resumeTask;
        lock (_sync)
        {
            resumeTask = _resumeSignal.Task;
        }

        await resumeTask.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static TaskCompletionSource<bool> CompletedSignal()
    {
        var signal = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        signal.SetResult(true);
        return signal;
    }
}
