---
uid: Guides.Operations.Count
title: Count with Venflow
---

# Count Rows with Venflow

Your `Database` class exposes `Table<T>` properties which expose count operations. In Venflow counts are always automatically generated. For this example, we want to get the amount of rows in the Posts table.

```cs
await database.Posts.CountAsync();
```
