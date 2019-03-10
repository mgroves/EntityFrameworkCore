// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Couchbase.Metadata.Internal
{
    public class CouchbaseEntityTypeBuilderAnnotations : CouchbaseEntityTypeAnnotations
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CouchbaseEntityTypeBuilderAnnotations(
            [NotNull] InternalEntityTypeBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new CouchbaseAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected new virtual CouchbaseAnnotationsBuilder Annotations => (CouchbaseAnnotationsBuilder)base.Annotations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalEntityTypeBuilder EntityTypeBuilder => (InternalEntityTypeBuilder)Annotations.MetadataBuilder;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override CouchbaseModelAnnotations GetAnnotations(IModel model)
            => new CouchbaseModelBuilderAnnotations(
                ((Model)model).Builder,
                Annotations.ConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override CouchbaseEntityTypeAnnotations GetAnnotations(IEntityType entityType)
            => new CouchbaseEntityTypeBuilderAnnotations(
                ((EntityType)entityType).Builder,
                Annotations.ConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ToContainer([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return SetContainerName(name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ToProperty([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return SetPropertyName(name);
        }
    }
}
