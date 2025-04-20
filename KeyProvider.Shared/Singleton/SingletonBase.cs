namespace KeyProvider.Shared.Singleton
{
    public class SingletonBase<T> where T : class, new()
    {
        private static T? instanceObject;
        private static readonly object instanceLock = new();

        public static T GetInstance
        {
            get
            {
                if (instanceObject == null)
                {
                    lock (instanceLock)
                    {
                        instanceObject = Activator.CreateInstance<T>();
                    }
                }

                return instanceObject;
            }
        }
    }
}
