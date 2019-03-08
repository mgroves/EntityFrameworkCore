﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class SqliteNetTopologySuiteMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        public SqliteNetTopologySuiteMemberTranslatorPlugin(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            Translators = new IMemberTranslator[]
            {
                new SqliteCurveMemberTranslator(sqlExpressionFactory),
                new SqliteGeometryMemberTranslator(sqlExpressionFactory),
                new SqliteGeometryCollectionMemberTranslator(sqlExpressionFactory),
                new SqliteLineStringMemberTranslator(sqlExpressionFactory),
                new SqliteMultiCurveMemberTranslator(sqlExpressionFactory),
                new SqlitePointMemberTranslator(sqlExpressionFactory),
                new SqlitePolygonMemberTranslator(sqlExpressionFactory)
            };
        }


        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IMemberTranslator> Translators { get; }
    }
}
