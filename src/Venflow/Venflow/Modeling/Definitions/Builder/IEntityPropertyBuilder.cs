using System.ComponentModel.DataAnnotations.Schema;
using Npgsql;
using NpgsqlTypes;

namespace Venflow.Modeling.Definitions.Builder
{
    public interface IEntityPropertyBuilder<TEntity>
        where TEntity : class, new()
    {
        IEntityPropertyBuilder<TEntity> IsId(DatabaseGeneratedOption option);
        IIndexBuilder<TEntity> IsIndex();
        IIndexBuilder<TEntity> IsIndex(string name);

        /// <summary>
        /// Maps a PostgreSQL enum to this CLR enum column.
        /// </summary>
        /// <param name="name">The name of the enum in PostgreSQL, if none used it will try to convert the name of the CLR enum e.g. 'FooBar' to 'foo_bar'</param>
        /// <param name="npgsqlNameTranslator">A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class). Defaults to <see cref="Npgsql.NameTranslation.NpgsqlSnakeCaseNameTranslator"/>.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IEntityBuilder<TEntity> HasPostgresEnum(string? name = default, INpgsqlNameTranslator? npgsqlNameTranslator = default);

        IEntityPropertyBuilder<TEntity> HasDefaultValue(string sql);
        IEntityPropertyBuilder<TEntity> HasDbType(NpgsqlDbType dbType);
        IEntityPropertyBuilder<TEntity> HasPrecision(uint precision);
        IEntityPropertyBuilder<TEntity> HasPrecision(uint precision, uint scale);
        IEntityPropertyBuilder<TEntity> HasMaxLength(uint length);
        IEntityPropertyBuilder<TEntity> WithName(string name);
        IEntityPropertyBuilder<TEntity> WithComment(string comment);
        /// <summary>
        /// only required in non C#8+ null-able context
        /// </summary>
        /// <returns></returns>
        IEntityPropertyBuilder<TEntity> IsRequired();
        IEntityPropertyBuilder<TEntity> IsRequired(bool required);
    }
}
