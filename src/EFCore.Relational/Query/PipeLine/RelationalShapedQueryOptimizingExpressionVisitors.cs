// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalShapedQueryOptimizingExpressionVisitors : ShapedQueryOptimizingExpressionVisitors
    {
        private QueryCompilationContext2 _queryCompilationContext;

        public RelationalShapedQueryOptimizingExpressionVisitors(QueryCompilationContext2 queryCompilationContext)
        {
            _queryCompilationContext = queryCompilationContext;
        }

        public override IEnumerable<ExpressionVisitor> GetVisitors()
        {
            yield return new SelectExpressionProjectionApplyingExpressionVisitor();
            yield return new NullComparisonTransformingExpressionVisitor();
        }
    }

    public class SelectExpressionProjectionApplyingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            if (node is SelectExpression selectExpression)
            {
                selectExpression.ApplyProjection();
            }

            return base.VisitExtension(node);
        }

    }
}
