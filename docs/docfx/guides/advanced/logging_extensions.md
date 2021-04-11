---
uid: Guides.Advanced.Logging.Extensions
title: Logging with Venflow and Microsoft.Extensions.Logging
---

# Logging with Venflow and Microsoft.Extensions.Logging

Venflow has out of the box support for [`Microsoft.Extensions.Logging`](https://www.nuget.org/packages/Microsoft.Extensions.Logging), however it needs an add-in package called [`Venflow.Extensions.Logging`](https://www.nuget.org/packages/Venflow.Extensions.Logging) which can be downloaded through the NuGet Package Manager.

After installing you will be able to use the extensions method [`UseLoggerFactory`](xref:Venflow.Extensions.Logging.DatabaseOptionsBuilderExtensions(Venflow.DatabaseOptionsBuilder, Microsoft.Extensions.Logging.ILoggerFactory. System.Boolean)) on your [`DatabaseOptionsBuilder`](xref:Venflow.DatabaseOptionsBuilder) instance as shown in the example below. The Boolean parameter specifies whether or not to include sensitive information in the formatted log. 
```cs
public class BlogDatabase : Database
{
    // Omitted for brevity.
    
    private readonly ILoggerFactory _loggerFactory;
    
    public BlogDatabase(ILoggerFactory loggerFactory) : base("Your connection string.")
    {
        _loggerFactory = loggerFactory;
    }
    
    protected override void Configure(DatabaseOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLoggerFactory(_loggerFactory, true);
    }
}
```