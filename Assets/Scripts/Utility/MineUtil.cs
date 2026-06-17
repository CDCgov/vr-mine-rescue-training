using UnityEngine;

public static class MineUtil
{
    public static int RoofCheckLayerMask;

    private static RaycastHit[] _raycastHits;
    private static Collider[] _colliders;

    static MineUtil()
    {
        RoofCheckLayerMask = LayerMask.GetMask("Default", "Floor", "RoofBolts");

        _raycastHits = new RaycastHit[25];
        _colliders = new Collider[25];
    }
    
    public static bool IsRoofGood(Vector3 pos)
    {        
        int numHits = 0;
        bool roofGood = true;

        //numHits = Physics.RaycastNonAlloc(pos - new Vector3(0,0.25f,0), Vector3.up, _raycastHits, 50.0f, RoofCheckLayerMask);

        //for (int i = 0; i < numHits; i++)
        //{
        //    var hit = _raycastHits[i];

        //    if (hit.collider != null && hit.collider.TryGetComponent<SoundingType>(out var colSoundingType))
        //    {
        //        if (colSoundingType.SoundMaterial == SoundType.BadRoof)
        //        {
        //            roofGood = false;
        //            break;
        //        }
        //    }
        //}

        numHits = Physics.OverlapBoxNonAlloc(pos, new Vector3(0.2f, 0.75f, 0.2f), _colliders);
        for (int i = 0; i < numHits; i++)
        {
            var hit = _colliders[i];

            if (hit.TryGetComponent<SoundingType>(out var colSoundingType))
            {
                if (colSoundingType.SoundMaterial == SoundType.BadRoof)
                {
                    roofGood = false;
                    break;
                }
            }
        }


        return roofGood;
    }
}
