using System.Linq.Expressions;

namespace Venflow.Modeling.Definitions
{
    internal static class ExpressionExtensions
    {
        internal static PropertyInfo ValidatePropertySelector<TSource, TTarget>(this Expression<Func<TSource, TTarget>> propertySelector, bool validateSetter = true)
        {
            if (propertySelector.Body is not MemberExpression body)
            {
                throw new ArgumentException($"The provided {nameof(propertySelector)} is not pointing to a property.", nameof(propertySelector));
            }

            if (body.Member is not PropertyInfo property)
            {
                throw new ArgumentException($"The provided {nameof(propertySelector)} is not pointing to a property.", nameof(propertySelector));
            }

            if (validateSetter && (!property.CanWrite || !property.SetMethod!.IsPublic))
            {
                throw new ArgumentException($"The provided property doesn't contain a setter or it isn't public.", nameof(propertySelector));
            }

            var type = typeof(TSource);

            if (type != property.ReflectedType &&
                !type.IsSubclassOf(property.ReflectedType!))
            {
                throw new ArgumentException($"The provided {nameof(propertySelector)} is not pointing to a property on the entity itself.", nameof(propertySelector));
            }

            return property;
        }
    }
}
