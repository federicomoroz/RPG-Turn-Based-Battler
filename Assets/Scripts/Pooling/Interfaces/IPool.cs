namespace Pool
{
    public interface IPool<T> where T : IPoolable<T>
    {
        T Pull();
        void Push(T item);
    }
}

