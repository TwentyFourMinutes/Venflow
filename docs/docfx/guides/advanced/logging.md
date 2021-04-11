---
uid: Guides.Advanced.Logging
title: Logging with Venflow
---

# Logging with Venflow

Logging in general is a very important topic, especially in a scenario like this, in which the ORM partially creates SQL. At the current state, Venflow only supports logging executed commands, rather than the logging of internal events.

## Setup the logging provider

You configure your logging provider on a [`Database`](xref:Venflow.Database) basis, by overriding the [`Configure`](xref:Venflow.Database.Configure(Venflow.DatabaseOptionsBuilder)) method. For this example, lets use the database which you already know from the [Setup Guide](../getting_started/setup.md). By using the provided [`DatabaseOptionsBuilder`](xref:Venflow.DatabaseOptionsBuilder), you can call the [`LogTo`](xref:Venflow.DatabaseOptionsBuilder.LogTo(Venflow.LoggerCallback)) method on it. The delegate defines three parameters, the executed [`NpgsqlCommand`](xref:Npgsql.NpgsqlCommand), the  [`CommandType`](xref:Venflow.Enums.CommandType) executed as well as the exception if any occurred. However it is important to note that Venflow, at least at the current state, only logs the executed commands and nothing else.

> [!NOTE] 
> The sensitive data logging is a bit special, since it populates the parameters on the client, rather than on the server. This means, that the _populated_ SQL might not always be 100% accurate. To get the the sensitive data call the [`NpgsqlCommandExtensions.GetUnParameterizedCommandText`](xref:Venflow.NpgsqlCommandExtensions.SetInterpolatedCommandText(Npgsql.NpgsqlCommand,System.FormattableString)) extension method on the provided [`NpgsqlCommand`](xref:Npgsql.NpgsqlCommand) instance. However, it also got some other caveats, you can get more information by further inspecting the API reference.


```cs
public class BlogDatabase : Database
{
    public Table<Blog> Blogs { get; set; }
    public Table<Post> Posts { get; set; }

    public BlogDatabase() : base("Your connection string.")
    {
        
    }
    
    protected override void Configure(DatabaseOptionsBuilder optionsBuilder)
    {
        // You can also configure multiple loggers.
        optionsBuilder.LogTo((command, type, exception) => Console.WriteLine(command.CommandText));
    }
}
```

## Setup the logging provider in a more specific manner

You don't always want the ORM to log every single SQL statement ever to be executed, in this case Venflow allows to individually override the logging behaviour on a command basis by calling [`LogTo`](xref:Venflow.Commands.IQueryCommandBuilder`2.LogTo(System.Boolean) ) on the method chain. In the example down below, all globally defined loggers will be overridden by the loggers configured on the command.

```cs
FormattableString sql = $@"SELECT * FROM ""Blogs"" WHERE ""Id"" = {someId} LIMIT 1";

var blog = await database.Blogs.QueryInterpolatedSingle(sql)
    						   // You can also configure multiple loggers.
                               .LogTo((command, type, exception) => Console.WriteLine(command.CommandText))
                               .QueryAsync();
```

However, lets assume you wouldn't want to configure the logger on a command basis, but would rather cherry pick the commands. In that case you would need to set the  [`DatabaseOptionsBuilder.DefaultLoggingBehavior`](xref:Venflow.DatabaseOptionsBuilder.DefaultLoggingBehavior) to [`DefaultLoggingBehavior.Never`](xref:Venflow.Enums.LoggingBehavior.Never) in the same method you configured the global logger. Then you would want to call [`LogTo`](xref:Venflow.Commands.IQueryCommandBuilder`2.LogTo(System.Boolean) ) on all commands you would want to be logged.

