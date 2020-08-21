---
uid: Guides.Operations.Truncate
title: Truncate with Venflow
---

# Truncate Table with Venflow

Your `Database` class exposes `Table<T>` properties which expose truncate operations. In Venflow truncates are always automatically. For this example, we want to truncate the Blogs table.

```cs
await database.Blogs.TruncateAsync();
```
