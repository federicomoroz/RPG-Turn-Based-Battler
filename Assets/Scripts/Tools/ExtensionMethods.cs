public static class ExtensionMethods 
{
    public static T SetMultipleCallbacks<T>(this T client, ref System.Action target, params System.Action[] callbacks) where T : class
    {
        for (int i = 0; i < callbacks.Length; i++)
            target += callbacks[i];

        return client;
    }
}
