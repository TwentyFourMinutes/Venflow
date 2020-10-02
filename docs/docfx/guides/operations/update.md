---
uid: Guides.Operations.Update
title: Update with Venflow
---

# Update Data with Venflow

Your `Database` class exposes `Table<T>` properties which expose update operations. In Venflow updates are always automatically generated. Updates of data happen through change-tracking of entities, however this does not include navigation properties. For this example, we want to update the title of a post.

> [!WARNING] 
> All properties which you want to be updateable by change-tracking have to  be marked as virtual!

In order to get a change-tracked entity you can choose either of two ways. You can use the `TrackChanges` method on the query builder to immediately get change tracked entities.

```cs
var post = await database.Posts.QuerySingle().TrackChanges().QueryAsync();

post.Title = "This post was updated!";

await database.Posts.UpdateAsync(post);
```

Or you change track an entity after it was created.

```cs
var post = new Post { Id = 1 };

database.Posts.TrackChanges(ref post);

post.Title = "This post was updated!";

await database.Posts.UpdateAsync(post);
```
> [!NOTE] 
> Change tracking is not limited to one `Database` instance, additionally updating a change tracked entity is thread save.

> [!WARNING] 
> Change tracking won't compare the old value and the new value of a property, this means that if you assign a property, no matter the value, it is considered to be changed.