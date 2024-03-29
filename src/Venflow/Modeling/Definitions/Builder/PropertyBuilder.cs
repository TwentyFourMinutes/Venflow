﻿using System.ComponentModel.DataAnnotations;
using NpgsqlTypes;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions.Builder
{
    internal class PropertyBuilder : IPropertyBuilder
    {
        private readonly ColumnDefinition _definition;

        internal PropertyBuilder(ColumnDefinition definition)
        {
            _definition = definition;
        }

        IPropertyBuilder IPropertyBuilder.HasDefault()
        {
            _definition.Options |= ColumnOptions.DefaultValue;

            return this;
        }

        IPropertyBuilder IPropertyBuilder.HasId()
        {
            _definition.Options |= ColumnOptions.PrimaryKey;

            return this;
        }

        IPropertyBuilder IPropertyBuilder.WithName(string name)
        {
            _definition.Name = name;

            return this;
        }

        IPropertyBuilder IPropertyBuilder.WithType(NpgsqlDbType dbType)
        {
            _definition.DbType = dbType;

            return this;
        }
    }

    /// <summary>
    /// Instances of this class are returned from methods inside the <see cref="EntityBuilder{TEntity}"/> class when using the Fluent API and it is not designed to be directly constructed in your application code.
    /// </summary>
    public interface IPropertyBuilder
    {
        /// <summary>
        /// Marks the current property as a primary key. This is the Fluent API equivalent to the <see cref="KeyAttribute"/>.
        /// </summary>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IPropertyBuilder HasId();

        /// <summary>
        /// Configures the name of the current column, if not configured it will use the name of the property.
        /// </summary>
        /// <param name="name">The name of the column in the database to which the used property should map to.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IPropertyBuilder WithName(string name);

        /// <summary>
        /// Configures the database type of the current column, if not configured it will use the default of the property.
        /// </summary>
        /// <param name="dbType">The type of the column in the database.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IPropertyBuilder WithType(NpgsqlDbType dbType);

        /// <summary>
        /// Marks the current column to be generated by the database
        /// </summary>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        IPropertyBuilder HasDefault();
    }
}
