// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class TPTInheritanceQuerySqlServerTest : TPTInheritanceQueryTestBase<TPTInheritanceQuerySqlServerFixture>
    {
        public TPTInheritanceQuerySqlServerTest(TPTInheritanceQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Byte_enum_value_constant_used_in_projection()
        {
            base.Byte_enum_value_constant_used_in_projection();

            AssertSql(
                @"SELECT CASE
    WHEN [b].[IsFlightless] = CAST(1 AS bit) THEN CAST(0 AS tinyint)
    ELSE CAST(1 AS tinyint)
END
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]");
        }

        public override void Can_filter_all_animals()
        {
            base.Can_filter_all_animals();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEagle], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[Name] = N'Great spotted kiwi'
ORDER BY [a].[Species]");
        }

        public override void Can_include_animals()
        {
            base.Can_include_animals();

            AssertSql(
                @"SELECT [c].[Id], [c].[Name], [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t].[FoundOn], [t].[IsEagle], [t].[IsKiwi]
FROM [Country] AS [c]
LEFT JOIN (
    SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
        WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsEagle], CASE
        WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsKiwi]
    FROM [Animals] AS [a]
    LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
    LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
    LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
) AS [t] ON [c].[Id] = [t].[CountryId]
ORDER BY [c].[Name], [c].[Id], [t].[Species]");
        }

        public override void Can_include_prey()
        {
            base.Can_include_prey();

            AssertSql(
                @"SELECT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[Group], [t0].[Species], [t0].[CountryId], [t0].[Name], [t0].[EagleId], [t0].[IsFlightless], [t0].[Group], [t0].[FoundOn], [t0].[IsEagle], [t0].[IsKiwi]
FROM (
    SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group]
    FROM [Animals] AS [a]
    INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
    INNER JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
) AS [t]
LEFT JOIN (
    SELECT [a0].[Species], [a0].[CountryId], [a0].[Name], [b0].[EagleId], [b0].[IsFlightless], [e0].[Group], [k].[FoundOn], CASE
        WHEN [e0].[Species] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsEagle], CASE
        WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsKiwi]
    FROM [Animals] AS [a0]
    INNER JOIN [Birds] AS [b0] ON [a0].[Species] = [b0].[Species]
    LEFT JOIN [Eagle] AS [e0] ON [a0].[Species] = [e0].[Species]
    LEFT JOIN [Kiwi] AS [k] ON [a0].[Species] = [k].[Species]
) AS [t0] ON [t].[Species] = [t0].[EagleId]
ORDER BY [t].[Species], [t0].[Species]");
        }

        public override void Can_insert_update_delete()
        {
            base.Can_insert_update_delete();

            AssertSql(
                @"SELECT TOP(2) [c].[Id], [c].[Name]
FROM [Country] AS [c]
WHERE [c].[Id] = 1",
                //
                @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)
@p1='1'
@p2='Little spotted kiwi' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Animals] ([Species], [CountryId], [Name])
VALUES (@p0, @p1, @p2);",
                //
                @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)
@p1=NULL (Size = 100)
@p2='True'

SET NOCOUNT ON;
INSERT INTO [Birds] ([Species], [EagleId], [IsFlightless])
VALUES (@p0, @p1, @p2);",
                //
                @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)
@p1='0' (Size = 1)

SET NOCOUNT ON;
INSERT INTO [Kiwi] ([Species], [FoundOn])
VALUES (@p0, @p1);",
                //
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[Species] LIKE N'%owenii'",
                //
                @"@p1='Apteryx owenii' (Nullable = false) (Size = 100)
@p0='Aquila chrysaetos canadensis' (Size = 100)

SET NOCOUNT ON;
UPDATE [Birds] SET [EagleId] = @p0
WHERE [Species] = @p1;
SELECT @@ROWCOUNT;",
                //
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[Species] LIKE N'%owenii'",
                //
                @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)

SET NOCOUNT ON;
DELETE FROM [Animals]
WHERE [Species] = @p0;
SELECT @@ROWCOUNT;",
                //
                @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)

SET NOCOUNT ON;
DELETE FROM [Birds]
WHERE [Species] = @p0;
SELECT @@ROWCOUNT;",
                //
                @"@p0='Apteryx owenii' (Nullable = false) (Size = 100)

SET NOCOUNT ON;
DELETE FROM [Kiwi]
WHERE [Species] = @p0;
SELECT @@ROWCOUNT;",
                //
                @"SELECT COUNT(*)
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [a].[Species] LIKE N'%owenii'");
        }

        public override void Can_query_all_animals()
        {
            base.Can_query_all_animals();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEagle], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
ORDER BY [a].[Species]");
        }

        public override void Can_query_all_birds()
        {
            base.Can_query_all_birds();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEagle], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
ORDER BY [a].[Species]");
        }

        public override void Can_query_all_plants()
        {
            base.Can_query_all_plants();

            AssertSql(
                @"SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [r].[HasThorns], CASE
    WHEN [d].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsDaisy], CASE
    WHEN [r].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsRose]
FROM [Plants] AS [p]
LEFT JOIN [Flowers] AS [f] ON [p].[Species] = [f].[Species]
LEFT JOIN [Daisies] AS [d] ON [p].[Species] = [d].[Species]
LEFT JOIN [Roses] AS [r] ON [p].[Species] = [r].[Species]
ORDER BY [p].[Species]");
        }

        public override void Can_query_all_types_when_shared_column()
        {
            base.Can_query_all_types_when_shared_column();

            AssertSql(
                @"SELECT [d].[Id], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams], [l].[LiltCO2], [l].[SugarGrams], [t].[CaffeineGrams], [t].[HasMilk], CASE
    WHEN [c].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsCoke], CASE
    WHEN [l].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsLilt], CASE
    WHEN [t].[Id] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsTea]
FROM [Drinks] AS [d]
LEFT JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]
LEFT JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]
LEFT JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]");
        }

        public override void Can_query_just_kiwis()
        {
            base.Can_query_just_kiwis();

            AssertSql(
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]");
        }

        public override void Can_query_just_roses()
        {
            base.Can_query_just_roses();

            AssertSql(
                @"SELECT TOP(2) [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [r].[HasThorns]
FROM [Plants] AS [p]
INNER JOIN [Flowers] AS [f] ON [p].[Species] = [f].[Species]
INNER JOIN [Roses] AS [r] ON [p].[Species] = [r].[Species]");
        }

        public override void Can_query_when_shared_column()
        {
            base.Can_query_when_shared_column();

            AssertSql(
                @"SELECT TOP(2) [d].[Id], [c].[CaffeineGrams], [c].[CokeCO2], [c].[SugarGrams]
FROM [Drinks] AS [d]
INNER JOIN [Coke] AS [c] ON [d].[Id] = [c].[Id]",
                //
                @"SELECT TOP(2) [d].[Id], [l].[LiltCO2], [l].[SugarGrams]
FROM [Drinks] AS [d]
INNER JOIN [Lilt] AS [l] ON [d].[Id] = [l].[Id]",
                //
                @"SELECT TOP(2) [d].[Id], [t].[CaffeineGrams], [t].[HasMilk]
FROM [Drinks] AS [d]
INNER JOIN [Tea] AS [t] ON [d].[Id] = [t].[Id]");
        }

        public override void Can_use_backwards_is_animal()
        {
            base.Can_use_backwards_is_animal();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]");
        }

        public override void Can_use_backwards_of_type_animal()
        {
            base.Can_use_backwards_of_type_animal();

            AssertSql(" ");
        }

        public override void Can_use_is_kiwi()
        {
            base.Can_use_is_kiwi();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEagle], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL");
        }

        public override void Can_use_is_kiwi_in_projection()
        {
            base.Can_use_is_kiwi_in_projection();

            AssertSql(
                @"SELECT CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]");
        }

        public override void Can_use_is_kiwi_with_other_predicate()
        {
            base.Can_use_is_kiwi_with_other_predicate();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEagle], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL AND ([a].[CountryId] = 1)");
        }

        public override void Can_use_of_type_animal()
        {
            base.Can_use_of_type_animal();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEagle], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
ORDER BY [a].[Species]");
        }

        public override void Can_use_of_type_bird()
        {
            base.Can_use_of_type_bird();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEagle], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [e].[Species] IS NOT NULL OR [k].[Species] IS NOT NULL
ORDER BY [a].[Species]");
        }

        public override void Can_use_of_type_bird_first()
        {
            base.Can_use_of_type_bird_first();

            AssertSql(
                @"SELECT TOP(1) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEagle], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [e].[Species] IS NOT NULL OR [k].[Species] IS NOT NULL
ORDER BY [a].[Species]");
        }

        public override void Can_use_of_type_bird_predicate()
        {
            base.Can_use_of_type_bird_predicate();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
    WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsEagle], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE ([a].[CountryId] = 1) AND ([e].[Species] IS NOT NULL OR [k].[Species] IS NOT NULL)
ORDER BY [a].[Species]");
        }

        public override void Can_use_of_type_bird_with_projection()
        {
            base.Can_use_of_type_bird_with_projection();

            AssertSql(
                @"SELECT [b].[EagleId]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [e].[Species] IS NOT NULL OR [k].[Species] IS NOT NULL");
        }

        public override void Can_use_of_type_kiwi()
        {
            base.Can_use_of_type_kiwi();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL");
        }

        public override void Can_use_of_type_kiwi_where_north_on_derived_property()
        {
            base.Can_use_of_type_kiwi_where_north_on_derived_property();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL AND ([k].[FoundOn] = CAST(0 AS tinyint))");
        }

        public override void Can_use_of_type_kiwi_where_south_on_derived_property()
        {
            base.Can_use_of_type_kiwi_where_south_on_derived_property();

            AssertSql(
                @"SELECT [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn], CASE
    WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsKiwi]
FROM [Animals] AS [a]
LEFT JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
WHERE [k].[Species] IS NOT NULL AND ([k].[FoundOn] = CAST(1 AS tinyint))");
        }

        public override void Can_use_of_type_rose()
        {
            base.Can_use_of_type_rose();

            AssertSql(
                @"SELECT [p].[Species], [p].[CountryId], [p].[Genus], [p].[Name], [r].[HasThorns], CASE
    WHEN [r].[Species] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsRose]
FROM [Plants] AS [p]
LEFT JOIN [Flowers] AS [f] ON [p].[Species] = [f].[Species]
LEFT JOIN [Daisies] AS [d] ON [p].[Species] = [d].[Species]
LEFT JOIN [Roses] AS [r] ON [p].[Species] = [r].[Species]
WHERE [r].[Species] IS NOT NULL");
        }

        public override void Member_access_on_intermediate_type_works()
        {
            base.Member_access_on_intermediate_type_works();

            AssertSql(
                @"SELECT [a].[Name]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
ORDER BY [a].[Name]");
        }

        public override void OfType_Union_OfType()
        {
            base.OfType_Union_OfType();

            AssertSql(" ");
        }

        public override void OfType_Union_subquery()
        {
            base.OfType_Union_subquery();

            AssertSql(" ");
        }

        public override void Setting_foreign_key_to_a_different_type_throws()
        {
            base.Setting_foreign_key_to_a_different_type_throws();

            AssertSql(
                @"SELECT TOP(2) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [k].[FoundOn]
FROM [Animals] AS [a]
INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
INNER JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]",
                //
                @"@p0='Haliaeetus leucocephalus' (Nullable = false) (Size = 100)
@p1='0'
@p2='Bald eagle' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [Animals] ([Species], [CountryId], [Name])
VALUES (@p0, @p1, @p2);");
        }

        public override void Subquery_OfType()
        {
            base.Subquery_OfType();

            AssertSql(
                @"@__p_0='5'

SELECT DISTINCT [t].[Species], [t].[CountryId], [t].[Name], [t].[EagleId], [t].[IsFlightless], [t].[FoundOn], [t].[IsKiwi]
FROM (
    SELECT TOP(@__p_0) [a].[Species], [a].[CountryId], [a].[Name], [b].[EagleId], [b].[IsFlightless], [e].[Group], [k].[FoundOn], CASE
        WHEN [e].[Species] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsEagle], CASE
        WHEN [k].[Species] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsKiwi]
    FROM [Animals] AS [a]
    INNER JOIN [Birds] AS [b] ON [a].[Species] = [b].[Species]
    LEFT JOIN [Eagle] AS [e] ON [a].[Species] = [e].[Species]
    LEFT JOIN [Kiwi] AS [k] ON [a].[Species] = [k].[Species]
) AS [t]
WHERE [t].[IsKiwi] = CAST(1 AS bit)");
        }

        public override void Union_entity_equality()
        {
            base.Union_entity_equality();

            AssertSql(" ");
        }

        public override void Union_siblings_with_duplicate_property_in_subquery()
        {
            base.Union_siblings_with_duplicate_property_in_subquery();

            AssertSql(" ");
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
