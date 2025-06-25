using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManagerBase : MonoBehaviour
{
    //protected static T GetDefault<T>(string objName, bool createNew = true) where T : Component
    //{


    //    T manager = FindObjectOfType<T>();
    //    if (manager != null)
    //        return manager;

    //    var obj = GameObject.Find(objName);
    //    if (obj == null)
    //    {
    //        if (createNew)
    //        {
    //            obj = new GameObject(objName);
    //            //obj.tag = "Manager";
    //        }
    //        else
    //            return null;
    //    }

    //    manager = obj.GetComponent<T>();
    //    if (manager == null)
    //    {
    //        if (createNew)
    //            manager = obj.AddComponent<T>();
    //        else
    //            return null;
    //    }


    //    return manager;
    //}
}
