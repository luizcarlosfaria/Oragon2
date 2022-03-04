namespace LuizCarlosFaria.Oragon2.RingBuffer;

public interface IAccquisitonController<out T> : IDisposable
{
    T Instance { get; }
}
