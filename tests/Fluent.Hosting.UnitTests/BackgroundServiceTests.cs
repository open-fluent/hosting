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

    private sealed class ServiceMarker;
}
