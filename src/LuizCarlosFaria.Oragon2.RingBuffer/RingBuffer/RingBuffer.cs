using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LuizCarlosFaria.Oragon2.RingBuffer;

public partial class RingBuffer<T> : IRingBuffer<T>
{
    private readonly Func<T> factoryFunc;
    private readonly Func<T, bool> checkFunc;
    private readonly Action<T> disposeAction;
    protected readonly ConcurrentQueue<T> buffer;
    private readonly ILogger<RingBuffer<T>> logger;

    public RingBuffer(ILogger<RingBuffer<T>> logger, int capacity, Func<T> factoryFunc, Func<T, bool> checkFunc, Action<T> disposeAction) : this(logger, capacity, factoryFunc, checkFunc, disposeAction, TimeSpan.FromMilliseconds(50))
    {

    }

    public RingBuffer(ILogger<RingBuffer<T>> logger, int capacity, Func<T> factoryFunc, Func<T, bool> checkFunc, Action<T> disposeAction, TimeSpan waitTime)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger can't be null");
        this.Capacity = capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero");
        this.factoryFunc = factoryFunc ?? throw new ArgumentNullException(nameof(factoryFunc), "factoryFunc can't be null");
        this.checkFunc = checkFunc ?? throw new ArgumentNullException(nameof(checkFunc), "checkFunc can't be null");
        this.disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction), "disposeAction can't be null");
        this.WaitTime = waitTime.Ticks > 0 ? waitTime : throw new ArgumentOutOfRangeException(nameof(waitTime), "waitTime can't be null"); ;
        this.VirtualCount = 0;
        this.buffer = new ConcurrentQueue<T>();

        for (int i = 1; i <= this.Capacity; i++)
        {
            this.buffer.Enqueue(this.factoryFunc());
            this.VirtualCount++;
        }
    }


    public int Capacity { get; }

    public TimeSpan WaitTime { get; }

    public int VirtualCount { get; private set; }

    public virtual IAccquisitonController<T> Accquire()
    {
        return new AccquisitonController(this, this.logger, this.WaitTime);
    }
}
