using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LuizCarlosFaria.Oragon2.Data.NH;

public static partial class NHibernateMappingsExtensions
{
    public static ClassMapping<TEntity> Configure<TEntity>(this ClassMapping<TEntity> mapping, string table, string? schema = null)
        where TEntity : class
    {
        if (schema != null)
            mapping.Schema(schema);

        mapping.Table(table);

        return mapping;
    }

    public static ClassMapping<TEntity> MapId<TEntity>(this ClassMapping<TEntity> mapping, Expression<Func<TEntity, Guid>> property, string? columnName = null, IGeneratorDef? generator = null)
        where TEntity : class
    {
        generator ??= Generators.Guid;

        columnName ??= ((MemberExpression)property.Body).Member.Name;

        mapping.Id(property, map =>
        {
            map.Column(columnName);
            map.Generator(generator);
        });

        return mapping;
    }

    public static ClassMapping<TEntity> MapString<TEntity>(this ClassMapping<TEntity> mapping, Expression<Func<TEntity, string>> property, bool nullable, int length, string? columnName = null)
        where TEntity : class
    {
        mapping.Property(property, map =>
        {
            map.NotNullable(!nullable);
            map.Length(length);

            if (columnName != null)
                map.Column(columnName);
        });

        return mapping;
    }

    public static ClassMapping<TEntity> MapClob<TEntity>(this ClassMapping<TEntity> mapping, Expression<Func<TEntity, string>> property, bool nullable, string? columnName = null)
        where TEntity : class
    {
        columnName ??= ((MemberExpression)property.Body).Member.Name;

        mapping.Property(property, map =>
        {
            map.NotNullable(!nullable);
            map.Length(1073741823); //TODO: magic number Text para Postgresql
            map.Lazy(true);
            map.Column(columnName);
        });

        return mapping;
    }

    public static IComponentMapper<TEntity> MapClob<TEntity>(this IComponentMapper<TEntity> mapping, Expression<Func<TEntity, string>> property, bool nullable, string? columnName = null)
    //where TEntity : class
    {
        columnName ??= ((MemberExpression)property.Body).Member.Name;

        mapping.Property(property, map =>
        {
            map.NotNullable(!nullable);
            map.Length(1073741823); //TODO: magic number Text para Postgresql
            map.Lazy(true);
            map.Column(columnName);
        });

        return mapping;
    }

    public static ClassMapping<TEntity> MapBool<TEntity>(this ClassMapping<TEntity> mapping, Expression<Func<TEntity, bool>> property, bool nullable, string? columnName = null)
       where TEntity : class
    {
        mapping.Property(property, map =>
        {
            map.NotNullable(!nullable);

            if (columnName != null)
                map.Column(columnName);
        });

        return mapping;
    }

    public static ClassMapping<TEntity> MapByte<TEntity>(this ClassMapping<TEntity> mapping, Expression<Func<TEntity, byte>> property, bool nullable, string? columnName = null)
       where TEntity : class
    {
        mapping.Property(property, map =>
        {
            map.NotNullable(!nullable);

            if (columnName != null)
                map.Column(columnName);
        });

        return mapping;
    }

    public static ClassMapping<TEntity> MapShort<TEntity>(this ClassMapping<TEntity> mapping, Expression<Func<TEntity, short>> property, bool nullable, string? columnName = null)
       where TEntity : class
    {
        mapping.Property(property, map =>
        {
            map.NotNullable(!nullable);

            if (columnName != null)
                map.Column(columnName);
        });

        return mapping;
    }

    public static ClassMapping<TEntity> MapInt<TEntity>(this ClassMapping<TEntity> mapping, Expression<Func<TEntity, int>> property, bool nullable, string? columnName = null)
       where TEntity : class
    {
        mapping.Property(property, map =>
        {
            map.NotNullable(!nullable);

            if (columnName != null)
                map.Column(columnName);
        });

        return mapping;
    }

    public static ClassMapping<TEntity> MapLong<TEntity>(this ClassMapping<TEntity> mapping, Expression<Func<TEntity, long>> property, bool nullable, string? columnName = null)
       where TEntity : class
    {
        mapping.Property(property, map =>
        {
            map.NotNullable(!nullable);

            if (columnName != null)
                map.Column(columnName);
        });

        return mapping;
    }

    public static ClassMapping<TEntity> MapDecimal<TEntity>(this ClassMapping<TEntity> mapping, Expression<Func<TEntity, decimal>> property, bool nullable, string? columnName = null)
    where TEntity : class
    {
        mapping.Property(property, map =>
        {
            map.NotNullable(!nullable);

            if (columnName != null)
                map.Column(columnName);
        });

        return mapping;
    }


    public static ClassMapping<TEntity> MapManyToOne<TProperty, TEntity>(this ClassMapping<TEntity> mapping, Expression<Func<TEntity, TProperty>> property, bool nullable, string? FKName = null, string? columnName = null)
       where TProperty : class
       where TEntity : class
    {
        mapping.ManyToOne(property, map =>
        {
            if (FKName == null)
            {
                var currentEntityType = typeof(TEntity);
                var referenceEntityType = typeof(TProperty);
                map.ForeignKey($"FK_{referenceEntityType.Name}_TO_{currentEntityType.Name}");
            }
            else
            {
                map.ForeignKey(FKName);
            }

            if (columnName == null)
            {
                var currentEntityType = typeof(TEntity);
                var referenceEntityType = typeof(TProperty);
                map.Column($"{referenceEntityType.Name}Id");
            }
            else
            {
                map.Column(columnName);
            }

            map.NotNullable(!nullable);
        });

        return mapping;
    }
}
