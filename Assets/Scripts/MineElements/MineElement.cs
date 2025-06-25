using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using Google.Protobuf;

[System.Serializable]
public class MineElement : INetSync, ISelectableObject
{

    //Public fields
    public string DisplayName;
    [Tooltip("Density in kg/m^3")]
    public float Density;
    public float Explosiveness;
    public float Flammability;
    public float MaxHealth;
    public float Health;//make private?
    public bool Indestructible;    
    //public Bounds Dimension;
    public string PrefabResource;
    //MineElementType?

    public MineElement()
    {
        Density = 1;
        Explosiveness = 1;
        Flammability = 1;
        Health = 100;
        Indestructible = false;
        //Dimension = new Bounds(new Vector3(0, 0, 0), new Vector3(1, 1, 1));
    }

    protected virtual GameObject GetCurrentPrefab()
    {
        throw new System.NotImplementedException(); 
    }
    
    /// <summary>
    /// Spawn the minelement into the unity scene, hosting it in an appropriate MineElementHost
    /// </summary>
    public virtual GameObject Spawn()
    {
        return null;
    }

    public GameObject Spawn(Vector3 pos, Quaternion rot)
    {
        GameObject obj = Spawn();

        if (obj != null)
        {
            obj.transform.position = pos;
            obj.transform.rotation = rot;
        }

        return obj;
    }

    /// <summary>
    /// Remove the mineelement from the unity scene, this is responsible for destroying any relevent MineElementHost
    /// </summary>
    public virtual void Despawn()
    {

    }

    /// <summary>
    /// Method called when health is reduced to zero or less.
    /// </summary>
    /// <param name="killedBy">Optional MineElement reference to handle unique behaviors when killed by certain MineElements (ex. Killed by fire, gas, debris, physics)</param>
    protected virtual void OnDeath(MineElement killedBy = null)
    {

    }

    /// <summary>
    /// Method called to handle cleanup. Only called once during the lifetime of the object.
    /// </summary>
    protected virtual void OnDestruction()
    {
        
    }

    /// <summary>
    /// Reduces health of the mine element.
    /// </summary>
    /// <param name="damage">Amount of damage to deal to the MineElement</param>
    /// <param name="owner">Identity of the other MineElement performing the damage, if any</param>
    /// <param name="source">Used to calculate the direction from which damage is taken</param>
    void ApplyDamage(float damage, MineElement owner = null, Transform source = null)
    {
        Health -= damage;
    }

    /// <summary>
    /// Public interface to destroy this MineElement.
    /// </summary>
    public void Destruct()
    {
        //Note: Destroy is a Monobehavior.
        //Also, .NET will automatically call garbage collection once all variables = null;
    }

    /// <summary>
    /// Actions or calculations performed once per frame. Called by MineElementHost.
    /// </summary>
    public virtual void Update()
    {

    }

    /// <summary>
    /// Does this MineElement need sync'd over the network
    /// </summary>
    public bool NeedsUpdate()
    {
        return false;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        return;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        return;
    }

    public virtual void GetObjectInfo(StringBuilder sb)
    {
        sb.AppendFormat("Health       : {0}\n", Health);
        sb.AppendFormat("Density      : {0}\n", Density);
        sb.AppendFormat("Explosiveness: {0}\n", Explosiveness);
        sb.AppendFormat("Flammability : {0}\n", Flammability);
        sb.AppendFormat("Destroyable  : {0}\n", Indestructible ? "No" : "Yes");

        if (PrefabResource != null)
        {
            string prefabName = PrefabResource;
            int index = prefabName.LastIndexOf('/');
            if (index > 0)
            {
                prefabName = prefabName.Substring(index+1);
                if (prefabName.Length > 18)
                {
                    prefabName = prefabName.Substring(0, 16) + "...";
                }
            }
            sb.AppendFormat("Prefab Name  : {0}\n", prefabName);
        }
    }

    public virtual string GetObjectDisplayName()
    {
        return DisplayName;
    }
}
