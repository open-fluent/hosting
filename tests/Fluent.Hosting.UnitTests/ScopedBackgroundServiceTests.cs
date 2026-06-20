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

    private sealed class ScopedMarker : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
