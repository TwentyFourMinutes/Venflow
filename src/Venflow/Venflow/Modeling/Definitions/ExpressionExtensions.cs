using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Venflow.Modeling.Definitions
{
    internal static class ExpressionExtensions
    {
        internal static PropertyInfo ValidatePropertySelector<TSource, TTarget>(this Expression<Func<TSource, TTarget>> propertySelector, bool validateSetter = true)
        {
            var body = propertySelector.Body as MemberExpression;

            if (body is null)
            {
                throw new ArgumentException($"The provided '{body}' is not pointing to a property.", nameof(propertySelector));
            }

            var property = body.Member as PropertyInfo;

            if (property is null)
            {
                throw new ArgumentException($"The provided '{body}' is not pointing to a property.", nameof(propertySelector));
            }

            if (validateSetter && (!property.CanWrite || !property.SetMethod.IsPublic))
            {
                throw new ArgumentException($"The provided property doesn't contain a setter or it isn't public.", nameof(propertySelector));
            }

            var type = typeof(TSource);

            if (type != property.ReflectedType &&
                !type.IsSubclassOf(property.ReflectedType))
            {
                throw new ArgumentException($"The provided '{body}' is not pointing to a property on the entity itself.", nameof(propertySelector));
            }

            return property;
        }

        internal static PropertyInfo[] ValidateMultiPropertySelector<TSource>(this Expression<Func<TSource, object>> propertiesSelector, Type parent)
        {
            if (propertiesSelector.Body is not NewExpression newExpression)
            {
                throw new InvalidOperationException($"The expression doesn't represent an anonymous object.");
            }

            var propertyInfos = new PropertyInfo[newExpression.Members.Count];

            for (int memberIndex = 0; memberIndex < newExpression.Members.Count; memberIndex++)
            {
                var member = newExpression.Members[memberIndex];

                if (member is not PropertyInfo property)
                {
                    throw new InvalidOperationException($"The member '{member}' doesn't represent a property.");
                }

                var parentProperty = parent.GetProperty(property.Name);

                if (parentProperty is null)
                {
                    throw new InvalidOperationException($"The entity '{parent.Name}' doesn't contain a property named '{property.Name}'");
                }

                propertyInfos[memberIndex] = parentProperty;
            }

            return propertyInfos;
        }
    }
}
