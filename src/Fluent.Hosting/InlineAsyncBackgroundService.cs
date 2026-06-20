// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Hosting;

/// <summary>
/// Represents a <see cref="BackgroundService" /> that runs an asynchronous delegate.
/// </summary>
/// <param name="services">The service provider passed to the asynchronous delegate.</param>
/// <param name="action">The asynchronous delegate to run in the background.</param>
public class InlineAsyncBackgroundService(IServiceProvider services, Func<IServiceProvider, Task> action)
    : BackgroundService
{
    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => action(services);
}
