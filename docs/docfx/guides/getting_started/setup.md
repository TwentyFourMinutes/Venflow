---
uid: Guides.GettingStarted.Setup
title: Setup Venflow
---

# Venflow Setup

## Configure the Database

In Venflow you are reflecting your PostgreSQL database with the `Database` class, which will host all of your tables. In the following example we will configure a database containing two tables, `Blogs` and `Posts`. One Blog contains many posts and a post contains a single Blog.

```cs
public class BlogDatabase : Database
{
    public Table<Blog> Blogs { get; set; }
    public Table<Post> Posts { get; set; }

    public BlogDatabase() : base("Your connection string.")
    {
    }
}
```
> [!NOTE] 
> Usually you would use your `Database` with an IOC Container and register it as a `Transient`/`Scoped` depending on the use.

> [!WARNING] 
> This class represents a connection to your database and therefor doesn't support multi threaded use.

## Entities

Entities represent a row inside a table of your database, for our current example this would look something like the following. Entities have to follow a specific schema:

- The Entity itself has to be public.
- All properties representing a column have to be public and need to have a public setter.

```cs
public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public IList<Post> Posts { get; }
    
    public Blog()
    {
        Posts = new List<Post>();
    }
}

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    
    public int BlogId { get; set; }
    public Blog Blog { get; set; }
}
```

> [!NOTE] 
> You don't necessarily need to instantiate the `Posts` navigation property on the `Blog` Entity, since Venflow will instantiate them otherwise. However in most cases it is more convenient to instantiate them on your own, otherwise foreign collections might be `null`!

> [!WARNING] 
> All properties which you want to be updateable by change-tracking have to be marked as virtual!

## Configuring Entities

Now lets configure the actual relation between Blogs and Posts through the `EntityConfiguration<T>` class. In the `Configure` , method you can configure several things such as the name of the table this entity should map to and much more. These configuration classes do automatically get discovered, if they are in the same assembly as the `Database` class. If they are not in the same assembly, you can override the `Configure` method in the `Database` class which passes in a `DatabaseOptionsBuilder`, which will allow you to specify assemblies which should also be searched for entity configurations.

```cs
public class BlogConfiguration : EntityConfiguration<Blog>
{
    protected override void Configure(IEntityBuilder<Blog> entityBuilder)
    {
        entityBuilder.HasMany(b => b.Posts)
                     .WithOne(p => p.Blog)
                     .UsingForeignKey(p => p.PostId);
    }
}
```

> [!NOTE] 
> Most of the configurations have to be configured with the `EntityConfiguration<T>` class, however there are a few exceptions to this rule.
>
> - Primary Keys get automatically mapped if they are named `Id` or decorated with the `KeyAttribute`.
> - A property can also be ignored with the `NotMappedAttribute`.