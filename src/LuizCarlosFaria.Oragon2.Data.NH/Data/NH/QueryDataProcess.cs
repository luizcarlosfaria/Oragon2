using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace LuizCarlosFaria.Oragon2.Data.NH;

public class QueryDataProcess<EntityBase, T> : DataProcess<EntityBase>
    where EntityBase : class
    where T : class, EntityBase
{
    public QueryDataProcess(DataContext dataContext) : base(dataContext)
    {
    }

    #region Public Properties

    public bool UseCacheByDefault { get; set; }

    #endregion Public Properties

    #region Public Methods

    /// <summary>
    ///     Obtém uma única instância de T com base no filtro informado.
    /// </summary>
    /// <param name="predicate">Filtro a ser aplicado.</param>
    /// <returns>Primeira ocorrência de T que atenda ao filtro.</returns>
    public T GetFirstBy(Expression<Func<T, bool>> predicate)
    {
        return this.GetFirstBy(predicate, this.UseCacheByDefault);
    }

    /// <summary>
    ///     Obtém uma única instância de T com base no filtro informado.
    /// </summary>
    /// <param name="predicate">Filtro a ser aplicado.</param>
    /// <param name="cacheable">Informa para a consulta que pode esta pode ser cacheada</param>
    /// <returns>Primeira ocorrência de T que atenda ao filtro.</returns>
    public T GetFirstBy(Expression<Func<T, bool>> predicate, bool cacheable)
    {
        IQueryOver<T> queryOver = this.Context.Session.QueryOver<T>().Where(predicate);
        if (cacheable)
            queryOver.Cacheable().CacheMode(CacheMode.Normal);
        return queryOver.Take(1).SingleOrDefault();
    }

    /// <summary>
    ///     Obtém uma única instância de T com base no filtro informado.
    /// </summary>
    /// <param name="predicate">Filtro a ser aplicado.</param>
    /// <returns>Primeira ocorrência de T que atenda ao filtro.</returns>
    public T GetSingleBy(Expression<Func<T, bool>> predicate)
    {
        return this.GetSingleBy(predicate, this.UseCacheByDefault);
    }

    /// <summary>
    ///     Obtém uma única instância de T com base no filtro informado.
    /// </summary>
    /// <param name="predicate">Filtro a ser aplicado.</param>
    /// <param name="cacheable">Informa para a consulta que pode esta pode ser cacheada</param>
    /// <returns>Primeira ocorrência de T que atenda ao filtro.</returns>
    public T GetSingleBy(Expression<Func<T, bool>> predicate, bool cacheable)
    {
        IQueryOver<T> queryOver = this.Context.Session.QueryOver<T>().Where(predicate);
        if (cacheable)
            queryOver.Cacheable().CacheMode(CacheMode.Normal);
        return queryOver.SingleOrDefault();
    }

    /// <summary>
    ///     Obtém uma lista de instâncias de T com base no filtro informado.
    /// </summary>
    /// <param name="predicate">Filtro a ser aplicado.</param>
    /// <param name="orderByExpression">Identifica o orderby a ser executado na consulta</param>
    /// <param name="cacheable">Informa para a consulta que pode esta pode ser cacheada</param>
    /// <returns>Lista com ocorrência de T que atendem ao filtro.</returns>
    public IList<T> GetListBy(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderByExpression = null, bool? cacheable = null)
    {
        if (cacheable == null) cacheable = this.UseCacheByDefault;

        IQueryOver<T, T> queryOver = this.Context.Session.QueryOver<T>();

        if (predicate != null) queryOver = queryOver.Where(predicate);

        if (orderByExpression != null) queryOver = queryOver.OrderBy(orderByExpression).Asc;

        if (cacheable.Value) queryOver.Cacheable().CacheMode(CacheMode.Normal);

        return queryOver.List();
    }

    #endregion Public Methods

}
