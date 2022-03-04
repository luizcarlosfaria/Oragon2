namespace LuizCarlosFaria.Oragon2.RingBuffer;

public interface IRingBuffer<T>
{
    IAccquisitonController<T> Accquire();
}