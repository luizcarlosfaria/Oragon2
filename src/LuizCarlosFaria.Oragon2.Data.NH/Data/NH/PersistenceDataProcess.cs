using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuizCarlosFaria.Oragon2.Data.NH;

public class PersistenceDataProcess<T> : DataProcess<T>
    where T : class
{
    public PersistenceDataProcess(DataContext dataContext) : base(dataContext)
    {
    }

    /// <summary>
    ///     Realiza a exclusão da entidade do banco
    /// </summary>
    /// <param name="entity">Entidade a ser persistida</param>
    public virtual void Delete(T entity, bool flushImediate = false)
    {
        this.Context.Session.Delete(entity);
        this.Context.Flush(imediate: flushImediate);
    }

    /// <summary>
    ///     Realiza a operação de insert dos dados de uma entidade no banco de dados
    /// </summary>
    /// <param name="entity">Entidade a ser persistida</param>
    public virtual void Save(T entity, bool flushImediate = false)
    {
        this.Context.Session.Save(entity);
        this.Context.Flush(imediate: flushImediate);
    }

    /// <summary>
    ///     Tenta realizar um insert ou update para garantir o armazenamento do dado no banco
    /// </summary>
    /// <param name="entity">Entidade a ser persistida</param>
    public virtual void SaveOrUpdate(T entity, bool flushImediate = false)
    {
        this.Context.Session.SaveOrUpdate(entity);
        this.Context.Flush(imediate: flushImediate);
    }

    /// <summary>
    ///     Realiza uma operãção de update no banco
    /// </summary>
    /// <param name="entity">Entidade a ser persistida</param>
    public virtual void Update(T entity, bool flushImediate = false)
    {
        this.Context.Session.Update(entity);
        this.Context.Flush(imediate: flushImediate);
    }
}
