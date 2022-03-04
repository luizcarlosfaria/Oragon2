using Microsoft.Extensions.Logging;

namespace LuizCarlosFaria.Oragon2.RingBuffer.Static;

public class StaticRingBufferAccquisitonController<T> : IAccquisitonController<T>
{
    private readonly StaticRingBuffer<T> ringBuffer;
    private readonly ILogger<StaticRingBuffer<T>> logger;
    private readonly TimeSpan waitTime;
    private readonly Func<T> factoryFunc;
    private readonly Func<T, bool> checkFunc;
    private readonly Action<T> disposeAction;

    internal StaticRingBufferAccquisitonController(StaticRingBuffer<T> ringBuffer, ILogger<StaticRingBuffer<T>> logger, TimeSpan waitTime, Func<T> factoryFunc, Func<T, bool> checkFunc, Action<T> disposeAction)
    {
        this.ringBuffer = ringBuffer;
        this.logger = logger;
        this.waitTime = waitTime;
        this.factoryFunc = factoryFunc;
        this.checkFunc = checkFunc;
        this.disposeAction = disposeAction;
        this.Instance = this.GetWorkerInstance();

    }

    private T GetWorkerInstance()
    {
        int tryCount = 0;
        T? buferedItem;
        while (this.ringBuffer.VirtualCount == 0 || this.ringBuffer.TryDequeue(out buferedItem) is false)
        {
            if (tryCount++ < Constants.MaxRetryTimes)
                throw new InvalidOperationException("Retry exceed 1000 times");
            this.logger.LogTrace("RingBuffer | Waiting... VirtualCount:{count} Capacity:{capacity}", this.ringBuffer.VirtualCount, this.ringBuffer.Capacity);

            Thread.Sleep(this.waitTime);

        }
        this.logger.LogTrace("RingBuffer | Acquired! VirtualCount:{count} Capacity:{capacity}", this.ringBuffer.VirtualCount, this.ringBuffer.Capacity);

        return buferedItem ?? throw new InvalidOperationException("BuferedItem is null");
    }

    public T Instance { get; }

    public void Dispose()
    {
        if (this.checkFunc(this.Instance))
            this.ringBuffer.Enqueue(this.Instance);
        else
        {
            this.ringBuffer.Enqueue(this.factoryFunc());

            this.logger.LogTrace("RingBuffer | Replacing....! VirtualCount:{count} Capacity:{capacity}", this.ringBuffer.VirtualCount, this.ringBuffer.Capacity);

            this.disposeAction(this.Instance);
        }
        GC.SuppressFinalize(this);
    }
}
