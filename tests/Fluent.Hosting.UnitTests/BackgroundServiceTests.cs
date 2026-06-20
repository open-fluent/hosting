// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fluent.Hosting.UnitTests;

public sealed class BackgroundServiceTests
{
    [Fact]
    public void AddBackgroundServiceRegistersHostedServiceAsSingleton()
    {
        ServiceCollection services = [];

        services.AddBackgroundService(_ => Task.CompletedTask);

        ServiceDescriptor descriptor = services.Single(static descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
        );

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public async Task AddBackgroundServiceResolvesSameHostedServiceInstance()
    {
        ServiceCollection services = [];

        services.AddBackgroundService(_ => Task.CompletedTask);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService first = serviceProvider.GetRequiredService<IHostedService>();
        IHostedService second = serviceProvider.GetRequiredService<IHostedService>();

        second.Should().BeSameAs(first);
    }

    [Fact]
    public void AddBackgroundServiceThrowsWhenActionIsNull()
    {
        ServiceCollection services = [];
        Func<IServiceProvider, Task> action = null!;

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddBackgroundService(action)
        );

        exception.ParamName.Should().Be(nameof(action));
    }

    [Fact]
    public void AddBackgroundServiceThrowsWhenTokenAwareActionIsNull()
    {
        ServiceCollection services = [];
        Func<IServiceProvider, CancellationToken, Task> action = null!;

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddBackgroundService(action)
        );

        exception.ParamName.Should().Be(nameof(action));
    }

    [Fact]
    public void AddBackgroundServiceOfTThrowsWhenActionIsNull()
    {
        ServiceCollection services = [];
        Func<ServiceMarker, Task> action = null!;

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddBackgroundService(action)
        );

        exception.ParamName.Should().Be(nameof(action));
    }

    [Fact]
    public void AddBackgroundServiceOfTThrowsWhenTokenAwareActionIsNull()
    {
        ServiceCollection services = [];
        Func<ServiceMarker, CancellationToken, Task> action = null!;

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddBackgroundService(action)
        );

        exception.ParamName.Should().Be(nameof(action));
    }

    [Fact]
    public async Task StartAsyncRunsRegisteredBackgroundService()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        ServiceMarker marker = new();

        builder.Services.AddSingleton(marker);

        ServiceMarker? resolvedMarker = null;
        var runCount = 0;

        builder.Services.AddBackgroundService(services =>
        {
            resolvedMarker = services.GetRequiredService<ServiceMarker>();
            runCount++;

            return Task.CompletedTask;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        runCount.Should().Be(1);
        resolvedMarker.Should().BeSameAs(marker);
    }

    [Fact]
    public async Task StartAsyncRunsRegisteredBackgroundServiceWithResolvedService()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        ServiceMarker marker = new();

        builder.Services.AddSingleton(marker);

        ServiceMarker? resolvedMarker = null;
        var runCount = 0;

        builder.Services.AddBackgroundService<ServiceMarker>(service =>
        {
            resolvedMarker = service;
            runCount++;

            return Task.CompletedTask;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        runCount.Should().Be(1);
        resolvedMarker.Should().BeSameAs(marker);
    }

    [Fact]
    public async Task StartAsyncPassesCancellationTokenToRegisteredBackgroundService()
    {
        ServiceCollection services = [];
        using CancellationTokenSource cancellationTokenSource = new();
        CancellationToken? capturedCancellationToken = null;

        services.AddBackgroundService(
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
    public async Task StartAsyncPassesResolvedServiceAndCancellationTokenToRegisteredBackgroundService()
    {
        ServiceMarker marker = new();
        ServiceCollection services = [];
        using CancellationTokenSource cancellationTokenSource = new();
        ServiceMarker? resolvedMarker = null;
        CancellationToken? capturedCancellationToken = null;

        services.AddSingleton(marker);
        services.AddBackgroundService<ServiceMarker>(
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

        resolvedMarker.Should().BeSameAs(marker);
        capturedCancellationToken.Should().NotBeNull();
        capturedCancellationToken!.Value.IsCancellationRequested.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsyncThrowsWhenResolvedBackgroundServiceDependencyIsMissing()
    {
        ServiceCollection services = [];

        services.AddBackgroundService<ServiceMarker>(_ => Task.CompletedTask);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService service = serviceProvider.GetRequiredService<IHostedService>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartAsync(CancellationToken.None));
    }

    [Fact]
    public async Task StartAsyncRunsAllSubsequentBackgroundServiceRegistrationsWithDifferentActions()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        List<string> calls = [];

        builder.Services.AddBackgroundService(_ =>
        {
            calls.Add("first");

            return Task.CompletedTask;
        });
        builder.Services.AddBackgroundService(_ =>
        {
            calls.Add("second");

            return Task.CompletedTask;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        calls.Should().Equal("first", "second");
    }

    [Fact]
    public async Task StartAsyncRunsAllSubsequentTypedBackgroundServiceRegistrationsWithDifferentActions()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        ServiceMarker marker = new();
        List<string> calls = [];

        builder.Services.AddSingleton(marker);
        builder.Services.AddBackgroundService<ServiceMarker>(service =>
        {
            service.Should().BeSameAs(marker);
            calls.Add("first");

            return Task.CompletedTask;
        });
        builder.Services.AddBackgroundService<ServiceMarker>(service =>
        {
            service.Should().BeSameAs(marker);
            calls.Add("second");

            return Task.CompletedTask;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        calls.Should().Equal("first", "second");
    }

    [Fact]
    public async Task StartAsyncPropagatesBackgroundServiceException()
    {
        ServiceCollection services = [];
        InvalidOperationException expectedException = new("Background service failed.");

        services.AddBackgroundService(_ => throw expectedException);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService service = serviceProvider.GetRequiredService<IHostedService>();

        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StartAsync(CancellationToken.None)
        );

        actualException.Should().BeSameAs(expectedException);
    }

    private sealed class ServiceMarker;
}
