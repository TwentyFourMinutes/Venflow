using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Venflow.Enums;

namespace Venflow.Commands
{
    internal static class InterpolatedSqlExpressionConverter
    {
        internal static (Delegate function, SqlExpressionOptions options, Type? parameterType) GetConvertedDelegate(List<Expression> instanceArguments)
        {
            var visitor = new ConstantExpressionVisitor();

            if (instanceArguments.Count > 0)
            {
                for (int instanceArgumentIndex = 0; instanceArgumentIndex < instanceArguments.Count; instanceArgumentIndex++)
                {
                    visitor.Visit(instanceArguments[instanceArgumentIndex]);
                }
            }

            if (visitor.DisplayClassType is null &&
                visitor.ThisType is null)
            {
                return (Expression.Lambda<Func<object[]>>(Expression.NewArrayInit(typeof(object), instanceArguments)).Compile(), SqlExpressionOptions.None, null);
            }

            var replacer = new ConstantExpressionReplacer(visitor.DisplayClassType, visitor.ThisType);

            for (int instanceArgumentIndex = 0; instanceArgumentIndex < instanceArguments.Count; instanceArgumentIndex++)
            {
                instanceArguments[instanceArgumentIndex] = replacer.Visit(instanceArguments[instanceArgumentIndex]);
            }

            return (Expression.Lambda<Func<object, object[]>>(Expression.Block(new[] { replacer.LocalExpression }, new Expression[] { replacer.ConvertExpression, Expression.NewArrayInit(typeof(object), instanceArguments) }), replacer.ParameterExpression).Compile(),
                    SqlExpressionOptions.HasDelegateParameter,
                    visitor.DisplayClassType is not null ? visitor.DisplayClassType : visitor.ThisType);
        }

        internal static object? ExtractInstance(Expression expression, Type type)
        {
            var extractor = new InstanceExpressionExtractor(type);

            extractor.Visit(expression);

            return extractor.Instance;
        }

        private class InstanceExpressionExtractor : ExpressionVisitor
        {
            internal object? Instance { get; private set; }

            private readonly Type _type;

            internal InstanceExpressionExtractor(Type type)
            {
                _type = type;
            }

#if !NET48
            [return: NotNullIfNotNull("node")]
#endif
            public override Expression? Visit(Expression? node)
            {
                if (Instance is not null)
                    return node;

                return base.Visit(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Type == _type)
                {
                    Instance = node.Value;

                    return node;
                }

                return base.VisitConstant(node);
            }
        }

        private class ConstantExpressionReplacer : ExpressionVisitor
        {
            internal ParameterExpression LocalExpression { get; }
            internal BinaryExpression ConvertExpression { get; }
            internal ParameterExpression ParameterExpression { get; }

            private readonly Type? _displayClassType;
            private readonly Type? _thisType;
            private readonly FieldInfo? _thisField;

            internal ConstantExpressionReplacer(Type? displayClassType, Type? thisType)
            {
                _displayClassType = displayClassType;
                _thisType = thisType;

                ParameterExpression = Expression.Parameter(typeof(object));

                if (displayClassType is null)
                {
                    LocalExpression = Expression.Parameter(thisType);
                    ConvertExpression = Expression.Assign(LocalExpression, Expression.Convert(ParameterExpression, thisType));
                }
                else
                {
                    LocalExpression = Expression.Parameter(displayClassType);
                    ConvertExpression = Expression.Assign(LocalExpression, Expression.Convert(ParameterExpression, displayClassType));

                    _thisField = _displayClassType.GetFields().FirstOrDefault(x => x.FieldType == thisType && x.Name.StartsWith("<>") && x.Name.Contains("__this"));
                }
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Type == _displayClassType)
                {
                    return LocalExpression;
                }

                if (node.Type == _thisType)
                {
                    if (_displayClassType is null)
                    {
                        return LocalExpression;
                    }
                    else
                    {
                        return Expression.Field(LocalExpression, _thisField);
                    }
                }

                return base.VisitConstant(node);
            }
        }

        private class ConstantExpressionVisitor : ExpressionVisitor
        {
            internal Type? DisplayClassType { get; private set; }
            internal Type? ThisType { get; private set; }

#if !NET48
            [return: NotNullIfNotNull("node")]
#endif
            public override Expression? Visit(Expression? node)
            {
                if (DisplayClassType is not null &&
                    ThisType is not null)
                    return node;

                return base.Visit(node);
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Type.IsClass &&
                    node.Type.Name.StartsWith("<>c__DisplayClass") &&
                    Attribute.IsDefined(node.Type, typeof(CompilerGeneratedAttribute)))
                {
                    DisplayClassType = node.Type;

                    return node;
                }

                if (node.Type.IsClass &&
                    node.Type.Assembly != typeof(int).Assembly)
                {
                    ThisType = node.Type;

                    return node;
                }


                return base.VisitConstant(node);
            }
        }
    }
}