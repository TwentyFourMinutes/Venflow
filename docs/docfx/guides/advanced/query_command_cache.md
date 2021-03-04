---
uid: Guides.Advanced.CommandQueryCache
title: Caching Command Queries
---

#  Caching Command Queries

Do note, that this guide is not about Query Response Caching, but rather about caching the underlying command.

Venflow uses different methods to cache the underlying query result parser aka. query materializer. There are two layers to this caching mechanism, where the first one is by SQL query string and the second one by the actual properties of the query such as returned columns. There is not much for you to configure, however if you do wish you can configure the cache expiration time of the first layer. This is important, in order to prevent too much memory allocation by the SQL query strings.

In the example shown below, the cache expiration time is set to 10 minutes instead of the default of 5 minutes. 

```cs
VenflowConfiguration.SetDynamicCacheExpirationTime(TimeSpan.FromMinutes(10));
```

> [!NOTE] 
> Once a SQL query reached its expiration time, it will indeed be removed from the first layer cache, however once it will be executed again, it will NOT need to recompile the query materializer, since it calls the slower, but memory-lighter second layer cache instead.