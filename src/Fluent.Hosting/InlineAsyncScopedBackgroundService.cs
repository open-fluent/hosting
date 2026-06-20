// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Hosting;

/// <summary>
/// Represents a <see cref="BackgroundService" /> that creates a scope and runs an asynchronous delegate.
/// </summary>
/// <param name="scopeFactory">The factory used to create the scope passed to the asynchronous delegate.</param>
/// <param name="action">The asynchronous delegate to run in the background.</param>
public class InlineAsyncScopedBackgroundService(
    IServiceScopeFactory scopeFactory,
    Func<IServiceProvider, Task> action
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();

        await action(scope.ServiceProvider);
    }
}
