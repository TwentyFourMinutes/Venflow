---
uid: Guides.Advanced.StrongKeys
title: Strongly-typed Ids with Venflow
---

# Strongly-typed Ids with Venflow

Usually when interacting with any SQL database ids are quite common, especially when you have to write the SQL on your own. One might now ask themselves, why there even would be a need for strongly-typed ids, lets look at the example below.

```cs
var blogId = 10;
var postId = 14;

var post = GetPostByBlog(postId, blogId);

public Post GetPostByBlog(int blogId, int postId)
{
    // Omitted for brevity 
}
```

This code will compile just fine, however as you might have spotted, the `postId` and `blogId` are in the wrong order and therefore might not give us the expected result. This can be a real nightmare to fix as it is rather hard to spot in a large codebase with a lot of different entities. Luckily Venflow directly provides you with a build in solution called strongly-typed ids, through the [`Key<T,TKey>`](xref:Venflow.Key`2) API. Lets look at a full implementation.

```cs
public class Blog
{
    public Key<Blog, int> Id { get; set; } // Using Key instead of int
    public string Name { get; set; }
    
    public IList<Post> Posts { get; }
    
    public Blog()
    {
        Posts = new List<Post>();
    }
}

public class Post
{
    public Key<Post, int> Id { get; set; } // Using Key instead of int
    public string Title { get; set; }
    public string Content { get; set; }
    
    public Key<Blog, int> BlogId { get; set; } // Using Key instead of int
    public Blog Blog { get; set; }
}
```

Not only do primary-keys benefit from this, but also all the columns/properties which take any kind of id, such as foreign-keys. Not only do we now have type-safety, but also close to zero effort, since equality operators as well as implicit cast operators are predefined. Lets take the example from before and see what changes.

```cs
Key<Blog, int> blogId = 10;
Key<Post, int> postId = 14;

var post = GetPostByBlog(postId, blogId);

public Post GetPostByBlog(Key<Blog, int> blogId, Key<Post, int> postId)
{
    // Omitted for brevity 
}
```

This will no longer compile, due to the fact, that the  `Key<Post, int>` stored in `postId` can not be implicitly converted to the 
`Key<Blog, int>` parameter defined in the by the `GetPostByBlog` method.

## Reducing the boilerplate

This is already an improvement, but it is somewhat annoying to always specify the type the key belongs to as well as the type of the key. In most databases you will most likely end up with the same type of id for all of your tables. Therefor it is rather unnecessary to specify it every time in code. Venflow provides you with a Source Generator to create a strongly-typed id with a fixed type for us. However, do note that Source Generators are only available through C#9.

```cs
[Venflow.GeneratedKey(typeof(int))]
public partial struct Key<T> { }
```

From now on you would be able to create all your strongly-typed ids as shown below.

```cs
public class Post
{
    public Key<Post> Id { get; set; } // Using Key<T> instead of Key<T, TKey>
    public string Title { get; set; }
    public string Content { get; set; }
    
    public Key<Blog> BlogId { get; set; } // Using Key<T> instead of Key<T, TKey>
    public Blog Blog { get; set; }
}
```

If required you could also create multiple strongly-typed ids with a fixed types, by naming them differently, for example `IntKey<T>` or `GuidKey<T>`.

> [!NOTE] 
If you want to use Strongly-typed id's without referencing the whole ORM, you can install Venflow.Keys from NuGet.

## Support for JSON

Venflow natively supports `System.Text.Json` and `Newtonsoft.Json`, however if you do want to use `Newtonsoft.Json` you will need to additionally add `Venflow.NewtonsoftJson`.  

### System.Text.Json

You will need to tell the serializer and de-serializer how to handle the new type, you can do this by adding the following line to your `JsonOptions`.

```cs
var options = new JsonOptions();

options.Converters.Add(new JsonKeyConverterFactory());
```

### Newtonsoft.Json

You will need to tell the serializer and de-serializer how to handle the new type, you can do this by adding the following line to your `JsonSerializerSettings`.

```cs
var settings = new JsonSerializerSettings();

settings.Converters.Add(new NewtonsoftJsonKeyConverter());
```

## Support for Asp.Net Core

Venflow natively supports `System.Text.Json` with Asp.Net Core through the `Venflow.AspNetCore` NuGet package. In your `Startup.cs` file you can add the code below in your `ConfigureServices` method.

```cs
servics.AddVenflowJson();
```

### Newtonsoft.Json

At the moment you still have to write your own implementation for `Newtonsoft.Json`, an example can be found below.
```cs
servics.AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.Converters.Add(new NewtonsoftJsonKeyConverter());
        });
```