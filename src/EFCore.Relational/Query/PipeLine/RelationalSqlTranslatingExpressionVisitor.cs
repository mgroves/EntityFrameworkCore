// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalSqlTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;
        private readonly IMemberTranslatorProvider _memberTranslatorProvider;
        private readonly IMethodCallTranslatorProvider _methodCallTranslatorProvider;
        private readonly SqlTypeMappingVerifyingExpressionVisitor _sqlVerifyingExpressionVisitor;

        private SelectExpression _selectExpression;

        public RelationalSqlTranslatingExpressionVisitor(
            IModel model,
            ISqlExpressionFactory sqlExpressionFactory,
            IMemberTranslatorProvider memberTranslatorProvider,
            IMethodCallTranslatorProvider methodCallTranslatorProvider)
        {
            _model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
            _memberTranslatorProvider = memberTranslatorProvider;
            _methodCallTranslatorProvider = methodCallTranslatorProvider;
            _sqlVerifyingExpressionVisitor = new SqlTypeMappingVerifyingExpressionVisitor();
        }

        public SqlExpression Translate(SelectExpression selectExpression, Expression expression)
        {
            _selectExpression = selectExpression;

            var translation = (SqlExpression)Visit(expression);

            _selectExpression = null;

            translation = _sqlExpressionFactory.ApplyDefaultTypeMapping(translation);

            _sqlVerifyingExpressionVisitor.Visit(translation);

            return translation;
        }

        private class SqlTypeMappingVerifyingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitExtension(Expression node)
            {
                if (node is SqlExpression sqlExpression
                    && !(node is SqlFragmentExpression))
                {
                    if (sqlExpression.TypeMapping == null)
                    {
                        throw new InvalidOperationException("Null TypeMapping in Sql Tree");
                    }
                }

                return base.VisitExtension(node);
            }
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);
            if (innerExpression is EntityShaperExpression entityShaper)
            {
                var entityType = entityShaper.EntityType;
                var property = entityType.FindProperty(memberExpression.Member.GetSimpleMemberName());

                return _selectExpression.BindProperty(entityShaper.ValueBufferExpression, property);
            }

            return TranslationFailed(memberExpression.Expression, innerExpression)
                ? null
                : _memberTranslatorProvider.Translate((SqlExpression)innerExpression, memberExpression.Member, memberExpression.Type);

        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsEFPropertyMethod())
            {
                if (Visit(methodCallExpression.Arguments[0]) is EntityShaperExpression entityShaper)
                {
                    var entityType = entityShaper.EntityType;
                    var property = entityType.FindProperty((string)((ConstantExpression)methodCallExpression.Arguments[1]).Value);

                    return _selectExpression.BindProperty(entityShaper.ValueBufferExpression, property);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            var @object = (SqlExpression)Visit(methodCallExpression.Object);
            var failed = TranslationFailed(methodCallExpression.Object, @object);
            var arguments = new SqlExpression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                arguments[i] = (SqlExpression)Visit(methodCallExpression.Arguments[i]);
                failed |= (methodCallExpression.Arguments[i] != null && arguments[i] == null);
            }

            return failed
                ? null
                : _methodCallTranslatorProvider.Translate(_model, @object, methodCallExpression.Method, arguments);
        }

        private (Expression, Expression) MatchCompare(Expression expression)
        {
            if (expression.Type == typeof(int)
                && expression is MethodCallExpression methodCall)
            {
                if (methodCall.Method.Name == "Compare"
                    && methodCall.Arguments.Count == 2
                    && methodCall.Arguments[0].Type == methodCall.Arguments[1].Type)
                {
                    return (methodCall.Arguments[0], methodCall.Arguments[1]);
                }
                else if (methodCall.Method.Name == "CompareTo"
                         && methodCall.Arguments.Count == 1
                         && methodCall.Object != null
                         && methodCall.Object.Type == methodCall.Arguments[0].Type)
                {
                    return (methodCall.Object, methodCall.Arguments[0]);
                }
            }

            return default;
        }

        private SqlExpression TranslateCompare(BinaryExpression binaryExpression)
        {
            if (!(binaryExpression.NodeType == ExpressionType.Equal
                || binaryExpression.NodeType == ExpressionType.NotEqual
                || binaryExpression.NodeType == ExpressionType.LessThan
                || binaryExpression.NodeType == ExpressionType.LessThanOrEqual
                || binaryExpression.NodeType == ExpressionType.GreaterThan
                || binaryExpression.NodeType == ExpressionType.GreaterThanOrEqual))
            {
                return null;
            }

            var constantValue = 0;
            Expression left = null;
            Expression right = null;

            if (binaryExpression.Left is ConstantExpression leftConstant
                && leftConstant.Type == typeof(int))
            {
                (right, left) = MatchCompare(binaryExpression.Right);
                constantValue = (int)leftConstant.Value;
            }
            else if (binaryExpression.Right is ConstantExpression rightConstant
                && rightConstant.Type == typeof(int))
            {
                (left, right) = MatchCompare(binaryExpression.Left);
                constantValue = (int)rightConstant.Value;
            }

            if (left != null)
            {
                var leftSql = (SqlExpression)Visit(left);
                var rightSql = (SqlExpression)Visit(right);
                var op = binaryExpression.NodeType;
                ExpressionType resultOp = default;
                if (leftSql != null && rightSql != null)
                {
                    // TODO: Can handle cases where value being compared is not -1,0,1
                    switch (constantValue)
                    {
                        case 0:
                            resultOp = op;
                            break;

                        case 1:
                            switch (op)
                            {
                                case ExpressionType.Equal:
                                case ExpressionType.GreaterThanOrEqual:
                                    resultOp = ExpressionType.GreaterThan;
                                    break;

                                case ExpressionType.NotEqual:
                                case ExpressionType.LessThan:
                                    resultOp = ExpressionType.LessThanOrEqual;
                                    break;

                                case ExpressionType.GreaterThan:
                                    return _sqlExpressionFactory.Constant(false);

                                case ExpressionType.LessThanOrEqual:
                                    return _sqlExpressionFactory.Constant(true);
                            }
                            break;

                        case -1:
                            switch (op)
                            {
                                case ExpressionType.Equal:
                                case ExpressionType.LessThanOrEqual:
                                    resultOp = ExpressionType.LessThan;
                                    break;

                                case ExpressionType.NotEqual:
                                case ExpressionType.GreaterThan:
                                    resultOp = ExpressionType.GreaterThanOrEqual;
                                    break;

                                case ExpressionType.LessThan:
                                    return _sqlExpressionFactory.Constant(false);

                                case ExpressionType.GreaterThanOrEqual:
                                    return _sqlExpressionFactory.Constant(true);
                            }
                            break;

                        default:
                            return null;
                    }
                }

                return _sqlExpressionFactory.MakeBinary(resultOp, leftSql, rightSql, null);
            }

            return null;
        }


        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            var comparison = TranslateCompare(binaryExpression);
            if (comparison != null)
            {
                return comparison;
            }

            var left = (SqlExpression)Visit(binaryExpression.Left);
            var right = (SqlExpression)Visit(binaryExpression.Right);

            if (TranslationFailed(binaryExpression.Left, left)
                || TranslationFailed(binaryExpression.Right, right))
            {
                return null;
            }

            return _sqlExpressionFactory.MakeBinary(
                binaryExpression.NodeType,
                left,
                right,
                null);
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
            => new SqlConstantExpression(constantExpression, null);

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
            => new SqlParameterExpression(parameterExpression, null);


        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is EntityShaperExpression)
            {
                return extensionExpression;
            }

            if (extensionExpression is ProjectionBindingExpression projectionBindingExpression)
            {
                return ((SelectExpression)projectionBindingExpression.QueryExpression)
                    .GetProjectionExpression(projectionBindingExpression.ProjectionMember);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            var test = (SqlExpression)Visit(conditionalExpression.Test);
            var ifTrue = (SqlExpression)Visit(conditionalExpression.IfTrue);
            var ifFalse = (SqlExpression)Visit(conditionalExpression.IfFalse);

            if (TranslationFailed(conditionalExpression.Test, test)
                || TranslationFailed(conditionalExpression.IfTrue, ifTrue)
                || TranslationFailed(conditionalExpression.IfFalse, ifFalse))
            {
                return null;
            }

            return _sqlExpressionFactory.Case(
                new[]
                {
                    new CaseWhenClause(test, ifTrue)
                },
                ifFalse);
        }

        //protected override Expression VisitNew(NewExpression newExpression)
        //{
        //    if (newExpression.Members == null
        //        || newExpression.Arguments.Count == 0)
        //    {
        //        return null;
        //    }

        //    var bindings = new Expression[newExpression.Arguments.Count];

        //    for (var i = 0; i < bindings.Length; i++)
        //    {
        //        var translation = Visit(newExpression.Arguments[i]);

        //        if (translation == null)
        //        {
        //            return null;
        //        }

        //        bindings[i] = translation;
        //    }

        //    return Expression.Constant(bindings);
        //}

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var operand = Visit(unaryExpression.Operand);

            if (TranslationFailed(unaryExpression.Operand, operand))
            {
                return null;
            }

            // In certain cases EF.Property would have convert node around the source.
            if (operand is EntityShaperExpression
                && unaryExpression.Type == typeof(object)
                && unaryExpression.NodeType == ExpressionType.Convert)
            {
                return operand;
            }

            var sqlOperand = (SqlExpression)operand;

            if (unaryExpression.NodeType == ExpressionType.Convert)
            {
                if (operand.Type.IsInterface
                        && unaryExpression.Type.GetInterfaces().Any(e => e == operand.Type)
                    || unaryExpression.Type.UnwrapNullableType() == operand.Type
                    || unaryExpression.Type.UnwrapNullableType() == typeof(Enum))
                {
                    return sqlOperand;
                }

                sqlOperand = _sqlExpressionFactory.ApplyDefaultTypeMapping(sqlOperand);

                return _sqlExpressionFactory.Convert(sqlOperand, unaryExpression.Type);
            }

            if (unaryExpression.NodeType == ExpressionType.Not)
            {
                return _sqlExpressionFactory.Not(sqlOperand);
            }

            return null;
        }

        private bool TranslationFailed(Expression original, Expression translation)
        {
            return original != null && translation == null;
        }
    }
}
