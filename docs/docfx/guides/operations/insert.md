---
uid: Guides.Operations.Insert
title: Insert with Venflow
---

# Insert Data with Venflow

Your `Database` class exposes `Table<T>` properties which expose insert operations. In Venflow insert are always automatically generated. For this example, we want to insert a blog with a few posts at once.

```cs
var blog = new Blog
{
    Name = "My new Blog",
    Posts = new List<Post>
    {
        new Post 
        {
            Title = "This is my first Post",
            Content = "Hey there."
        },
        new Post 
        {
            Title = "This is my second Post",
            Content = "Hey there again!"
        }
    }
};

await database.Blogs.InsertAsync(blog);
```

## Insert Data without relations

One of the nice things with Venflow is, that you don't need to set all navigation properties to null, if you don't want to insert them. In this example we would have posts with a blog, but we only want to insert the blog.

```cs
var blog = new Blog
{
    Name = "My new Blog",
    Posts = new List<Post>
    {
        new Post 
        {
            Title = "This is my first Post",
            Content = "Hey there."
        },
        new Post 
        {
            Title = "This is my second Post",
            Content = "Hey there again!"
        }
    }
};

await database.Blogs.Insert().InsertAsync(blog);
```

> [!NOTE] 
> This builder exposes similar methods to the Query builder, you can use `InsertWith` and `AndWith` to further configure the insert.

> [!NOTE] 
> The method `InsertWithAll` will insert with all populated and reachable relations. This is the equivalent to calling `database.Blogs.InsertAsync()`.

