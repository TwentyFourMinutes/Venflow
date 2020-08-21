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
