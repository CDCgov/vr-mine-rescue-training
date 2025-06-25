using System;
using UnityEngine;
using System.Collections.Generic;

public class StaticVentilationManager : SceneManagerBase
{
    //public MineAtmosphere DefaultMineAtmosphere;
    public VentilationManager VentilationManager;

    private StaticVentilationZone[] _ventZones;

    private List<ActiveAtmosphere> _activeAtmospheres;

    private struct ActiveAtmosphere
    {
        public ActiveAtmosphere(Vector3 worldPos, float strength, MineAtmosphere atmosphere)
        {
            Strength = strength;
            Atmosphere = atmosphere;
            WorldPos = worldPos;
        }

        public Vector3 WorldPos;
        public float Strength;
        public MineAtmosphere Atmosphere;
    }

    public static StaticVentilationManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<StaticVentilationManager>("StaticVentilationManager", false);
    }

    public void GetMineAtmosphere(Vector3 worldPos, out MineAtmosphere mineAtmosphere)
    {
        MineAtmosphere defaultAtmosphere = MineAtmosphere.NormalAtmosphere;
        if (VentilationManager != null)
            defaultAtmosphere = VentilationManager.DefaultAtmosphere;

        mineAtmosphere = new MineAtmosphere(0, 0, 0, 0);
        mineAtmosphere.SetStrength(0);

        //zoneStrength = 0;
        if (_ventZones == null || _ventZones.Length <= 0)
        {
            //mineAtmosphere = defaultAtmosphere;
            return;
        }

        _activeAtmospheres.Clear();

        foreach (var zone in _ventZones)
        {
            MineAtmosphere localAtmosphere;
            float localStrength;

            if (zone == null || zone.gameObject == null)
                continue;

            zone.GetMineAtmosphere(worldPos, out localStrength, out localAtmosphere);
            if (localStrength > 0)
            {
                localAtmosphere.ScaleStrength(localStrength);
                mineAtmosphere.Combine(localAtmosphere);
            }
        }

        //mineAtmosphere.Normalize()

        //foreach (var zone in _ventZones)
        //{
        //    MineAtmosphere localAtmosphere;
        //    float strength;

        //    if (zone == null || zone.gameObject == null)
        //        continue;

        //    zone.GetMineAtmosphere(worldPos, out strength, out localAtmosphere);
        //    if (strength > 0)
        //    {
        //        if (strength >= 1)
        //        {
        //            numFullStrength++;
        //        }

        //        //Debug.Log($"In atm zone {zone.name} - {strength}");
        //        _activeAtmospheres.Add(new ActiveAtmosphere(zone.transform.position, strength, localAtmosphere));

        //        totalScale += strength;
        //        mineAtmosphere = mineAtmosphere + (localAtmosphere * strength);
        //    }
        //}

        //if (totalScale < 1)
        //{
        //    //add in default atmosphere if other zones aren't providing full strength
        //    float defaultStrength = 1.0f - totalScale;
        //    mineAtmosphere = mineAtmosphere + (defaultAtmosphere * defaultStrength);
        //}
        //else
        //{           
        //    if (numFullStrength >= 1)
        //    {
        //        float minDist = float.MaxValue;

        //        foreach (var active in _activeAtmospheres)
        //        {
        //            if (active.Strength < 1)
        //                continue;

        //            float dist = Vector3.Distance(worldPos, active.WorldPos);
        //            if (dist < minDist)
        //            {
        //                minDist = dist;
        //                mineAtmosphere = active.Atmosphere;
        //            }
        //        }

        //        if (minDist < float.MaxValue)
        //        {
        //            //found full strength zone, use this value
        //            zoneStrength = 1;
        //            return;
        //        }
        //    }


        //    //normalize to 1.0 strength
        //    mineAtmosphere = mineAtmosphere * (1.0f / totalScale);
        //}

        //zoneStrength = Mathf.Clamp(totalScale, 0, 1);


    }

    private void Awake()
    {
        _activeAtmospheres = new List<ActiveAtmosphere>(50);
    }

    public void LoadStaticZones()
    {
        _ventZones = FindObjectsOfType<StaticVentilationZone>();

        if (_ventZones != null)
            Debug.Log($"StaticVentilationManager: Found {_ventZones.Length} static vent zones");
    }

    // Use this for initialization
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        LoadStaticZones();
    }



}
