using System.IO;
using UnityEngine;

public class JsonDataSaveService : ISaveService
{
    private string GetPath(string key)
    {
        return Path.Combine(Application.persistentDataPath, $"{key}.json");
    }

    public void Save<T>(T entity, string key)
    {
        var json = JsonUtility.ToJson(entity, true);
        File.WriteAllText(GetPath(key), json);
    }

    public T Load<T>(string key) where T : class
    {
        var path = GetPath(key);

        if (!File.Exists(path)) return default(T);

        var json = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }
}
