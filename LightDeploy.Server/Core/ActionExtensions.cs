using Microsoft.AspNetCore.Components;

namespace LightDeploy.Server.Core;

public static class ActionExtensions
{
    /// <summary>
    /// Creates a debounced version of the given function <paramref name="action" />.
    /// Use this method when you want a function to be executed after a certain amount of time has passed since it was called the last time.
    /// </summary>
    /// <param name="action">The function to debounce.</param>
    /// <param name="delay">The debounce delay.</param>
    /// <returns>A debounced version of the function <paramref name="action" />.</returns>
    /// <remarks>
    /// When the returned action is invoked the specified callback <paramref name="action" /> will not be executed immediately.
    /// 
    /// Instead a timer is started that waits until the specified amount of time (<paramref name="delay" />) has passed.
    /// Once the timer has elapsed the callback <paramref name="action" /> is executed.
    /// 
    /// If the returned action is invoked again before the timer has elapsed all previous invocations to the returned action are ignored and the timer starts again.
    /// </remarks>
    public static Action Debounce(this Action action, TimeSpan delay)
    {
        CancellationTokenSource? cancellationTokenSource = null;

        return () =>
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new();
            Task
                .Delay(delay, cancellationTokenSource.Token)
                .ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        action();
                    }
                }, TaskScheduler.Default);
        };
    }
   
}