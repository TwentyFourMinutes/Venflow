---
uid: Guides.Operations.Execute
title: Execute with Venflow
---

# Execute SQL with Venflow

> [!WARNING] 
> Be carful while dealing with raw SQL and ensure that you never pass user modified SQL to any of the methods. Instead use parameterized  overloads or the `Interpolated` siblings.

Your `Database` class exposes `Execute`methods which allow for fully custom SQL. In this example we drop a table.

```cs
await database.ExecuteAsync(@"DROP TABLE ""Foo""");
```
