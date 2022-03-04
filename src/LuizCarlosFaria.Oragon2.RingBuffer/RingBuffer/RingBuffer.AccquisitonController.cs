using Microsoft.Extensions.Logging;

namespace LuizCarlosFaria.Oragon2.RingBuffer;

public partial class RingBuffer<T>
{
    public class AccquisitonController : IAccquisitonController<T>
    {
        private readonly RingBuffer<T> ringBuffer;
        private readonly ILogger<RingBuffer<T>> logger;
        private readonly TimeSpan waitTime;

        internal AccquisitonController(RingBuffer<T> ringBuffer, ILogger<RingBuffer<T>> logger, TimeSpan waitTime)
        {
            this.ringBuffer = ringBuffer;
            this.logger = logger;
            this.waitTime = waitTime;
            this.Instance = this.GetWorkerInstance();

        }

        private T GetWorkerInstance()
        {
            int tryCount = 0;
            T? buferedItem;
            while (this.ringBuffer.VirtualCount == 0 || this.ringBuffer.buffer.TryDequeue(out buferedItem) is false)
            {
                if (tryCount++ < Constants.MaxRetryTimes)
                {
                    throw new InvalidOperationException("Retry exceed 1000 times");
                }
                this.logger.LogTrace($"RingBuffer | Waiting.. VirtualCount:{this.ringBuffer.VirtualCount} Capacity:{this.ringBuffer.Capacity}");

                Thread.Sleep(this.waitTime);

            }
            this.logger.LogTrace($"RingBuffer | Acquired! VirtualCount:{this.ringBuffer.VirtualCount} Capacity:{this.ringBuffer.Capacity}");

            return buferedItem ?? throw new InvalidOperationException("BuferedItem is null");
        }

        public T Instance { get; }

        public void Dispose()
        {
            if (this.ringBuffer.checkFunc(this.Instance))
            {
                this.ringBuffer.buffer.Enqueue(this.Instance);
            }
            else
            {
                this.ringBuffer.buffer.Enqueue(this.ringBuffer.factoryFunc());
                
                this.logger.LogTrace($"RingBuffer | Replacing....! VirtualCount:{this.ringBuffer.VirtualCount} Capacity:{this.ringBuffer.Capacity}");

                this.ringBuffer.disposeAction(this.Instance);
            }
            GC.SuppressFinalize(this);
        }
    }
}
