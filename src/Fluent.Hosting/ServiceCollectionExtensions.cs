// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

namespace Fluent.Hosting;

/// <summary>
/// Provides extension methods for registering hosted services.
/// </summary>
public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds an <see cref="IHostedService" /> that runs the specified asynchronous action when the host starts.
        /// </summary>
        /// <param name="action">The asynchronous action to run with the application's service provider.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddHostedService(Func<IServiceProvider, Task> action)
        {
            return services.AddSingleton<IHostedService>(sp => new InlineAsyncHostedService(sp, action));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that creates a scope and runs the specified asynchronous action when the host starts.
        /// </summary>
        /// <param name="action">The asynchronous action to run with a scoped service provider.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddScopedHostedService(Func<IServiceProvider, Task> action)
        {
            return services.AddSingleton<IHostedService>(sp => new InlineAsyncScopedHostedService(
                sp.GetRequiredService<IServiceScopeFactory>(),
                action
            ));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that runs the specified asynchronous action as a background service.
        /// </summary>
        /// <param name="action">The asynchronous action to run with the application's service provider.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddBackgroundService(Func<IServiceProvider, Task> action)
        {
            return services.AddSingleton<IHostedService>(sp => new InlineAsyncBackgroundService(sp, action));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that creates a scope and runs the specified asynchronous action as a background service.
        /// </summary>
        /// <param name="action">The asynchronous action to run with a scoped service provider.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddScopedBackgroundService(Func<IServiceProvider, Task> action)
        {
            return services.AddSingleton<IHostedService>(sp => new InlineAsyncScopedBackgroundService(
                sp.GetRequiredService<IServiceScopeFactory>(),
                action
            ));
        }
    }
}
