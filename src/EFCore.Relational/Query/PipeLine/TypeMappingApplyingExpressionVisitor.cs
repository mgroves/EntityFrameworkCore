// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class TypeMappingApplyingExpressionVisitor : ITypeMappingApplyingExpressionVisitor
    {
        private readonly RelationalTypeMapping _boolTypeMapping;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public TypeMappingApplyingExpressionVisitor(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
            _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool));
        }

        public virtual SqlExpression ApplyTypeMapping(
            SqlExpression expression, RelationalTypeMapping typeMapping)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression.TypeMapping != null)
            // ColumnExpression, SqlNullExpression, SqlNotExpression should be captured here.
            {
                return expression;
            }

            switch (expression)
            {
                case LikeExpression likeExpression:
                    return ApplyTypeMappingOnLike(likeExpression, typeMapping);

                case SqlFragmentExpression sqlFragmentExpression:
                    return sqlFragmentExpression;

                case SqlFunctionExpression sqlFunctionExpression:
                    return ApplyTypeMappingOnSqlFunction(sqlFunctionExpression, typeMapping);

                default:
                    return ApplyTypeMappingOnExtension(expression, typeMapping);

            }
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlFunction(
            SqlFunctionExpression sqlFunctionExpression, RelationalTypeMapping typeMapping)
        {
            return sqlFunctionExpression.ApplyTypeMapping(typeMapping);
        }

        protected virtual SqlExpression ApplyTypeMappingOnLike(
            LikeExpression likeExpression, RelationalTypeMapping typeMapping)
        {
            var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(likeExpression.Match, likeExpression.Pattern);

            if (inferredTypeMapping == null)
            {
                throw new InvalidOperationException("TypeMapping should not be null.");
            }

            var match = ApplyTypeMapping(likeExpression.Match, inferredTypeMapping);
            var pattern = ApplyTypeMapping(likeExpression.Pattern, inferredTypeMapping);
            var escapeChar = ApplyTypeMapping(likeExpression.EscapeChar, inferredTypeMapping);

            return new LikeExpression(
                match,
                pattern,
                escapeChar,
                _boolTypeMapping);
        }

        protected virtual SqlExpression ApplyTypeMappingOnExtension(
            SqlExpression expression, RelationalTypeMapping typeMapping)
        {
            return expression;
        }
    }
}
