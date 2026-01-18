public interface ISaveService
{
    void Save<T>(T entity, string key);
    T Load<T>(string key) where T : class;
}
