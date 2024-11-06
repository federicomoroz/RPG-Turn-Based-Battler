namespace Utils
{
    public static class ExtensionMethods 
    {
        public static T RegisterMultipleCallbacks<T>(this T client, ref System.Action target, params System.Action[] callbacks) where T : class
        {
            for (int i = 0; i < callbacks.Length; i++)
                target += callbacks[i];

            return client;
        }
        public static T RegisterMultipleCallbacks<T>(this T client, ref System.Action<float, float> target, params System.Action<float, float>[] callbacks) where T : class
        {
            for (int i = 0; i < callbacks.Length; i++)
                target += callbacks[i];

            return client;
        }
    }
}
