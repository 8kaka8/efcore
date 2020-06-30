// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


// ReSharper disable InconsistentNaming
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class TPTInheritanceQueryTestBase<TFixture> : InheritanceQueryTestBase<TFixture>
        where TFixture : TPTInheritanceQueryFixture, new()
    {
        public TPTInheritanceQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        // Keyless entities does not have TPT
        public override void Can_query_all_animal_views()
        {
        }

        // TPT does not have discriminator
        public override void Discriminator_used_when_projection_over_derived_type()
        {
        }

        // TPT does not have discriminator
        public override void Discriminator_used_when_projection_over_derived_type2()
        {
        }

        // TPT does not have discriminator
        public override void Discriminator_used_when_projection_over_of_type()
        {
        }

        // TPT does not have discriminator
        public override void Discriminator_with_cast_in_shadow_property()
        {
        }
    }
}
