namespace Pool
{
    public interface IPool<T>
    {
        T Pull();
        void Push(T item);
    }
}

