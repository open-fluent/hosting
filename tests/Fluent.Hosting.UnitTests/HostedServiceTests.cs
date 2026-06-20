// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fluent.Hosting.UnitTests;

public sealed class HostedServiceTests
{
    [Fact]
    public void AddHostedServiceRegistersHostedServiceAsSingleton()
    {
        ServiceCollection services = [];

        services.AddHostedService(_ => Task.CompletedTask);

        ServiceDescriptor descriptor = services.Single(static descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
        );

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public async Task AddHostedServiceResolvesSameHostedServiceInstance()
    {
        ServiceCollection services = [];

        services.AddHostedService(_ => Task.CompletedTask);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService first = serviceProvider.GetRequiredService<IHostedService>();
        IHostedService second = serviceProvider.GetRequiredService<IHostedService>();

        second.Should().BeSameAs(first);
    }

    [Fact]
    public async Task StartAsyncRunsRegisteredHostedService()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        ServiceMarker marker = new();

        builder.Services.AddSingleton(marker);

        IServiceProvider? capturedServices = null;
        ServiceMarker? resolvedMarker = null;
        var runCount = 0;

        builder.Services.AddHostedService(async services =>
        {
            await Task.Yield();

            capturedServices = services;
            resolvedMarker = services.GetRequiredService<ServiceMarker>();
            runCount++;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        runCount.Should().Be(1);
        capturedServices.Should().NotBeNull();
        resolvedMarker.Should().BeSameAs(marker);
    }

    [Fact]
    public async Task StartAsyncRunsAllSubsequentHostedServiceRegistrationsWithDifferentActions()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        List<string> calls = [];

        builder.Services.AddHostedService(_ =>
        {
            calls.Add("first");

            return Task.CompletedTask;
        });
        builder.Services.AddHostedService(_ =>
        {
            calls.Add("second");

            return Task.CompletedTask;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        calls.Should().Equal("first", "second");
    }

    [Fact]
    public async Task StartAsyncWaitsForRegisteredHostedServiceToComplete()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var completed = false;

        builder.Services.AddHostedService(async _ =>
        {
            started.SetResult();

            await release.Task;

            completed = true;
        });

        using IHost host = builder.Build();

        Task startTask = host.StartAsync();

        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));

        startTask.IsCompleted.Should().BeFalse();
        completed.Should().BeFalse();

        release.SetResult();

        await startTask.WaitAsync(TimeSpan.FromSeconds(5));

        completed.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsyncDoesNotRunHostedServiceAction()
    {
        await using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
        var runCount = 0;
        InlineAsyncHostedService service = new(
            services,
            _ =>
            {
                runCount++;

                return Task.CompletedTask;
            }
        );

        await service.StopAsync(CancellationToken.None);

        runCount.Should().Be(0);
    }

    private sealed class ServiceMarker;
}
