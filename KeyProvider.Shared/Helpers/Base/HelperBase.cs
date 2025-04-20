using KeyProvider.Shared.Singleton;

namespace KeyProvider.Shared.Helpers.Base
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public abstract class HelperBase<T> : SingletonBase<T> where T : class, new()
    {
    }
}
