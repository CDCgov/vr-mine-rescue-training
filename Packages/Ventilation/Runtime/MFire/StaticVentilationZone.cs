using UnityEngine;
using System.Collections;

public class StaticVentilationZone : MonoBehaviour
{
    public StaticMineAtmosphere StaticMineAtmosphere;

    public float Radius = 10;
    public float FalloffSize = 10;

    public virtual void Awake()
    {
        if (StaticMineAtmosphere == null)
            StaticMineAtmosphere = ScriptableObject.CreateInstance<StaticMineAtmosphere>();
    }

    public virtual void GetMineAtmosphere(Vector3 worldPos, out float zoneStrength, out MineAtmosphere atmosphere)
    {
        float dist = Vector3.Distance(transform.position, worldPos);

        //get local atmosphere with variation applied
        atmosphere = GetLocalAtmosphere();

        if (dist > FalloffSize+Radius)
        {
            zoneStrength = 0;
        }
        else if (dist > Radius && FalloffSize > 0)
        {
            zoneStrength = 1.0f - ((dist - Radius) / FalloffSize);
        }
        else
        {
            zoneStrength = 1;
        }
    }

    protected MineAtmosphere GetLocalAtmosphere()
    {
        ////compute variation 
        //var atmVariation = StaticMineAtmosphere.MineAtmosphereVariation;
        //atmVariation.Oxygen *= NoiseMultiplier(0);
        //atmVariation.CarbonMonoxide *= NoiseMultiplier(1);
        //atmVariation.Methane *= NoiseMultiplier(2);
        //atmVariation.HydrogenSulfide *= NoiseMultiplier(3);

        //var atmosphere = StaticMineAtmosphere.MineAtmosphere + atmVariation;

        //return atmosphere;

        return StaticMineAtmosphere.GetAtmosphere();
    }

   
   
    protected virtual void OnDrawGizmosSelected()
    {        
        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, Radius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Radius+FalloffSize);

    }

}
