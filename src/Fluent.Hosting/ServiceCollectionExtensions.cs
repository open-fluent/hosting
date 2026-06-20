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
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddHostedService((serviceProvider, _) => action(serviceProvider));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that runs the specified asynchronous action when the host starts.
        /// </summary>
        /// <param name="action">The asynchronous action to run with the application's service provider and start cancellation token.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddHostedService(Func<IServiceProvider, CancellationToken, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddSingleton<IHostedService>(sp => new InlineAsyncHostedService(sp, action));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that resolves a service and runs the specified asynchronous action when the host starts.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve from the application's service provider.</typeparam>
        /// <param name="action">The asynchronous action to run with the resolved service.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddHostedService<TService>(Func<TService, Task> action)
            where TService : notnull
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddHostedService<TService>((service, _) => action(service));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that resolves a service and runs the specified asynchronous action when the host starts.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve from the application's service provider.</typeparam>
        /// <param name="action">The asynchronous action to run with the resolved service and start cancellation token.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddHostedService<TService>(Func<TService, CancellationToken, Task> action)
            where TService : notnull
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddHostedService(
                (serviceProvider, cancellationToken) =>
                    action(serviceProvider.GetRequiredService<TService>(), cancellationToken)
            );
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that creates a scope and runs the specified asynchronous action when the host starts.
        /// </summary>
        /// <param name="action">The asynchronous action to run with a scoped service provider.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddScopedHostedService(Func<IServiceProvider, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddScopedHostedService((serviceProvider, _) => action(serviceProvider));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that creates a scope and runs the specified asynchronous action when the host starts.
        /// </summary>
        /// <param name="action">The asynchronous action to run with a scoped service provider and start cancellation token.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddScopedHostedService(
            Func<IServiceProvider, CancellationToken, Task> action
        )
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddSingleton<IHostedService>(sp => new InlineAsyncScopedHostedService(
                sp.GetRequiredService<IServiceScopeFactory>(),
                action
            ));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that creates a scope, resolves a service, and runs the specified asynchronous action when the host starts.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve from the scoped service provider.</typeparam>
        /// <param name="action">The asynchronous action to run with the resolved scoped service.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddScopedHostedService<TService>(Func<TService, Task> action)
            where TService : notnull
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddScopedHostedService<TService>((service, _) => action(service));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that creates a scope, resolves a service, and runs the specified asynchronous action when the host starts.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve from the scoped service provider.</typeparam>
        /// <param name="action">The asynchronous action to run with the resolved scoped service and start cancellation token.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddScopedHostedService<TService>(
            Func<TService, CancellationToken, Task> action
        )
            where TService : notnull
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddScopedHostedService(
                (serviceProvider, cancellationToken) =>
                    action(serviceProvider.GetRequiredService<TService>(), cancellationToken)
            );
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that runs the specified asynchronous action as a background service.
        /// </summary>
        /// <param name="action">The asynchronous action to run with the application's service provider.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddBackgroundService(Func<IServiceProvider, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddBackgroundService((serviceProvider, _) => action(serviceProvider));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that runs the specified asynchronous action as a background service.
        /// </summary>
        /// <param name="action">The asynchronous action to run with the application's service provider and stopping cancellation token.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddBackgroundService(Func<IServiceProvider, CancellationToken, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddSingleton<IHostedService>(sp => new InlineAsyncBackgroundService(sp, action));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that resolves a service and runs the specified asynchronous action as a background service.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve from the application's service provider.</typeparam>
        /// <param name="action">The asynchronous action to run with the resolved service.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddBackgroundService<TService>(Func<TService, Task> action)
            where TService : notnull
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddBackgroundService<TService>((service, _) => action(service));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that resolves a service and runs the specified asynchronous action as a background service.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve from the application's service provider.</typeparam>
        /// <param name="action">The asynchronous action to run with the resolved service and stopping cancellation token.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddBackgroundService<TService>(
            Func<TService, CancellationToken, Task> action
        )
            where TService : notnull
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddBackgroundService(
                (serviceProvider, cancellationToken) =>
                    action(serviceProvider.GetRequiredService<TService>(), cancellationToken)
            );
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that creates a scope and runs the specified asynchronous action as a background service.
        /// </summary>
        /// <param name="action">The asynchronous action to run with a scoped service provider.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddScopedBackgroundService(Func<IServiceProvider, Task> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddScopedBackgroundService((serviceProvider, _) => action(serviceProvider));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that creates a scope and runs the specified asynchronous action as a background service.
        /// </summary>
        /// <param name="action">The asynchronous action to run with a scoped service provider and stopping cancellation token.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddScopedBackgroundService(
            Func<IServiceProvider, CancellationToken, Task> action
        )
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddSingleton<IHostedService>(sp => new InlineAsyncScopedBackgroundService(
                sp.GetRequiredService<IServiceScopeFactory>(),
                action
            ));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that creates a scope, resolves a service, and runs the specified asynchronous action as a background service.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve from the scoped service provider.</typeparam>
        /// <param name="action">The asynchronous action to run with the resolved scoped service.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddScopedBackgroundService<TService>(Func<TService, Task> action)
            where TService : notnull
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddScopedBackgroundService<TService>((service, _) => action(service));
        }

        /// <summary>
        /// Adds an <see cref="IHostedService" /> that creates a scope, resolves a service, and runs the specified asynchronous action as a background service.
        /// </summary>
        /// <typeparam name="TService">The service type to resolve from the scoped service provider.</typeparam>
        /// <param name="action">The asynchronous action to run with the resolved scoped service and stopping cancellation token.</param>
        /// <returns>The same <see cref="IServiceCollection" /> instance so additional calls can be chained.</returns>
        public IServiceCollection AddScopedBackgroundService<TService>(
            Func<TService, CancellationToken, Task> action
        )
            where TService : notnull
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return services.AddScopedBackgroundService(
                (serviceProvider, cancellationToken) =>
                    action(serviceProvider.GetRequiredService<TService>(), cancellationToken)
            );
        }
    }
}
