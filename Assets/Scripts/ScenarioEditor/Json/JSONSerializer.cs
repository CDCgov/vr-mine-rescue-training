using System;
using System.IO;
using UnityEngine;

public class JSONSerializer : MonoBehaviour
{
    public static T Deserialize<T>(string jsonString)
    {
        try
        {
            return JsonUtility.FromJson<T>(jsonString);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message + " | " + typeof(T).ToString() + " | " + jsonString);
        }
    }

    public static string Serialize<T>(T data, string filePath = null)
    {
        if (filePath != null)
        {
            string path = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string jsonString = JsonUtility.ToJson(data);
            File.WriteAllText(filePath + ".json", jsonString);
            return null;
        }
        else
        {
            return JsonUtility.ToJson(data);
        }

    }

}