// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and Fluent Framework Contributors.
// All Rights Reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fluent.Hosting.UnitTests;

public sealed class ScopedHostedServiceTests
{
    [Fact]
    public void AddScopedHostedServiceRegistersHostedServiceAsSingleton()
    {
        ServiceCollection services = [];

        services.AddScopedHostedService(_ => Task.CompletedTask);

        ServiceDescriptor descriptor = services.Single(static descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
        );

        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public async Task AddScopedHostedServiceResolvesSameHostedServiceInstance()
    {
        ServiceCollection services = [];

        services.AddScopedHostedService(_ => Task.CompletedTask);

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        IHostedService first = serviceProvider.GetRequiredService<IHostedService>();
        IHostedService second = serviceProvider.GetRequiredService<IHostedService>();

        second.Should().BeSameAs(first);
    }

    [Fact]
    public async Task StartAsyncRunsRegisteredScopedHostedServiceInScope()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Services.AddScoped<ScopedMarker>();

        IServiceProvider? capturedServices = null;
        ScopedMarker? resolvedMarker = null;

        builder.Services.AddScopedHostedService(services =>
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
    public async Task StartAsyncRunsAllSubsequentScopedHostedServiceRegistrationsWithDifferentActions()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        List<string> calls = [];

        builder.Services.AddScopedHostedService(_ =>
        {
            calls.Add("first");

            return Task.CompletedTask;
        });
        builder.Services.AddScopedHostedService(_ =>
        {
            calls.Add("second");

            return Task.CompletedTask;
        });

        using IHost host = builder.Build();

        await host.StartAsync();

        calls.Should().Equal("first", "second");
    }

    [Fact]
    public async Task StartAsyncWaitsForRegisteredScopedHostedServiceToComplete()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var completed = false;

        builder.Services.AddScopedHostedService(async _ =>
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
    public async Task StopAsyncDoesNotRunScopedHostedServiceAction()
    {
        await using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
        IServiceScopeFactory scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
        var runCount = 0;
        InlineAsyncScopedHostedService service = new(
            scopeFactory,
            _ =>
            {
                runCount++;

                return Task.CompletedTask;
            }
        );

        await service.StopAsync(CancellationToken.None);

        runCount.Should().Be(0);
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
