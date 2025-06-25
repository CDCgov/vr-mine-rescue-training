using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RespawnableSceneObject : MonoBehaviour
{
    public SceneObjectResetManager SceneObjectResetManager;

    public ObjectResetCategory RespawnCategory;

    //should the object be destroyed if it still exists when its reset?
    public bool DestroyBeforeRespawn = false;
    public bool ResetPosition = true;
    public bool ResetRotation = true;
    public bool ResetScale = true;

    public AssetReference Prefab; 

    /*
    private Vector3 _startPos;
    private Quaternion _startRot; */

    //public ObjectRespawnData RespawnData;

    // Start is called before the first frame update
    void Start()
    {
        if (SceneObjectResetManager == null)
            SceneObjectResetManager = SceneObjectResetManager.GetDefault(gameObject);

        SceneObjectResetManager.RegisterObject(RespawnCategory, gameObject,
            Prefab, DestroyBeforeRespawn, ResetPosition, ResetRotation,
            ResetScale);

        //_startPos = transform.position;
        //_startRot = transform.rotation;
    }

    //public static void RespawnObject(RespawnableSceneObject obj)
    //{

    //}

}
