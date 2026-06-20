// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Hosting;

/// <summary>
/// Represents an <see cref="IHostedService" /> that creates a scope and runs an asynchronous delegate when started.
/// </summary>
/// <param name="scopeFactory">The factory used to create the scope passed to the asynchronous delegate.</param>
/// <param name="action">The asynchronous delegate to run when the service starts.</param>
public class InlineAsyncScopedHostedService(
    IServiceScopeFactory scopeFactory,
    Func<IServiceProvider, Task> action
) : IHostedService
{
    /// <inheritdoc />
    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope? scope = scopeFactory.CreateScope();

        await action(scope.ServiceProvider);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
