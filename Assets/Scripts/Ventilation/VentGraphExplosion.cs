using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VentGraphExplosion : MonoBehaviour
{
    private struct AirwayExplosionData
    {
        public float ExplosionTime;
        public VentAirway Airway;
        public Vector3 StartPos;
        public Vector3 EndPos;
    }

    public VentilationManager VentilationManager;

    public GameObject VFXPrefab;
    public SoundEffectCollection ExplosionSoundEffects;

    public float VFXSpeed = 5;
    public float VFXDuration = 2;
    public float VFXDestroyDelay = 5;

    public bool PlayAudioEffect = true;
    public bool ExplosiveRangeOnly = false;

    private VentGraph _ventGraph;
    private HashSet<int> _visitedAirways;
    //private List<AirwayExplosionData> _nextAirways;
    //private List<AirwayExplosionData> _currentAirways;
    private LinkedList<AirwayExplosionData> _nextAirways;
    //private List<GameObject> _activeExplosions;

    //private float _nextExplosionTime;

    // Start is called before the first frame update
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _visitedAirways = new HashSet<int>();
        //_nextAirways = new List<AirwayExplosionData>();
        //_currentAirways = new List<AirwayExplosionData>();
        _nextAirways = new LinkedList<AirwayExplosionData>();
        //_activeExplosions = new List<GameObject>();

        _ventGraph = VentilationManager.GetVentilationGraph();
        if (_ventGraph == null)
        {
            Debug.LogError("VentGraphExplosion: Couldn't find VentGraph");
            this.enabled = false;
            Destroy(gameObject);
            return;
        }

        var startAirway = _ventGraph.FindClosestAirway(transform.position);
        if (startAirway == null)
        {
            Debug.LogError("VentGraphExplosion: Couldn't find start airway");
            this.enabled = false;
            Destroy(gameObject);
            return;
        }

        var midPoint = startAirway.Start.WorldPosition + startAirway.End.WorldPosition;
        midPoint *= 0.5f;

        _nextAirways.AddLast(new AirwayExplosionData
        {
            Airway = startAirway,
            StartPos = midPoint,
            EndPos = startAirway.Start.WorldPosition,
            ExplosionTime = 0,
        });
        _nextAirways.AddLast(new AirwayExplosionData
        {
            Airway = startAirway,
            StartPos = midPoint,
            EndPos = startAirway.End.WorldPosition,
            ExplosionTime = 0,
        });

        _visitedAirways.Add(startAirway.AirwayID);

        SpawnNextExplosions();

        if (PlayAudioEffect)
            PlayExplosionAudioEffect();
    }

    private void SpawnNextExplosions()
    {
        //_nextExplosionTime = Time.time + VFXDuration;

        //schedule current explosions for destruction
        //foreach (var obj in _activeExplosions)
        //{
        //    Destroy(obj, VFXDestroyDelay);
        //}

        //_activeExplosions.Clear();

        //swap next airways to current airways
        //var tmp = _nextAirways;
        //_nextAirways = _currentAirways;
        //_currentAirways = tmp;

        //clear old data
        //_nextAirways.Clear();

        var nextNode = _nextAirways.First;

        while (nextNode != null)
        {
            var currentNode = nextNode;
            nextNode = currentNode.Next;

            var data = currentNode.Value;

            if (data.ExplosionTime > Time.time)
                continue;

            //remove from queue
            _nextAirways.Remove(currentNode);

            Debug.Log($"VentGraphExplosion: Spawning explosion in airway {data.Airway.AirwayID}");
            //Debug.DrawLine(data.Airway.Start.WorldPosition, data.Airway.End.WorldPosition, Color.red, VFXDuration);

            var color = Random.ColorHSV(0, 1, 1, 1, 1, 1);
            Debug.DrawLine(data.StartPos, data.EndPos, color, VFXDuration);
            var duration = SpawnExplosion(data.StartPos, data.EndPos);

            foreach (var airway in data.Airway.GetAdjacentAirways())
            {
                if (airway == null)
                    continue;

                if (_visitedAirways.Contains(airway.AirwayID))
                    continue;

                Vector3 startPos = Vector3.zero;
                Vector3 endPos = Vector3.zero;

                if (Vector3.Distance(data.EndPos, airway.End.WorldPosition) < 0.5f)
                {
                    startPos = airway.End.WorldPosition;
                    endPos = airway.Start.WorldPosition;
                }
                else if (Vector3.Distance(data.EndPos, airway.Start.WorldPosition) < 0.5f)
                {
                    startPos = airway.Start.WorldPosition;
                    endPos = airway.End.WorldPosition;
                }
                else
                    continue;

                _nextAirways.AddLast(new AirwayExplosionData
                {
                    Airway = airway,
                    StartPos = startPos,
                    EndPos = endPos,
                    ExplosionTime = Time.time + duration,
                });
                _visitedAirways.Add(airway.AirwayID);
            }
        }
    }

    private float SpawnExplosion(Vector3 startPos, Vector3 endPos)
    {
        var dist = Vector3.Distance(startPos, endPos);
        var duration = dist / VFXSpeed;

        var obj = Instantiate<GameObject>(VFXPrefab);
        //schedule destruction
        Destroy(obj, duration + VFXDestroyDelay);

        obj.transform.position = startPos;
        obj.transform.forward = (endPos - startPos).normalized;

        var vfx = obj.GetComponent<VisualEffect>();

        startPos = obj.transform.InverseTransformPoint(startPos);
        endPos = obj.transform.InverseTransformPoint(endPos);
        vfx.SetVector3("StartPosition", startPos);
        vfx.SetVector3("EndPosition", endPos);
        vfx.SetFloat("Duration", duration);

        return duration;
        //_activeExplosions.Add(obj);
    }

    private void PlayExplosionAudioEffect()
    {
        if (!PlayAudioEffect)
            return;

        if (ExplosionSoundEffects == null)
            return;

        var audioClip = ExplosionSoundEffects.GetRandomSound();
        if (audioClip == null)
            return;

        if (!TryGetComponent<AudioSource>(out var source))
            return;

        source.clip = audioClip;
        source.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (_nextAirways == null || _nextAirways.Count <= 0)
        {
            this.enabled = false;
            Destroy(gameObject, VFXDestroyDelay);
            return;
        }

        SpawnNextExplosions();
    }
}
