---
uid: Guides.Advanced.Interpolation
title: Interpolation with Venflow
---

# Interpolation with Venflow

*If you never heard about string interpolation you should check the [official docs](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated).*

Writing SQL can be a real pain especially while writing in by hand and keeping it injection safe. However Venflow tries to help you with all of that, especially by providing a simple way to write injection safe SQL. All API's which allow for SQL with parameters also have a counterpart called something along the lines of \*Interpolation\*. They accept a [`FormattableString`](xref:System.FormattableString) which allow for interpolated strings. Lets take a look at how this would like with a query, that queries all blogs with a similar one to the name provided by the user.

```cs
var name = Console.ReadLine(); // The name of the blogs to find with a similar name

var blogs = await database.Blogs.QueryInterpolatedBatch($@"SELECT * FROM ""Blogs"" WHERE ""Name"" LIKE {name}")
                                .QueryAsync();
```

> [!WARNING] 
> This way of injecting parameters is totally safe, however you need to be very careful to **always** choose the interpolation methods while doing this. 

### Extract interpolated SQL into variables

If your SQL statement is a little bit larger than usual you might want to extract your string to a local variable or similar. However you need to be careful while choosing the variable type. Your habits might want to tell you to choose a [`string`](xref:System.String) or [`var`](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/var) however you need to make sure that you explicitly set the variable type to [`FormattableString`](xref:System.FormattableString). Otherwise the string interpolation happens inline and no parameterizing by Venflow will happen. Down below you will see a simple example of how that would look like.

```cs
FormattableString sql = $@"SELECT * FROM ""Blogs"" WHERE ""Name"" LIKE {Console.ReadLine()}";

var blogs = await database.Blogs.QueryInterpolatedBatch(sql)
                                .QueryAsync();
```

## Supported interpolation types

At the current state you can use all types [Npgsql supports](https://www.npgsql.org/doc/types/basic.html), as well as a few neat features Venflow implemented such as the support for [`ulong`](xref:System.UInt64), [`ulong?`](xref:System.UInt64) and all types that implement [`IList<T>`](xref:System.Collections.Generic.IList`1), this includes types such as arrays and all collections that implement it. Lets look at its behaviour with an example.

```cs
var blogIds = new int[3] { 1, 2, 3 };

FormattableString sql = $@"SELECT * FROM ""Blogs"" WHERE ""Id"" IN ({blogIds})";

var blogs = await database.Blogs.QueryInterpolatedBatch(sql)
                                .QueryAsync();
```

This will query all blogs with the ids `1`, `2` and `3`. The above written SQL statement will be converted to the following:

`"SELECT * FROM ""Blogs"" WHERE ""Id"" IN (1, 2, 3)"` 

Of course the numbers usually would be parametrized, however for the sake of this example it contains the values directly.  

## Dynamic SQL

There might be situations in which you need to dynamically generate SQL with parameters, in which case the common [`StringBuilder`](xref:System.Text.StringBuilder) isn't sufficient enough. Venflow provides you with the [`FormattableSqlStringBuilder`](xref:Venflow.FormattableSqlStringBuilder) class which acts like a  [`StringBuilder`](xref:System.Text.StringBuilder), however it provides methods, which allow for interpolated SQL. Lets take a look at this with a more practical example.

```cs
public Task<List<Blogs>> GetBlogsAsync(string[]? names)
{
    var stringBuilder = new FormattableSqlStringBuilder();
	
    stringBuilder.Append(@"SELECT * FROM ""Blogs""");
    
    if(names is not null &&
	   names.Length > 0)
    {
        stringBuilder.Append(@" WHERE ""Name"" IN (");
        stringBuilder.AppendParameter(names);
        stringBuilder.AppendInterpolated(@$") AND LENGTH(""Name"") > {5}");
    }
    
    return database.Blogs.QueryInterpolatedBatch(stringBuilder).QueryAsync();
}
```

Obviously the query shown above is not too useful, however if names would be provided, it would only query those and additionally they would need to be longer than 5 characters.

