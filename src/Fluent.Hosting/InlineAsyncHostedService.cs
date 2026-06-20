// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Hosting;

/// <summary>
/// Represents an <see cref="IHostedService" /> that runs an asynchronous delegate when started.
/// </summary>
/// <param name="services">The service provider passed to the asynchronous delegate.</param>
/// <param name="action">The asynchronous delegate to run when the service starts.</param>
public class InlineAsyncHostedService(IServiceProvider services, Func<IServiceProvider, Task> action)
    : IHostedService
{
    /// <inheritdoc />
    public virtual Task StartAsync(CancellationToken cancellationToken) => action(services);

    /// <inheritdoc />
    public virtual Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
