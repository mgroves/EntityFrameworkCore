// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public abstract class SqlExpression : Expression
    {
        protected SqlExpression(Type type, RelationalTypeMapping typeMapping)
        {
            Type = type;
            TypeMapping = typeMapping;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type { get; }
        public RelationalTypeMapping TypeMapping { get; }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlExpression sqlExpression
                    && Equals(sqlExpression));

        private bool Equals(SqlExpression sqlExpression)
            => Type == sqlExpression.Type
            && TypeMapping?.Equals(sqlExpression.TypeMapping) == true;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Type.GetHashCode();
                hashCode = (hashCode * 397) ^ (TypeMapping?.GetHashCode() ?? 0);

                return hashCode;
            }
        }
    }
}
