using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using Npgsql;

namespace Venflow.Modeling.Definitions.Builder
{
    public interface IEntityPropertyBuilder<TEntity>
        where TEntity : class, new()
    {
        IEntityPropertyBuilder<TEntity> HasId<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, DatabaseGeneratedOption option);

        /// <summary>
        /// Maps a PostgreSQL enum to this CLR enum column.
        /// </summary>
        /// <typeparam name="TTarget">The type of the enum.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the enum which should be mapped on this entity type.</param>
        /// <param name="name">The name of the enum in PostgreSQL, if none used it will try to convert the name of the CLR enum e.g. 'FooBar' to 'foo_bar'</param>
        /// <param name="npgsqlNameTranslator">A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class). Defaults to <see cref="Npgsql.NameTranslation.NpgsqlSnakeCaseNameTranslator"/>.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> HasPostgresEnum<TTarget>(Expression<Func<TEntity, TTarget>> propertySelector, string? name = default, INpgsqlNameTranslator? npgsqlNameTranslator = default)
            where TTarget : struct, Enum;

        /// <summary>
        /// Maps a PostgreSQL enum to this CLR enum column.
        /// </summary>
        /// <typeparam name="TTarget">The type of the enum.</typeparam>
        /// <param name="propertySelector">A lambda expression representing the enum which should be mapped on this entity type.</param>
        /// <param name="name">The name of the enum in PostgreSQL, if none used it will try to convert the name of the CLR enum e.g. 'FooBar' to 'foo_bar'</param>
        /// <param name="npgsqlNameTranslator">A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class). Defaults to <see cref="Npgsql.NameTranslation.NpgsqlSnakeCaseNameTranslator"/>.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> HasPostgresEnum<TTarget>(Expression<Func<TEntity, TTarget?>> propertySelector, string? name = default, INpgsqlNameTranslator? npgsqlNameTranslator = default)
            where TTarget : struct, Enum;

        IEntityPropertyBuilder<TEntity> HasDefaultValue(string sql);
        IEntityPropertyBuilder<TEntity> HasDbType(uint precision, uint scale);
        IEntityPropertyBuilder<TEntity> HasPrecision(uint precision);
        IEntityPropertyBuilder<TEntity> HasPrecision(uint precision, uint scale);
        IEntityPropertyBuilder<TEntity> WithComment(string comment);
        /// <summary>
        /// only required in non C#8+ null-able context
        /// </summary>
        /// <returns></returns>
        IEntityPropertyBuilder<TEntity> IsRequired();
        IEntityPropertyBuilder<TEntity> IsRequired(bool required);
    }
}
