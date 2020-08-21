---
uid: Guides.Advanced.Enums
title: Enums with Venflow
---

# Enums with Venflow

Enums are quite heavily used in C# and PostgreSQL, there are two different options of mapping you can choose from. By default any enum will be mapped as the underlying numeric data type in the database. However if you want to use a native PostgreSQL enum you have to specify that in the Configuration of your entity.

```cs
public class FooConfiguration : EntityConfiguration<Foo>
{
    protected override void Configure(IEntityBuilder<Foo> entityBuilder)
    {
        entityBuilder.MapPostgresEnum(x => x.Bar);
    }
}
```

> [!NOTE] 
> By default the name of the enum within C# will be converted to a lower-cased `_` separated name. That means `FooBaz` would be converted to `foo_baz`. If you want to override any of the naming behaviours you can pass a valid value to the `name`/`npgsqlNameTranslator` parameters.

