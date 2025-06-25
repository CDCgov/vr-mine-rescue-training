using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JSONFileManagement : MonoBehaviour
{
    /// <summary>
    /// Returns string of json data from file at filepath parameter if the file exists.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string LoadJSONAsString(string filePath)
    {
        string data = string.Empty;

        try
        {
            if(!filePath.Contains(".json")) { filePath = filePath + ".json"; }
            using (StreamReader streamReader = File.OpenText(filePath))
            {
                string str = string.Empty;
                while ((str = streamReader.ReadLine()) != null)
                {
                    data += str;
                }
            }
            return data;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

    /// <summary>
    /// Returns list of filenames at provided path parameter if any files exist.
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    public static List<string> GetJSONFileNames(string directoryPath)
    {
        List<string> fileNames = new List<string>();
        string[] filesAtPath = Directory.GetFiles(directoryPath, "*.json");

        for (int i = 0; i < filesAtPath.Length; i++)
        {
            fileNames.Add(Path.GetFileName(filesAtPath[i]));
        }

        return fileNames;

    }

    /// <summary>
    /// Returns list of filenames at provided path parameter if any files exist.
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    public static List<string> GetJSONFileNamesWithoutExtensions(string directoryPath)
    {
        List<string> fileNames = new List<string>();
        string[] filesAtPath = Directory.GetFiles(directoryPath, "*.json");

        for (int i = 0; i < filesAtPath.Length; i++)
        {
            fileNames.Add(Path.GetFileNameWithoutExtension(filesAtPath[i]));
        }

        return fileNames;

    }

    /// <summary>
    /// Returns list of directory names of all directories at filePath if any directories exist.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<string> GetJSONDirectoryNames(string filePath)
    {
        List<string> directoryNames = new List<string>();
        string[] directoriesAtPath = Directory.GetDirectories(filePath);

        for (int i = 0; i < directoriesAtPath.Length; i++)
        {
            directoryNames.Add(Path.GetFileName(directoriesAtPath[i]));
        }

        return directoryNames;
    }

    public static List<string> LoadAllJSONInDirectoryAsStrings(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            List<string> jsonFileNames = new List<string>(GetJSONFileNames(directoryPath));
            List<string> jsonStrings = new List<string>();
            int count = jsonFileNames.Count;
            for (int i = 0; i < count; i++)
            {
                jsonStrings.Add(LoadJSONAsString(directoryPath + jsonFileNames[i]));
            }
            return jsonStrings;
        }
        else
        {
            Debug.LogError("No directory found at: " + directoryPath);
            return null;
        }
    }

    public static DateTime GetDateTimeForFile(string filePath)
    {
        return File.GetCreationTime(filePath);
    }

    public static bool CheckForExistingFileName(string filePath)
    {
        return File.Exists(filePath);
    }

    public static bool CheckForValidDirectory(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }
}