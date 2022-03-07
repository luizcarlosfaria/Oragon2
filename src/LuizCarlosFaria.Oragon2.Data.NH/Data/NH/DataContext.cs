using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuizCarlosFaria.Oragon2.Data.NH;

public class DataContext : IDisposable
{
    private bool disposedValue;

    public NHibernate.ISession Session { get; private set; }

    public NHibernate.ITransaction? Transaction { get; private set; }

    public IsolationLevel? IsolationLevel { get; private set; }

    public bool FlushRequested { get; private set; }

    public DataContext(NHibernate.ISession session)
    {
        this.Session = session;
        this.FlushRequested = false;
    }

    public void EnsureTransaction(IsolationLevel? isolationLevel = null)
    {
        if (this.Transaction == null)
        {
            if (isolationLevel == null)
            {
                this.Transaction = this.Session.BeginTransaction();
            }
            else
            {
                this.Transaction = this.Session.BeginTransaction(isolationLevel.Value);
                this.IsolationLevel = isolationLevel.Value;
            }
        }
    }

    public void Flush(bool imediate = false)
    {
        if (imediate)
        {
            this.Session.Flush();
        }
        else
        {
            this.FlushRequested = true;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                if (this.FlushRequested)
                    this.Session.Flush();

                if (this.Transaction != null)
                {
                    this.Transaction.Dispose();
                    this.Transaction = null;
                }

                if (this.Session != null)
                {
                    this.Session.Dispose();
                    this.Session = null!;
                }
            }
            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
