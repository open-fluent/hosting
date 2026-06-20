# Fluent.Hosting

[Created in Poland by Leszek Pomianowski](https://lepo.co/) and [open-source community](https://github.com/lepoco/fluent/graphs/contributors).  
Small fluent extensions for registering inline hosted services in .NET dependency injection.

[![NuGet](https://img.shields.io/nuget/v/Fluent.Hosting.svg)](https://www.nuget.org/packages/Fluent.Hosting) [![NuGet Downloads](https://img.shields.io/nuget/dt/Fluent.Hosting.svg)](https://www.nuget.org/packages/Fluent.Hosting) [![GitHub license](https://img.shields.io/github/license/lepoco/fluent)](https://github.com/lepoco/fluent/blob/main/LICENSE)

## Getting started

```powershell
dotnet add package Fluent.Hosting
```

<https://www.nuget.org/packages/Fluent.Hosting>

```csharp
using Fluent.Hosting;

builder.Services.AddHostedService<IMigrator>(async (migrator, token) =>
{
    await migrator.MigrateAsync(token);
});
```

The package registers delegates as `IHostedService` instances. Use it when a startup or background task is simple enough that a dedicated hosted service class would add noise.

## Choosing an extension

| Method | Provider used by the action | Runs as | Typical use |
|--------|-----------------------------|---------|-------------|
| `AddHostedService` | Application service provider | `IHostedService` | Startup work that should complete before the host finishes starting |
| `AddScopedHostedService` | New DI scope | `IHostedService` | Startup work that needs scoped services, such as database sessions |
| `AddBackgroundService` | Application service provider | `BackgroundService` | Long-running or fire-and-forget background work |
| `AddScopedBackgroundService` | New DI scope | `BackgroundService` | Background work that needs scoped services |

All registrations are singleton `IHostedService` registrations. Multiple calls add multiple hosted services; each delegate runs.

## Hosted services

Use `AddHostedService` when the delegate can use singleton services or the root service provider directly.

```csharp
builder.Services.AddHostedService(async (services, token) =>
{
    var cache = services.GetRequiredService<IProductCache>();

    await cache.WarmAsync(token);
});
```

You can also ask Fluent.Hosting to resolve the service for you:

```csharp
builder.Services.AddHostedService<IProductCache>(async (cache, token) =>
{
    await cache.WarmAsync(token);
});
```

`AddHostedService` waits for the delegate to complete during host startup.

## Scoped hosted services

Use `AddScopedHostedService` when the work needs scoped dependencies.

```csharp
builder.Services.AddScopedHostedService<IDocumentSession>(async (session, token) =>
{
    session.Store(new MyData());

    await session.SaveChangesAsync(token);
});
```

The service creates a scope for the delegate, resolves `IDocumentSession` from that scope, and disposes the scope after the delegate completes.

## Background services

Use `AddBackgroundService` when the delegate should run as a `BackgroundService`.

```csharp
builder.Services.AddBackgroundService<IQueueWorker>(async (worker, token) =>
{
    await worker.RunAsync(token);
});
```

You can also access the provider directly:

```csharp
builder.Services.AddBackgroundService(async (services, token) =>
{
    var worker = services.GetRequiredService<IQueueWorker>();

    await worker.RunAsync(token);
});
```

## Scoped background services

Use `AddScopedBackgroundService` when background work needs scoped dependencies.

```csharp
builder.Services.AddScopedBackgroundService<IDocumentSession>(async (session, token) =>
{
    var pending = await session.Query<PendingJob>().ToListAsync(token);

    foreach (PendingJob job in pending)
    {
        job.MarkStarted();
    }

    await session.SaveChangesAsync(token);
});
```

The scope remains alive until the async delegate completes.

## Cancellation tokens

Every extension has two delegate shapes:

```csharp
builder.Services.AddHostedService<IWorker>(worker => worker.RunAsync());

builder.Services.AddHostedService<IWorker>((worker, token) => worker.RunAsync(token));
```

Use the token-aware overload for I/O, database work, queues, or anything that should stop promptly during shutdown or cancelled startup.

## Multiple registrations

Each call registers a separate hosted service. They are started by the host in registration order.

```csharp
builder.Services.AddHostedService<IClockSetup>((setup, token) => setup.InitializeAsync(token));
builder.Services.AddScopedHostedService<IDocumentSession>((session, token) => session.SaveChangesAsync(token));
builder.Services.AddBackgroundService<IQueueWorker>((worker, token) => worker.RunAsync(token));
```

## License

Fluent.Hosting is free and open source software licensed under the **MIT License**. You can use it in private and commercial projects.  
Keep in mind that you must include a copy of the license in your project.
