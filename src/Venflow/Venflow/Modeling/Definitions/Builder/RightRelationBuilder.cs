using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions.Builder
{
    internal class RightRelationBuilder<TEntity, TRelation> : INotRequiredMultiRightRelationBuilder<TEntity, TRelation>, IRequiredMultiRightRelationBuilder<TEntity, TRelation>, IForeignKeyRelationBuilder<TEntity, TRelation>, IRelationConfigurationBuilder<TEntity, TRelation>
        where TEntity : class, new()
        where TRelation : class
    {
        private EntityRelationDefinition _entityRelationDefinition;

        private PropertyInfo? _rightNavigationProperty;
        private RelationPartType _leftRelationType;

        private readonly RelationPartType _rightRelationType;
        private readonly PropertyInfo? _leftNavigationProperty;
        private readonly EntityBuilder<TEntity> _entityBuilder;

        internal RightRelationBuilder(RelationPartType rightRelationType, PropertyInfo? leftNavigationProperty, EntityBuilder<TEntity> entityBuilder)
        {
            _rightRelationType = rightRelationType;
            _leftNavigationProperty = leftNavigationProperty;
            _entityBuilder = entityBuilder;
        }

        IForeignKeyRelationBuilder<TEntity, TRelation> IMultiRightRelationBuilder<TEntity, TRelation>.WithMany(Expression<Func<TRelation, IList<TEntity>>> navigationProperty)
        {
            _leftRelationType = RelationPartType.Many;

            _rightNavigationProperty = navigationProperty.ValidatePropertySelector(false);

            return this;
        }


        IForeignKeyRelationBuilder<TEntity, TRelation> INotRequiredMultiRightRelationBuilder<TEntity, TRelation>.WithMany()
        {
            _leftRelationType = RelationPartType.Many;

            return this;
        }

        IForeignKeyRelationBuilder<TEntity, TRelation> IRequiredSingleRightRelationBuilder<TEntity, TRelation>.WithOne(Expression<Func<TRelation, TEntity>> navigationProperty)
        {
            _leftRelationType = RelationPartType.One;

            _rightNavigationProperty = navigationProperty.ValidatePropertySelector();

            return this;
        }

        IForeignKeyRelationBuilder<TEntity, TRelation> INotRequiredSingleRightRelationBuilder<TEntity, TRelation>.WithOne()
        {
            _leftRelationType = RelationPartType.One;

            return this;
        }

        IRelationConfigurationBuilder<TEntity, TRelation> IForeignKeyRelationBuilder<TEntity, TRelation>.UsingForeignKey<TKey>(Expression<Func<TEntity, TKey>> navigationProperty)
        {
            ApplyRelation(navigationProperty.ValidatePropertySelector(), ForeignKeyLocation.Left);

            return this;
        }

        IRelationConfigurationBuilder<TEntity, TRelation> IForeignKeyRelationBuilder<TEntity, TRelation>.UsingForeignKey<TKey>(Expression<Func<TRelation, TKey>> navigationProperty)
        {
            ApplyRelation(navigationProperty.ValidatePropertySelector(), ForeignKeyLocation.Right);

            return this;
        }

        IRelationConfigurationBuilder<TEntity, TRelation> IRelationConfigurationBuilder<TEntity, TRelation>.HasConstraintName(string name)
        {
            if (VenflowConfiguration.PopulateEntityInformation)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new InvalidOperationException($"The constraint name '{name}' is invalid.");
                }

                _entityRelationDefinition.Information.ConstraintName = name;
            }

            return this;
        }

        IRelationConfigurationBuilder<TEntity, TRelation> IRelationConfigurationBuilder<TEntity, TRelation>.OnDelete(ConstraintAction constraintAction)
        {
            if (VenflowConfiguration.PopulateEntityInformation)
            {
                _entityRelationDefinition.Information.OnDeleteAction = constraintAction;
            }

            return this;
        }

        IRelationConfigurationBuilder<TEntity, TRelation> IRelationConfigurationBuilder<TEntity, TRelation>.OnUpdate(ConstraintAction constraintAction)
        {
            if (VenflowConfiguration.PopulateEntityInformation)
            {
                _entityRelationDefinition.Information.OnUpdateAction = constraintAction;
            }

            return this;
        }

        private void ApplyRelation(PropertyInfo foreignKey, ForeignKeyLocation keyLoaction)
        {
            _entityRelationDefinition = new EntityRelationDefinition(EntityBuilder.RelationCounter++, _entityBuilder, _leftNavigationProperty, typeof(TRelation).Name, _rightNavigationProperty, foreignKey.Name, GetRelationFromParts(_leftRelationType, _rightRelationType), keyLoaction);

            _entityBuilder.Relations.Add(_entityRelationDefinition);
        }

        private RelationType GetRelationFromParts(RelationPartType leftPart, RelationPartType rightPart)
        {
            if (leftPart == RelationPartType.One)
            {
                return rightPart == RelationPartType.Many ? RelationType.OneToMany : RelationType.OneToOne;
            }
            else
            {
                return RelationType.ManyToOne;
            }
        }
    }
}
