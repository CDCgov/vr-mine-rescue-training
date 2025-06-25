using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;

[Serializable]
public class ComponentInfo_BoltGraph : MonoBehaviour, ISaveableComponent
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "BoltGraph";
    //protected ObjectInfo objectInfo;

    [Tooltip("The assigned component that lives in the prefab. The reference to the component does not save and must be assigned in the scenario prefab. It should be assigned in the editor prefab if the available, but is not required ")]
    public Inspector.ExposureLevel interactableExposureLevel;

    public GameObject BoltPrefab;
    
    private Dictionary<SerializableVector3, SerializableQuaternion> _boltLocations;
    private byte[] _byteBoltGraph;
    private string _encodedBoltGraph;
    private System.Runtime.Serialization.Formatters.Binary.BinaryFormatter _binaryFormatter;

    private void Awake()
    {
        //objectInfo = GetComponent<ObjectInfo>();
        //if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
        //if (objectInfo != null)
        //{
        //    if (!objectInfo.componentInfo_BoltGraphs.Contains(this)) objectInfo.componentInfo_BoltGraphs.Add(this);
        //}
        //_byteBoltGraph = new byte[];
        _boltLocations = new Dictionary<SerializableVector3, SerializableQuaternion>();
    }
    public string[] SaveInfo()
    {
        RoofBolt[] bolts = GameObject.FindObjectsOfType<RoofBolt>();
        foreach (var bolt in bolts)
        {
            if (!_boltLocations.ContainsKey(bolt.transform.position))
            {
                _boltLocations.Add(bolt.transform.position, bolt.transform.rotation);
            }
        }
        
        _byteBoltGraph = ObjectSerialize.Serialize(_boltLocations);
        Debug.Log($"Byte size: {_byteBoltGraph.Length}");
        //_encodedBoltGraph = Encoding.UTF8.GetString(_byteBoltGraph, 0, _byteBoltGraph.Length);
        _encodedBoltGraph = Convert.ToBase64String(_byteBoltGraph);
        //_encodedBoltGraph = _binaryFormatter.Serialize(_encodedBoltGraph, _boltLocations);
        return new string[] { "BoltGraph|" + _encodedBoltGraph };
    }
    public string SaveName()
    {
        return componentName;
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
        {
            Debug.Log("Failed to load interactable component info. Saved component is null for " + gameObject.name); return;
        }
        componentName = component.GetComponentName();
        //float.TryParse(component.GetParamValueAsStringByName("Mass"), out mass);
        _encodedBoltGraph = component.GetParamValueAsStringByName("BoltGraph");
        _byteBoltGraph = Encoding.UTF8.GetBytes(_encodedBoltGraph);
        //_byteBoltGraph = Convert.TOByt(_encodedBoltGraph);
        _boltLocations = (Dictionary<SerializableVector3, SerializableQuaternion>)ObjectSerialize.DeSerialize(_byteBoltGraph);
        //bool.TryParse(component.GetParamValueAsStringByName("IsInteractable"), out IsInteractable);

        SpawnBoltsFromDictionary();
    }

    void SpawnBoltsFromDictionary()
    {
        foreach (var item in _boltLocations)
        {
            GameObject newBolt = Instantiate(BoltPrefab, item.Key, item.Value);
        }
    }

}
public static class ObjectSerialize
{
    public static byte[] Serialize(this System.Object obj)
    {
        if (obj == null)
        {
            return null;
        }

        using (var memoryStream = new MemoryStream())
        {
            var binaryFormatter = new BinaryFormatter();

            binaryFormatter.Serialize(memoryStream, obj);

            var compressed = Compress(memoryStream.ToArray());
            return compressed;
        }
    }

    public static System.Object DeSerialize(this byte[] arrBytes)
    {
        using (var memoryStream = new MemoryStream())
        {
            var binaryFormatter = new BinaryFormatter();
            var decompressed = Decompress(arrBytes);

            memoryStream.Write(decompressed, 0, decompressed.Length);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return binaryFormatter.Deserialize(memoryStream);
        }
    }

    public static byte[] Compress(byte[] input)
    {
        byte[] compressesData;

        using (var outputStream = new MemoryStream())
        {
            using (var zip = new GZipStream(outputStream, CompressionMode.Compress))
            {
                zip.Write(input, 0, input.Length);
            }

            compressesData = outputStream.ToArray();
        }

        return compressesData;
    }

    public static byte[] Decompress(byte[] input)
    {
        byte[] decompressedData;

        using (var outputStream = new MemoryStream())
        {
            using (var inputStream = new MemoryStream(input))
            {
                using (var zip = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    zip.CopyTo(outputStream);
                }
            }

            decompressedData = outputStream.ToArray();
        }

        return decompressedData;
    }
}

[System.Serializable]
public struct SerializableVector3
{
    /// <summary>
    /// x component
    /// </summary>
    public float x;

    /// <summary>
    /// y component
    /// </summary>
    public float y;

    /// <summary>
    /// z component
    /// </summary>
    public float z;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    /// <param name="rZ"></param>
    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }

    /// <summary>
    /// Automatic conversion from SerializableVector3 to Vector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Vector3(SerializableVector3 rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    /// <summary>
    /// Automatic conversion from Vector3 to SerializableVector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableVector3(Vector3 rValue)
    {
        return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    }
}

[System.Serializable]
public struct SerializableQuaternion
{
    /// <summary>
    /// x component
    /// </summary>
    public float x;

    /// <summary>
    /// y component
    /// </summary>
    public float y;

    /// <summary>
    /// z component
    /// </summary>
    public float z;

    /// <summary>
    /// w component
    /// </summary>
    public float w;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    /// <param name="rZ"></param>
    /// <param name="rW"></param>
    public SerializableQuaternion(float rX, float rY, float rZ, float rW)
    {
        x = rX;
        y = rY;
        z = rZ;
        w = rW;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
    }

    /// <summary>
    /// Automatic conversion from SerializableQuaternion to Quaternion
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Quaternion(SerializableQuaternion rValue)
    {
        return new Quaternion(rValue.x, rValue.y, rValue.z, rValue.w);
    }

    /// <summary>
    /// Automatic conversion from Quaternion to SerializableQuaternion
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableQuaternion(Quaternion rValue)
    {
        return new SerializableQuaternion(rValue.x, rValue.y, rValue.z, rValue.w);
    }
}