using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions
{
    internal class RightRelationBuilder<TEntity, TRelation> : INotRequiredMultiRightRelationBuilder<TEntity, TRelation>, IRequiredMultiRightRelationBuilder<TEntity, TRelation>, IForeignKeyRelationBuilder<TEntity, TRelation> where TEntity : class where TRelation : class
    {
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

            _rightNavigationProperty = navigationProperty.ValidatePropertySelector();

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

        void IForeignKeyRelationBuilder<TEntity, TRelation>.UsingForeignKey<TKey>(Expression<Func<TEntity, TKey>> navigationProperty)
        {
            ApplyRelation(navigationProperty.ValidatePropertySelector(), ForeignKeyLocation.Left);
        }

        void IForeignKeyRelationBuilder<TEntity, TRelation>.UsingForeignKey<TKey>(Expression<Func<TRelation, TKey>> navigationProperty)
        {
            ApplyRelation(navigationProperty.ValidatePropertySelector(), ForeignKeyLocation.Right);
        }

        private void ApplyRelation(PropertyInfo foreignKey, ForeignKeyLocation keyLoaction)
        {
            _entityBuilder.Relations.Add(new EntityRelationDefinition(EntityBuilder.RelationCounter++, _entityBuilder, _leftNavigationProperty, typeof(TRelation).Name, _rightNavigationProperty, foreignKey.Name, GetRelationFromParts(_leftRelationType, _rightRelationType), keyLoaction));
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
