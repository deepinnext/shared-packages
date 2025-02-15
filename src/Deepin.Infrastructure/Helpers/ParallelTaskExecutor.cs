using System.Collections.Concurrent;

namespace Deepin.Infrastructure.Helpers;

public class ParallelTaskExecutor : IDisposable
{
    private readonly int _maxDegreeOfParallelism;
    private readonly ConcurrentQueue<(Func<CancellationToken, Task> Task, TaskCompletionSource<bool> Tcs)> _taskQueue;
    private readonly SemaphoreSlim _semaphore;
    private readonly CancellationTokenSource _globalCts;
    private bool _isRunning;
    private bool _isDisposed;

    public ParallelTaskExecutor(int? maxDegreeOfParallelism = null)
    {
        _maxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism ?? Environment.ProcessorCount);
        _taskQueue = new ConcurrentQueue<(Func<CancellationToken, Task>, TaskCompletionSource<bool>)>();
        _semaphore = new SemaphoreSlim(_maxDegreeOfParallelism);
        _globalCts = new CancellationTokenSource();
        _isRunning = true;
    }

    public Task EnqueueAsync(Func<CancellationToken, Task> task, CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
            throw new InvalidOperationException("Executor has been stopped.");

        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ParallelTaskExecutor));

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _globalCts.Token);
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        linkedCts.Token.Register(() =>
        {
            tcs.TrySetCanceled();
            linkedCts.Dispose();
        }, useSynchronizationContext: false);

        _taskQueue.Enqueue((task, tcs));
        _ = ProcessTaskAsync(linkedCts.Token);
        return tcs.Task;
    }

    private async Task ProcessTaskAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        try
        {
            if (_taskQueue.TryDequeue(out var queueItem))
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await queueItem.Task(cancellationToken);
                        queueItem.Tcs.TrySetResult(true);
                    }
                }
                catch (OperationCanceledException)
                {
                    queueItem.Tcs.TrySetCanceled(cancellationToken);
                }
                catch (Exception ex)
                {
                    queueItem.Tcs.TrySetException(ex);
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task StopAsync(bool waitForRunningTasks = true)
    {
        if (!_isRunning) return;

        _isRunning = false;
        _globalCts.Cancel();

        if (waitForRunningTasks)
        {
            try
            {
                for (int i = 0; i < _maxDegreeOfParallelism; i++)
                {
                    await _semaphore.WaitAsync();
                }
            }
            finally
            {
                for (int i = 0; i < _maxDegreeOfParallelism; i++)
                {
                    _semaphore.Release();
                }
            }
        }

        while (_taskQueue.TryDequeue(out var queueItem))
        {
            queueItem.Tcs.TrySetCanceled();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            _globalCts.Cancel();
            _globalCts.Dispose();
            _semaphore.Dispose();
        }

        _isDisposed = true;
    }
}
public class ParallelTaskExecutor<TResult> : IDisposable
{
    private readonly int _maxDegreeOfParallelism;
    private readonly ConcurrentQueue<(Func<CancellationToken, Task<TResult>> Task, TaskCompletionSource<TResult> Tcs)> _taskQueue;
    private readonly SemaphoreSlim _semaphore;
    private readonly CancellationTokenSource _globalCts;
    private bool _isRunning;
    private bool _isDisposed;

    public ParallelTaskExecutor(int? maxDegreeOfParallelism = null)
    {
        _maxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism ?? Environment.ProcessorCount);
        _taskQueue = new ConcurrentQueue<(Func<CancellationToken, Task<TResult>>, TaskCompletionSource<TResult>)>();
        _semaphore = new SemaphoreSlim(_maxDegreeOfParallelism);
        _globalCts = new CancellationTokenSource();
        _isRunning = true;
    }

    public Task<TResult> EnqueueAsync(Func<CancellationToken, Task<TResult>> task, CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
            throw new InvalidOperationException("Executor has been stopped.");

        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ParallelTaskExecutor<TResult>));

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _globalCts.Token);
        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        linkedCts.Token.Register(() =>
        {
            tcs.TrySetCanceled();
            linkedCts.Dispose();
        }, useSynchronizationContext: false);

        _taskQueue.Enqueue((task, tcs));
        _ = ProcessTaskAsync(linkedCts.Token);
        return tcs.Task;
    }

    private async Task ProcessTaskAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        try
        {
            if (_taskQueue.TryDequeue(out var queueItem))
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        var result = await queueItem.Task(cancellationToken);
                        queueItem.Tcs.TrySetResult(result);
                    }
                }
                catch (OperationCanceledException)
                {
                    queueItem.Tcs.TrySetCanceled(cancellationToken);
                }
                catch (Exception ex)
                {
                    queueItem.Tcs.TrySetException(ex);
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task StopAsync(bool waitForRunningTasks = true)
    {
        if (!_isRunning) return;

        _isRunning = false;
        _globalCts.Cancel();

        if (waitForRunningTasks)
        {
            try
            {
                for (int i = 0; i < _maxDegreeOfParallelism; i++)
                {
                    await _semaphore.WaitAsync();
                }
            }
            finally
            {
                for (int i = 0; i < _maxDegreeOfParallelism; i++)
                {
                    _semaphore.Release();
                }
            }
        }

        while (_taskQueue.TryDequeue(out var queueItem))
        {
            queueItem.Tcs.TrySetCanceled();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            _globalCts.Cancel();
            _globalCts.Dispose();
            _semaphore.Dispose();
        }

        _isDisposed = true;
    }
}