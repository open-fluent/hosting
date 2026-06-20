// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fluent.Hosting.UnitTests;

public sealed class ScopedBackgroundServiceTests
{
    [Fact]
    public void AddScopedBackgroundServiceRegistersHostedServiceAsSingleton()
    {
        ServiceCollection services = [];

        services.AddScopedBackgroundService(_ => Task.CompletedTask);

        ServiceDescriptor descriptor = services.Single(static descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
        );

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public async Task AddScopedBackgroundServiceResolvesSameHostedServiceInstance()
    {
        ServiceCollection services = [];

        services.AddScopedBackgroundService(_ => Task.CompletedTask);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService first = serviceProvider.GetRequiredService<IHostedService>();
        IHostedService second = serviceProvider.GetRequiredService<IHostedService>();

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void AddScopedBackgroundServiceThrowsWhenActionIsNull()
    {
        ServiceCollection services = [];
        Func<IServiceProvider, Task> action = null!;

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddScopedBackgroundService(action)
        );

        exception.ParamName.Should().Be(nameof(action));
    }

    [Fact]
    public void AddScopedBackgroundServiceThrowsWhenTokenAwareActionIsNull()
    {
        ServiceCollection services = [];
        Func<IServiceProvider, CancellationToken, Task> action = null!;

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddScopedBackgroundService(action)
        );

        exception.ParamName.Should().Be(nameof(action));
    }

    [Fact]
    public void AddScopedBackgroundServiceOfTThrowsWhenActionIsNull()
    {
        ServiceCollection services = [];
        Func<ScopedMarker, Task> action = null!;

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddScopedBackgroundService(action)
        );

        exception.ParamName.Should().Be(nameof(action));
    }

    [Fact]
    public void AddScopedBackgroundServiceOfTThrowsWhenTokenAwareActionIsNull()
    {
        ServiceCollection services = [];
        Func<ScopedMarker, CancellationToken, Task> action = null!;

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddScopedBackgroundService(action)
        );

        exception.ParamName.Should().Be(nameof(action));
    }

    [Fact]
    public async Task StartAsyncRunsRegisteredScopedBackgroundServiceInScope()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Services.AddScoped<ScopedMarker>();

        IServiceProvider? capturedServices = null;
        ScopedMarker? resolvedMarker = null;

        builder.Services.AddScopedBackgroundService(services =>
        {
            capturedServices = services;
            resolvedMarker = services.GetRequiredService<ScopedMarker>();

            return Task.CompletedTask;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        capturedServices.Should().NotBeNull();
        capturedServices.Should().NotBeSameAs(host.Services);
        resolvedMarker.Should().NotBeNull();
        resolvedMarker!.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsyncRunsRegisteredScopedBackgroundServiceWithResolvedService()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Services.AddScoped<ScopedMarker>();

        ScopedMarker? resolvedMarker = null;

        builder.Services.AddScopedBackgroundService<ScopedMarker>(service =>
        {
            resolvedMarker = service;

            return Task.CompletedTask;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        resolvedMarker.Should().NotBeNull();
        resolvedMarker!.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsyncPassesCancellationTokenToRegisteredScopedBackgroundService()
    {
        ServiceCollection services = [];
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken? capturedCancellationToken = null;

        services.AddScopedBackgroundService(
            (_, cancellationToken) =>
            {
                capturedCancellationToken = cancellationToken;

                return Task.CompletedTask;
            }
        );

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService service = serviceProvider.GetRequiredService<IHostedService>();

        await cancellationTokenSource.CancelAsync();
        await service.StartAsync(cancellationTokenSource.Token);

        capturedCancellationToken.Should().NotBeNull();
        capturedCancellationToken!.Value.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsyncPassesResolvedServiceAndCancellationTokenToRegisteredScopedBackgroundService()
    {
        ServiceCollection services = [];
        using CancellationTokenSource cancellationTokenSource = new();
        ScopedMarker? resolvedMarker = null;
        CancellationToken? capturedCancellationToken = null;

        services.AddScoped<ScopedMarker>();
        services.AddScopedBackgroundService<ScopedMarker>(
            (service, cancellationToken) =>
            {
                resolvedMarker = service;
                capturedCancellationToken = cancellationToken;

                return Task.CompletedTask;
            }
        );

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService service = serviceProvider.GetRequiredService<IHostedService>();

        await cancellationTokenSource.CancelAsync();
        await service.StartAsync(cancellationTokenSource.Token);

        resolvedMarker.Should().NotBeNull();
        resolvedMarker!.IsDisposed.Should().BeTrue();
        capturedCancellationToken.Should().NotBeNull();
        capturedCancellationToken!.Value.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsyncThrowsWhenResolvedScopedBackgroundServiceDependencyIsMissing()
    {
        ServiceCollection services = [];

        services.AddScopedBackgroundService<ScopedMarker>(_ => Task.CompletedTask);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService service = serviceProvider.GetRequiredService<IHostedService>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsyncRunsAllSubsequentScopedBackgroundServiceRegistrationsWithDifferentActions()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        List<string> calls = [];

        builder.Services.AddScopedBackgroundService(_ =>
        {
            calls.Add("first");

            return Task.CompletedTask;
        });
        builder.Services.AddScopedBackgroundService(_ =>
        {
            calls.Add("second");

            return Task.CompletedTask;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        calls.Should().Equal("first", "second");
    }

    [Fact]
    public async Task StartAsyncRunsAllSubsequentTypedScopedBackgroundServiceRegistrationsWithDifferentActions()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        List<string> calls = [];

        builder.Services.AddScoped<ScopedMarker>();
        builder.Services.AddScopedBackgroundService<ScopedMarker>(service =>
        {
            service.IsDisposed.Should().BeFalse();
            calls.Add("first");

            return Task.CompletedTask;
        });
        builder.Services.AddScopedBackgroundService<ScopedMarker>(service =>
        {
            service.IsDisposed.Should().BeFalse();
            calls.Add("second");

            return Task.CompletedTask;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        calls.Should().Equal("first", "second");
    }

    [Fact]
    public async Task StartAsyncPropagatesScopedBackgroundServiceExceptionAndDisposesScope()
    {
        ServiceCollection services = [];
        InvalidOperationException expectedException = new("Scoped background service failed.");
        ScopedMarker? resolvedMarker = null;

        services.AddScoped<ScopedMarker>();
        services.AddScopedBackgroundService<ScopedMarker>(service =>
        {
            resolvedMarker = service;

            throw expectedException;
        });

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService service = serviceProvider.GetRequiredService<IHostedService>();

        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartAsync(CancellationToken.None)
        );

        actualException.Should().BeSameAs(expectedException);
        resolvedMarker.Should().NotBeNull();
        resolvedMarker!.IsDisposed.Should().BeTrue();
    }

    private sealed class ScopedMarker : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
