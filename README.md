# Fluent.Hosting

[Created in Poland by Leszek Pomianowski](https://lepo.co/) and [open-source community](https://github.com/lepoco/fluent/graphs/contributors).  
Fluent API for hosted services in .NET dependency injection framework.

[![NuGet](https://img.shields.io/nuget/v/Fluent.Hosting.svg)](https://www.nuget.org/packages/Fluent.Hosting) [![NuGet Downloads](https://img.shields.io/nuget/dt/Fluent.Hosting.svg)](https://www.nuget.org/packages/Fluent.Hosting) [![GitHub license](https://img.shields.io/github/license/lepoco/fluent)](https://github.com/lepoco/fluent/blob/main/LICENSE)

## Getting started

```powershell
dotnet add package Fluent.Hosting
```

<https://www.nuget.org/packages/Fluent.Hosting>

```csharp
using Fluent.Hosting;

builder.Services.AddHostedService(async s => {
    var worker = s.GetRequiredService<Worker>();
    await worker.RunAsync();
})
```

## License

Fluent.Hosting is free and open source software licensed under the **MIT License**. You can use it in private and commercial projects.  
Keep in mind that you must include a copy of the license in your project.
