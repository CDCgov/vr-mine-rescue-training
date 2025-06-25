
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using System.Threading;
using System.Threading.Tasks;

[RequireComponent(typeof(ParticleSystem))]
public class VentParticleControl : MonoBehaviour
{
    public VentilationManager VentilationManager;

    public int MaxSpawnPerCycle = 25;
    public float VelocityMultiplier = 3.0f;
    public bool EnableParticleSpawn = true;

    public VentVisualizationData VisualizationData;
    //public Gradient ColorGradient;
    //public float ColorMinValue = 0;
    //public float ColorMaxValue = 1;

    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _particles;
    private Vector3[] _particleVel;
    private Color32[] _particleCol;

    private UpdateParticlesJob _job = new UpdateParticlesJob();

    public static VentGraph VentGraph;

    private CancellationTokenSource _cancelSource;
    private CancellationToken _cancelToken;
    private bool _ventUpdated = false;

    // Start is called before the first frame update
    void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        _ps = GetComponent<ParticleSystem>();
        var main = _ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        _particles = new ParticleSystem.Particle[_ps.main.maxParticles];
        _particleVel = new Vector3[_ps.main.maxParticles];
        _particleCol = new Color32[_ps.main.maxParticles];

        _cancelSource = new CancellationTokenSource();
        _cancelToken = _cancelSource.Token;

        VentGraph = VentilationManager.GetVentilationGraph();
        //_job.Schedule(_ps);

        //StartCoroutine(UpdateParticlesLoop());
        UpdateParticlesLoop();
    }

    private void OnDestroy()
    {
        _cancelSource.Cancel();
    }

    ParticleSystem.EmitParams ComputeEmitParams()
    {
        ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();

        var graph = VentilationManager.GetVentilationGraph();
        if (graph == null)
            return ep;

        var airway = graph.GetRandomAirway();
        if (airway.Start == null || airway.End == null)
            return ep;

        var start = airway.Start.WorldPosition;
        var end = airway.End.WorldPosition;
        var dir = (end - start).normalized;

        var offset = dir;
        offset.y = 0;
        offset.x = -1 * dir.z;
        offset.z = dir.x;

        //rotate randomly around the axis
        Quaternion rot = Quaternion.AngleAxis(Random.value * 360, dir);
        offset = rot * offset;

        offset.Normalize();

        var pos = Vector3.Lerp(start, end, Random.value);
        pos += offset * Random.value * 2.0f;
        pos.y = Random.Range(0.5f, 2.0f);

        float speed = airway.ComputeAirSpeed() * VelocityMultiplier;

        //if (speed <= 0)
        //    ep.startLifetime = 1.0f;
        //else
        //{
        //    var lifetime = (1.0f / speed) * 2.0f; //travel a fixed dist
        //    if (lifetime > 2.0f)
        //        lifetime = 2.0f;

        //    ep.startLifetime = lifetime;
        //}
        ep.startLifetime = Random.Range(2.0f, 8.0f);

        ep.position = pos;
        ep.velocity = dir * speed;

        ep.startColor = Color.HSVToRGB(Random.value, 1, 1);


        return ep;
    }

    private void ComputeParticleVelocities(ParticleSystem.Particle[] particles, VentGraph graph, Vector3[] vels)
    {
        for (int i = 0; i < particles.Length; i++)
        {
            var p = particles[i];
            vels[i] = ComputeParticleVelocity(p, graph);
        }
    }

    private void ComputeParticleColors(ParticleSystem.Particle[] particles, VentGraph graph, Color32[] colors)
    {
        for (int i = 0; i < particles.Length; i++)
        {
            var p = particles[i];
            colors[i] = ComputeParticleColor(p, graph);
        }
    }

    void UpdateParticlesTest()
    {
        int numAlive = _ps.GetParticles(_particles);

        for (int i = 0; i < numAlive; i++)
        {
            var p = _particles[i];

            //p.position = Random.insideUnitSphere * 3;
            p.startColor = Color.HSVToRGB(Random.value, 1, 1);

            _particles[i] = p;
        }

        var main = _ps.main;

        /*int spawnCount = 0;
        for (int i = numAlive; i < _particles.Length; i++)
        {
            var p = _particles[i];

            
            p.startLifetime = main.startLifetime.Evaluate(0);
            p.remainingLifetime = p.startLifetime;
            p.position = Random.insideUnitSphere * 1;
            p.startColor = p.GetCurrentColor(_ps);
            p.startSize = p.GetCurrentSize(_ps);
            p.startSize3D = p.GetCurrentSize3D(_ps);
            p.velocity = Vector3.up + Random.insideUnitSphere;
            //p.animatedVelocity = Vector3.zero;

            

            _particles[i] = p;

            spawnCount++;
            if (spawnCount > 5)
                break;
        }*/

        //Debug.Log($"Spawned {spawnCount} particles");

        //_ps.SetParticles(_particles, numAlive+spawnCount);
        _ps.SetParticles(_particles, numAlive);
    }

    private Vector3 ComputeParticleVelocity(ParticleSystem.Particle p, VentGraph graph)
    {
        VentAirway a1, a2;
        float d1, d2;

        if (!graph.FindNearbyAirways(p.position, out a1, out a2, out d1, out d2))
            return Vector3.zero;

        float ratio;
        if (d1 > 3.0f || d2 > 3.0f)
        {
            if (d1 > d2)
                ratio = 1.0f;
            else
                ratio = 0.0f;
        }
        else
        {
            //ratio = (d1 / d2);
            //ratio = Mathf.Clamp(ratio, 0, 3) / 3.0f;
            ratio = 0.5f;
        }

        var vel1 = a1.ComputeAirVelocity() * VelocityMultiplier;
        var vel2 = a2.ComputeAirVelocity() * VelocityMultiplier;
        //var targetVel = (vel1 + vel2) * 0.5f;
        var targetVel = vel1 * (1 - ratio) + vel2 * ratio;

        return targetVel;
    }

    private Color32 ComputeParticleColor(ParticleSystem.Particle p, VentGraph graph)
    {
        if (VisualizationData == null)
            return Color.red;

        //float t = graph.ComputeLocalTemperature(p.position);
        //float t = graph.ComputeLocalMethane(p.position);
        //t -= ColorMinValue;
        //t = t / (ColorMaxValue - ColorMinValue);
        //return ColorGradient.Evaluate(t);

        float t = 0;
        switch (VisualizationData.VisualizationParameter)
        {
            case VentVisualizationParameter.Contaminant:
                t = graph.ComputeLocalContaminant(p.position);
                break;

            case VentVisualizationParameter.Temperature:
                t = graph.ComputeLocalTemperature(p.position);
                break;

            default:
            case VentVisualizationParameter.Methane:
                t = graph.ComputeLocalMethane(p.position);
                break;
        }
        
        t -= VisualizationData.ParamMinValue;
        t = t / (VisualizationData.ParamMaxValue - VisualizationData.ParamMinValue);

        return VisualizationData.ColorGradient.Evaluate(t);
    }

    void UpdateParticles()
    {
        var graph = VentilationManager.GetVentilationGraph();
        if (graph == null)
            return;

        int numAlive = _ps.GetParticles(_particles);

        for (int i = 0; i < numAlive; i++)
        {
            var p = _particles[i];

            //p.position = Random.insideUnitSphere * 3;
            //p.startColor = Color.HSVToRGB(Random.value, 1, 1);

            //var airway = graph.FindClosestAirway(p.position);
            //var targetVel = airway.ComputeAirVelocity() * VelocityMultiplier;

            var targetVel = ComputeParticleVelocity(p, graph);
            p.velocity = Vector3.Lerp(p.velocity, targetVel, 0.15f);


            //compute color            
            p.startColor = ComputeParticleColor(p, graph);

            _particles[i] = p;
        }       
        _ps.SetParticles(_particles, numAlive);
    }

    void SpawnParticles()
    {
        //ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
        //ep.position = Random.insideUnitSphere * 1;
        //ep.velocity = Vector3.up + Random.insideUnitSphere;

        var main = _ps.main;
        var numAlive = _ps.particleCount;

        var spawnCount = Mathf.Min(MaxSpawnPerCycle, main.maxParticles - numAlive);
        for (int i = 0; i < spawnCount; i++)
        {
            ParticleSystem.EmitParams ep = ComputeEmitParams();
            _ps.Emit(ep, 1);
        }
    }

    private async void UpdateParticlesLoop()
    {
        Task[] tasks = new Task[2];

        var graph = VentilationManager.GetVentilationGraph();
        VentilationControl ventControl = null;
       

        Debug.Log($"VentParticleControl: Starting particle update loop");

        while (!_cancelToken.IsCancellationRequested)
        {
            try
            {
                if (ventControl == null)
                {
                    ventControl = VentilationManager.GetVentilationControl();
                    if (ventControl != null)
                    {
                        ventControl.VentilationUpdated += () =>
                        {
                            _ventUpdated = true;
                        };
                    }
                    await Task.Delay(100);
                    continue;
                }

                if (graph == null || graph.NumAirways <= 0 || graph.NumJuncions <= 0)
                {
                    await Task.Delay(100);
                    graph = VentilationManager.GetVentilationGraph();
                    continue;
                }

                _ventUpdated = false;

                int numAlive = _ps.GetParticles(_particles);

                int startFrame = Time.frameCount;
                //Debug.Log($"VentParticleControl: Starting particle update on frame {startFrame}");

                tasks[0] = new Task(() => { ComputeParticleVelocities(_particles, graph, _particleVel); },
                    _cancelToken);
                tasks[1] = new Task(() => { ComputeParticleColors(_particles, graph, _particleCol); },
                    _cancelToken);

                tasks[0].Start();
                tasks[1].Start();
                //Task.WaitAll(tasks, _cancelToken);
                await Task.WhenAll(tasks);

                if (_ventUpdated)
                    continue; //restart computation if ventilation was updated

                int numAlive2 = _ps.GetParticles(_particles);
                numAlive = Mathf.Min(numAlive, numAlive2, _particleCol.Length, _particleVel.Length);
                for (int i = 0; i < numAlive; i++)
                {
                    _particles[i].velocity = _particleVel[i];
                    _particles[i].startColor = _particleCol[i];
                }

                _ps.SetParticles(_particles, numAlive);

                if (EnableParticleSpawn)
                    SpawnParticles();

                if (startFrame == Time.frameCount)
                    await Task.Delay(10);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"VentParticleControl::UpdateParticlesLoop Exception: {ex.Message}");
            }
        }
    }

    // Update is called once per frame
    //void Update()
    //{
    //    //UpdateParticles();
    //    //SpawnParticles();

    //    //_job.Schedule(_ps);

    //}

    struct UpdateParticlesJob : IJobParticleSystem
    {
        //public VentGraph VentGraph;

        public void Execute(ParticleSystemJobData particles)
        {
            var positionsX = particles.positions.x;
            var positionsY = particles.positions.y;
            var positionsZ = particles.positions.z;

            var velocitiesX = particles.velocities.x;
            var velocitiesY = particles.velocities.y;
            var velocitiesZ = particles.velocities.z;

            var colors = particles.startColors;

            var randomSeeds = particles.randomSeeds;

            for (int i = 0; i < particles.count; i++)
            {
                Vector3 position = new Vector3(positionsX[i], positionsY[i], positionsZ[i]);

                var airway = VentParticleControl.VentGraph.FindClosestAirway(position);

                Vector3 velocity = airway.ComputeAirVelocity();
                velocitiesX[i] = velocity.x;
                velocitiesY[i] = velocity.y;
                velocitiesZ[i] = velocity.z;

            }
        }
    }
}
