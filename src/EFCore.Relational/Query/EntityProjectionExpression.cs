// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         An expression that represents an entity in the projection of <see cref="SelectExpression"/>.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class EntityProjectionExpression : Expression
    {
        private readonly IDictionary<IProperty, ColumnExpression> _propertyExpressions = new Dictionary<IProperty, ColumnExpression>();
        private readonly IDictionary<INavigation, EntityShaperExpression> _navigationExpressions
            = new Dictionary<INavigation, EntityShaperExpression>();

        /// <summary>
        ///     Creates a new instance of the <see cref="EntityProjectionExpression" /> class.
        /// </summary>
        /// <param name="entityType"> The entity type to shape. </param>
        /// <param name="propertyExpressions"> A dictionary of column expressions corresponding to properties of the entity type. </param>
        /// <param name="discriminatorExpressions"> A dictionary of <see cref="SqlExpression"/> to discriminator each entity type in hierarchy. </param>
        public EntityProjectionExpression(
            [NotNull] IEntityType entityType,
            [NotNull] IDictionary<IProperty, ColumnExpression> propertyExpressions,
            [CanBeNull] IReadOnlyDictionary<IEntityType, SqlExpression> discriminatorExpressions = null)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(propertyExpressions, nameof(propertyExpressions));

            EntityType = entityType;
            _propertyExpressions = propertyExpressions;
            DiscriminatorExpressions = discriminatorExpressions;
        }

        /// <summary>
        ///     The entity type being projected out.
        /// </summary>
        public virtual IEntityType EntityType { get; }
        /// <summary>
        ///     Dictionary of discriminator expressions.
        /// </summary>
        public virtual IReadOnlyDictionary<IEntityType, SqlExpression> DiscriminatorExpressions { get; }
        /// <inheritdoc />
        public sealed override ExpressionType NodeType => ExpressionType.Extension;
        /// <inheritdoc />
        public override Type Type => EntityType.ClrType;

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var changed = false;
            var newCache = new Dictionary<IProperty, ColumnExpression>();
            foreach (var expression in _propertyExpressions)
            {
                var newExpression = (ColumnExpression)visitor.Visit(expression.Value);
                changed |= newExpression != expression.Value;

                newCache[expression.Key] = newExpression;
            }

            Dictionary<IEntityType, SqlExpression> newDiscriminators = null;
            if (DiscriminatorExpressions != null)
            {
                newDiscriminators = new Dictionary<IEntityType, SqlExpression>();
                foreach (var expression in DiscriminatorExpressions)
                {
                    var newExpression = (SqlExpression)visitor.Visit(expression.Value);
                    changed |= newExpression != expression.Value;

                    newDiscriminators[expression.Key] = newExpression;
                }
            }

            return changed
                ? new EntityProjectionExpression(EntityType, newCache, newDiscriminators)
                : this;
        }

        /// <summary>
        ///     Makes entity instance in projection nullable.
        /// </summary>
        /// <returns> A new entity projection expression which can project nullable entity. </returns>
        public virtual EntityProjectionExpression MakeNullable()
        {
            var newCache = new Dictionary<IProperty, ColumnExpression>();
            foreach (var expression in _propertyExpressions)
            {
                newCache[expression.Key] = expression.Value.MakeNullable();
            }

            return new EntityProjectionExpression(EntityType, newCache, DiscriminatorExpressions);
        }

        /// <summary>
        ///     Updates the entity type being projected out to one of the derived type.
        /// </summary>
        /// <param name="derivedType"> A derived entity type which should be projected. </param>
        /// <returns> A new entity projection expression which has the derived type being projected. </returns>
        public virtual EntityProjectionExpression UpdateEntityType([NotNull] IEntityType derivedType)
        {
            Check.NotNull(derivedType, nameof(derivedType));

            var propertyExpressionCache = new Dictionary<IProperty, ColumnExpression>();
            foreach (var kvp in _propertyExpressions)
            {
                var property = kvp.Key;
                if (derivedType.IsAssignableFrom(property.DeclaringEntityType)
                    || property.DeclaringEntityType.IsAssignableFrom(derivedType))
                {
                    propertyExpressionCache[property] = kvp.Value;
                }
            }

            Dictionary<IEntityType, SqlExpression> discriminatorExpressions = null;
            if (DiscriminatorExpressions != null)
            {
                discriminatorExpressions = new Dictionary<IEntityType, SqlExpression>();
                foreach (var kvp in DiscriminatorExpressions)
                {
                    var entityType = kvp.Key;
                    if (derivedType.IsAssignableFrom(entityType)
                        || entityType.IsAssignableFrom(derivedType))
                    {
                        discriminatorExpressions[entityType] = kvp.Value;
                    }
                }
            }

            return new EntityProjectionExpression(derivedType, propertyExpressionCache, discriminatorExpressions);
        }

        /// <summary>
        ///     Binds a property with this entity projection to get the SQL representation.
        /// </summary>
        /// <param name="property"> A property to bind. </param>
        /// <returns> A column which is a SQL representation of the property. </returns>
        public virtual ColumnExpression BindProperty([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
                && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityProjectionExpressionCalledWithIncorrectInterface(
                        "BindProperty",
                        "IProperty",
                        EntityType.DisplayName(),
                        property.Name));
            }

            return _propertyExpressions[property];
        }

        /// <summary>
        ///     Adds a navigation binding for this entity projection when the target entity type of the navigation is owned or weak.
        /// </summary>
        /// <param name="navigation"> A navigation to add binding for. </param>
        /// <param name="entityShaper"> An entity shaper expression for the target type. </param>
        public virtual void AddNavigationBinding([NotNull] INavigation navigation, [NotNull] EntityShaperExpression entityShaper)
        {
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(entityShaper, nameof(entityShaper));

            if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
                && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityProjectionExpressionCalledWithIncorrectInterface(
                        "AddNavigationBinding",
                        "INavigation",
                        EntityType.DisplayName(),
                        navigation.Name));
            }

            _navigationExpressions[navigation] = entityShaper;
        }

        /// <summary>
        ///     Binds a navigation with this entity projection to get entity shaper for the target entity type of the navigation which was
        ///     previously added using <see cref="AddNavigationBinding(INavigation, EntityShaperExpression)"/> method.
        /// </summary>
        /// <param name="navigation"> A navigation to bind. </param>
        /// <returns> An entity shaper expression for the target entity type of the navigation. </returns>
        public virtual EntityShaperExpression BindNavigation([NotNull] INavigation navigation)
        {
            Check.NotNull(navigation, nameof(navigation));

            if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
                && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityProjectionExpressionCalledWithIncorrectInterface(
                        "BindNavigation",
                        "INavigation",
                        EntityType.DisplayName(),
                        navigation.Name));
            }

            return _navigationExpressions.TryGetValue(navigation, out var expression)
                ? expression
                : null;
        }
    }
}
