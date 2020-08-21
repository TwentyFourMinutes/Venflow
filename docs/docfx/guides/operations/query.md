---
uid: Guides.Operations.Query
title: Query with Venflow
---

# Query Data with Venflow

> [!WARNING] 
> Be carful while dealing with raw SQL and ensure that you never pass user modified SQL to any of the methods. Instead use parameterized  overloads or the `Interpolated` siblings.

## Query data without relations

Your `Database` class exposes `Table<T>` properties which expose query operations. In Venflow queries are based on hand-written SQL, however for very simple scenarios there are generators which do the job for you. In this case we query the first 10 blogs in the database.

```cs
await using var database = new BlogDatabase(); // You should register this in a Transient/Scoped your IOC Container.

// You can re-use this in different BlogDatabase instances through the database.Blogs.QueryAsync() method
// If you intend to reuse the query below you need to pass the QueryBatch method false for the disposeCommand,
// otherwise the underyling command will be disposed after the first use.
var query = database.Blogs.QueryBatch(10).Build(); 

var blogs = await query.QueryAsync(); // You can also inline this with the line above.
```

If you instead only wanted to query the first result, you can use the `QuerySingle` API.

```cs
var blogs = await query.Blogs.QuerySingle().Build().QueryAsync();
```

## Query data with relations

In this case we want to get the first 5 blogs with all of their posts. If you want to perform a join, the builder exposes the `JoinWith` and the `ThenWith` method to perform nested joins.

```cs
const string sql = 
@"SELECT * FROM 
(
	SELECT * FROM ""Blogs""
	LIMIT 5
) AS ""Blogs"" 
JOIN ""Posts"" ON ""Posts"".""BlogId"" = ""Blogs"".""Id""";

var query = await database.Blogs.QueryBatch(sql).JoinWith(x => x.Posts).Build().QueryAsync();
```

If you instead only wanted to query the first blog with all of its posts, you can again use the `QuerySingle` API.

```cs
var blogs = await query.Blogs.QuerySingle().JoinWith(x => x.Posts).Build().QueryAsync();
```

## Queries with parameters

Most of the times your query contains some sort of parameters. With Venflow you have two options, either by using the raw `NpgsqlParameter` class and the matching `QuerySingle`/`QueryBatch` overloads or the Interpolated SQL methods.

In this example, we try to query the first blog with the id `5` with all of its posts. 

```cs
FormattableString sql =
$@"SELECT * FROM 
(
	SELECT * FROM ""Blogs""
	WHERE ""Id"" = {5}
	LIMIT 1
) AS ""Blogs"" 
JOIN ""Posts"" ON ""Posts"".""BlogId"" = ""Blogs"".""Id""";

var blogs = await query.Blogs.QueryInterpolatedSingle(sql).JoinWith(x => x.Posts).Build().QueryAsync();
```

> [!NOTE] 
> Most of the methods in Venflow which accept raw SQL do have a sibling method called `*Interpolated*`.

## Queries which don't return entities

In Venflow you have the option to construct queries which don't necessarily return a row of a table, in this case you can use the `Custom<T>` method on your `Database` class. 

In the following example we want to return the amount of rows in the Blogs table.

```cs
public class CountReturn
{
    public int Count { get; set; }
}

await database.Custom<CountReturn>().QuerySingle(@"SELECT COUNT(*) FROM ""Blogs""").Build().QueryAsync();
```

> [!WARNING] 
> This API does not support any of the usual methods available on regular entities, such as change tracking or joins.