---
uid: Guides.Operations.Query
title: Query with Venflow
---

# Query Data with Venflow

> [!WARNING] 
> Be carful while dealing with raw SQL and ensure that you never pass user modified SQL to any of the methods. Instead use parameterized  overloads or the `Interpolated` siblings.

> [!WARNING] 
> The primary key always has to be the first column of a given table returned by an SQL Query.

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

Additionally if you do not intend to reuse the the command instance you can omit the `Build` method call and directly call `QueryAsync`.

Also, if you instead only wanted to query the first result, you can use the `QuerySingle` API.

```cs
var blogs = await database.Blogs.QuerySingle().QueryAsync();
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
var blogs = await database.Blogs.QuerySingle().JoinWith(x => x.Posts).Build().QueryAsync();
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

var blogs = await database.Blogs.QueryInterpolatedSingle(sql).JoinWith(x => x.Posts).Build().QueryAsync();
```

> [!NOTE] 
> Most of the methods in Venflow which accept raw SQL do have a sibling method called `*Interpolated*`.

## Query partial data

The beautiful thing about Venflow is that you can only query for partial data, which means that a query doesn't necessarily  has to return all column. There are only two thing you need to be aware of:

- The primary key always needs to be returned
- Assuming you have two tables, table A and table B, which you are joining together. Table A isn't allowed to contain a column (other than the primary key) to have the same name as the primary key of table B. Otherwise Venflow won't be able to know when to spilt the SQL result.

A simple example of that would be something like the following, where we just query the name and the primary key of all blogs.

```cs
const string sql = @"SELECT ""Id"", ""Name"" FROM ""Blogs""";

var blogs = await database.Blogs.QueryBatch(sql).Build().QueryAsync();
```

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