---
uid: Guides.Advanced.AspNetCore
title: Asp.Net Core with Venflow
---

# Asp.Net Core with Venflow

Venflow natively adds container support for Asp.Net Core through the `Venflow.AspNetCore` NuGet package. It allows to directly register a [`Database`](xref:Venflow.Database) to your `IServiceCollection` through the `AddDatabase` method.

> [!WARNING] 
> Although this is fairly straightforward, you should ensure that your [`Database`](xref:Venflow.Database) class has a public constructor with a [`DatabaseOptionsBuilder<T>`](xref:Venflow.DatabaseOptionsBuilder`1) as a parameter which is getting passed to the appropriate base constructor. Otherwise it will ignore any options you configured.
