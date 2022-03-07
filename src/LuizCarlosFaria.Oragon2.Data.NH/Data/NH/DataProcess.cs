using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuizCarlosFaria.Oragon2.Data.NH;

public abstract class DataProcess<EntityBase>
    where EntityBase : class
    //where Entity : class, EntityBase

{
    public DataContext Context { get; }

    protected DataProcess(DataContext dataContext)
    {
        this.Context = dataContext;
    }

    #region Public Methods

    /// <summary>
    ///     Reanexa um objeto
    /// </summary>
    /// <param name="itemToAttach"></param>
    public virtual void Attach(EntityBase itemToAttach)
    {
        this.Context.Session.Refresh(itemToAttach, LockMode.None);
    }

    public virtual bool IsAttached(object itemToAttach)
    {
        bool returnValue = this.Context.Session.Contains(itemToAttach);
        return returnValue;
    }

    #endregion Public Methods

    #region Protected Methods

    /// <summary>
    ///     Obtém IQueryOver pronto para realizar consultas usando lambda expressions
    /// </summary>
    /// <returns></returns>
    protected virtual IQueryable<T> Query<T>()
        where T : class, EntityBase
    {
        IQueryable<T> query = this.Context.Session.Query<T>();
        return query;
    }

    /// <summary>
    ///     Obtém IQueryOver (ICriteria API) para que possa ser utilizado em consultas com lambda expressions
    /// </summary>
    /// <returns></returns>
    protected virtual IQueryOver<T, T> QueryOver<T>()
        where T : class, EntityBase
    {
        IQueryOver<T, T> query = this.Context.Session.QueryOver<T>();
        return query;
    }

    #endregion Protected Methods

}
