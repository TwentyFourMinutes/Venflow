---
uid: Guides.Operations.Delete
title: Delete with Venflow
---

# Delete Data with Venflow

Your `Database` class exposes `Table<T>` properties which expose delete operations. In Venflow deletes are always automatically generated. For this example, we want to delete a blog with all of its posts at once.

```cs
var blog = await database.Blogs.QuerySingle(@"SELECT * FROM ""Blogs"" LIMIT 1").QueryAsync();

await database.Blogs.DeleteAsync(blog);
```

As an alternative, if you already have the primary key of the blog you want to delete you can create a new `Blog` instance.

```cs
await database.Blogs.DeleteAsync(new Blog { Id = 1 });
```