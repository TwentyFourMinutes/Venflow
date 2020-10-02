---
uid: Guides.Advanced.Preparation
title: Statement Preparation with Venflow
---

# Statement Preparation with Venflow

You can also prepare Query statements with Venflow through the Query Builder. This would look something like the following.

```cs
var someId = 10;
var query = await database.Blogs.QueryInterpolatedBatch($@"SELECT * FROM ""Blogs"" WHERE ""Id"" = {someId}", false).Build().PrepareAsync(); // You need to store this stamenet in a field or similar and reuse it, every time you want to query through this prepared statment.

var blogs = await _database.Blogs.QueryAsync(query); // You can also inline this with the line above.
```

> [!NOTE] 
> Also do not forget to call `UnprepareAsync` or `DisposeAsync` on this command once you no longer need it.

> [!NOTE] 
> One handy feature that comes in for you, is that you can use this prepared command with any `Database` instance.