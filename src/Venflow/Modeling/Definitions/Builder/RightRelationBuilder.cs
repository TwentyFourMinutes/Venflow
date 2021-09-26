using System.Linq.Expressions;
using Venflow.Enums;

namespace Venflow.Modeling.Definitions.Builder
{
    internal class RightRelationBuilder<TEntity, TRelation> : INotRequiredMultiRightRelationBuilder<TEntity, TRelation>, IRequiredMultiRightRelationBuilder<TEntity, TRelation>, IForeignKeyRelationBuilder<TEntity, TRelation> where TEntity : class, new() where TRelation : class
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

            _rightNavigationProperty = navigationProperty.ValidatePropertySelector(false);

            if (!_rightNavigationProperty.CanWrite &&
                _rightNavigationProperty.GetBackingField() is null)
            {
                throw new InvalidOperationException($"The foreign property '{_rightNavigationProperty.Name}' on the entity '{_rightNavigationProperty!.ReflectedType!.Name}' doesn't implement a setter, nor does it match the common backing field pattern ('<{_rightNavigationProperty.Name}>k__BackingField', '{char.ToLowerInvariant(_leftNavigationProperty!.Name[0])}{_leftNavigationProperty.Name.Substring(1)}' or  '_{char.ToLowerInvariant(_leftNavigationProperty.Name[0])}{_leftNavigationProperty.Name.Substring(1)}').");
            }

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

            if (!_rightNavigationProperty.CanWrite &&
                _rightNavigationProperty.GetBackingField() is null)
            {
                throw new InvalidOperationException($"The foreign property '{_rightNavigationProperty.Name}' on the entity '{_rightNavigationProperty!.ReflectedType!.Name}' doesn't implement a setter, nor does it match the common backing field pattern ('<{_rightNavigationProperty.Name}>k__BackingField', '{char.ToLowerInvariant(_leftNavigationProperty!.Name[0])}{_leftNavigationProperty!.Name.Substring(1)}' or  '_{char.ToLowerInvariant(_leftNavigationProperty.Name[0])}{_leftNavigationProperty.Name.Substring(1)}').");
            }

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
