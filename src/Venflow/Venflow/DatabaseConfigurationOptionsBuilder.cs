using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Npgsql;
using Npgsql.NameTranslation;
using Venflow.Modeling.Definitions;

namespace Venflow
{
    /// <summary>
    /// Provides an option builder to further <i>statically</i> configure a <see cref="Database"/> instance.
    /// </summary>
    public class DatabaseConfigurationOptionsBuilder
    {
        internal Database EffectiveDatabase { get; }
        internal Type EffectiveDatabaseType { get; }
        internal List<Assembly> ConfigurationAssemblies { get; }
        internal INpgsqlNameTranslator NpgsqlNameTranslator { get; private set; }

        internal DatabaseConfigurationOptionsBuilder(Database effectiveDatabase)
        {
            EffectiveDatabase = effectiveDatabase;
            EffectiveDatabaseType = EffectiveDatabase.GetType();
            ConfigurationAssemblies = new(1) { EffectiveDatabaseType.Assembly };
            NpgsqlNameTranslator = new NpgsqlSnakeCaseNameTranslator();
        }

        /// <summary>
        /// Adds the assembly of the type <typeparamref name="T"/> to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        /// <typeparam name="T">The type of which the assembly should be added to the lookup list.</typeparam>
        /// <returns>An object that can be used to configure the current <see cref="Database"/> instance.</returns>
        /// <remarks>If you add a custom configuration location, the assembly of the database type will not be automatically included.</remarks>
        public DatabaseConfigurationOptionsBuilder UseConfigurations<T>()
        {
            ConfigurationAssemblies.Add(typeof(T).Assembly);

            return this;
        }

        /// <summary>
        /// Adds the assembly to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        /// <param name="assembly">The assembly which should be added to the lookup list.</param>
        /// <returns>An object that can be used to configure the current <see cref="Database"/> instance.</returns>
        /// <remarks>If you add a custom configuration location, the assembly of the database type will not be automatically included.</remarks>
        public DatabaseConfigurationOptionsBuilder UseConfigurations(Assembly assembly)
        {
            ConfigurationAssemblies.Add(assembly);

            return this;
        }

        /// <summary>
        /// Adds the assemblies to the <see cref="EntityConfiguration{TEntity}"/> lookup list.
        /// </summary>
        /// <param name="assemblies">The assemblies which should be added to the lookup list.</param>
        /// <returns>An object that can be used to configure the current <see cref="Database"/> instance.</returns>
        /// <remarks>If you add a custom configuration location, the assembly of the database type will not be automatically included.</remarks>
        public DatabaseConfigurationOptionsBuilder UseConfigurations(params Assembly[] assemblies)
        {
            ConfigurationAssemblies.AddRange(assemblies);

            return this;
        }

        /// <summary>
        /// Sets the naming convention to be used for entity table & column names.
        /// </summary>
        /// <typeparam name="T">An implementation of <see cref="INpgsqlNameTranslator"/> to be used for name translation.</typeparam>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public DatabaseConfigurationOptionsBuilder SetNamingConvention<T>()
            where T : INpgsqlNameTranslator, new()
        {
            NpgsqlNameTranslator = new T();

            return this;
        }


        /// <summary>
        /// Maps a PostgreSQL enum to a CLR enum.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="name">The name of the enum in PostgreSQL, if none used it will try to convert the name of the CLR enum e.g. 'FooBar' to 'foo_bar'</param>
        /// <param name="npgsqlNameTranslator">A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class). Defaults to <see cref="Npgsql.NameTranslation.NpgsqlSnakeCaseNameTranslator"/>.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public DatabaseConfigurationOptionsBuilder RegisterPostgresEnum<TEnum>(string? name = default, INpgsqlNameTranslator? npgsqlNameTranslator = default) where TEnum : struct, Enum
        {
            var type = typeof(TEnum);

            if (string.IsNullOrWhiteSpace(name))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);

                name = underlyingType is not null ? underlyingType.Name : type.Name;

                var nameBuilder = new StringBuilder(name.Length * 2 - 1);

                nameBuilder.Append(char.ToLowerInvariant(name[0]));

                var nameSpan = name.AsSpan();

                for (int i = 1; i < nameSpan.Length; i++)
                {
                    var c = nameSpan[i];

                    if (char.IsUpper(c))
                    {
                        nameBuilder.Append('_');
                        nameBuilder.Append(char.ToLowerInvariant(c));
                    }
                    else
                    {
                        nameBuilder.Append(c);
                    }
                }

                name = nameBuilder.ToString();
            }

            if (!ParameterTypeHandler.PostgreEnums.Contains(type))
            {
                NpgsqlConnection.GlobalTypeMapper.MapEnum<TEnum>(name, npgsqlNameTranslator);

                ParameterTypeHandler.PostgreEnums.Add(type);
            }

            return this;
        }
    }
}