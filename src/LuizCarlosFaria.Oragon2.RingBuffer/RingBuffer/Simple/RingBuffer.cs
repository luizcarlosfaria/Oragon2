using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LuizCarlosFaria.Oragon2.RingBuffer;

public partial class RingBuffer<T> : IRingBuffer<T>
{
    private Func<T> FactoryFunc { get; set; }
    private Func<T, bool> CheckFunc { get; set; }
    private Action<T> DisposeAction { get; set; }
    private ConcurrentQueue<T> Buffer { get; set; }
    private ILogger<RingBuffer<T>> Logger { get; set; }

    public RingBuffer(ILogger<RingBuffer<T>> logger, int capacity, Func<T> factoryFunc, Func<T, bool> checkFunc, Action<T> disposeAction) : this(logger, capacity, factoryFunc, checkFunc, disposeAction, TimeSpan.FromMilliseconds(50))
    {

    }

    public RingBuffer(ILogger<RingBuffer<T>> logger, int capacity, Func<T> factoryFunc, Func<T, bool> checkFunc, Action<T> disposeAction, TimeSpan waitTime)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger can't be null");
        this.Capacity = capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero");
        this.FactoryFunc = factoryFunc ?? throw new ArgumentNullException(nameof(factoryFunc), "factoryFunc can't be null");
        this.CheckFunc = checkFunc ?? throw new ArgumentNullException(nameof(checkFunc), "checkFunc can't be null");
        this.DisposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction), "disposeAction can't be null");
        this.WaitTime = waitTime.Ticks > 0 ? waitTime : throw new ArgumentOutOfRangeException(nameof(waitTime), "waitTime can't be null"); ;
        this.VirtualCount = 0;
        this.Buffer = new ConcurrentQueue<T>();

        for (int i = 1; i <= this.Capacity; i++)
        {
            this.Buffer.Enqueue(this.FactoryFunc());
            this.VirtualCount++;
        }
    }


    public int Capacity { get; }

    public TimeSpan WaitTime { get; }

    public int VirtualCount { get; private set; }

    internal bool TryDequeue(out T? result) => this.Buffer.TryDequeue(out result);

    internal void Enqueue(T item) => this.Buffer.Enqueue(item);

    public virtual IAccquisitonController<T> Accquire()
    {
        return new AccquisitonController<T>(this, this.Logger, this.WaitTime, this.FactoryFunc, this.CheckFunc, this.DisposeAction);
    }
}
