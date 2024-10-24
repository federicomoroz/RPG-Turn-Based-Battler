namespace Pool
{
    public interface IPoolable<T>
    {
        T Initialize();
        void Dispose();
    }
}

