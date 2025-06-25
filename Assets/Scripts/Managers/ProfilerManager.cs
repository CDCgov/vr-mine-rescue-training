using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;


public class ProfilerManager : SceneManagerBase
{
    private struct ProfilerRecorderData
    {
        public ProfilerRecorder Recorder;
        public string Name;

        public static ProfilerRecorderData Init(string name, ProfilerCategory category, string statName)
        {
            ProfilerRecorderData data = new ProfilerRecorderData();

            data.Recorder = new ProfilerRecorder(category, statName, 100);
            data.Name = name;

            data.Recorder.Start();

            return data;
        }

        public double GetAverage()
        {
            if (Recorder.Capacity <= 0)
                return 0;

            var samplesCount = Recorder.Capacity;

            //From unity ProfilerRecorder docs:
            double r = 0;
            unsafe
            {
                var samples = stackalloc ProfilerRecorderSample[samplesCount];
                Recorder.CopyTo(samples, samplesCount);
                for (var i = 0; i < samplesCount; ++i)
                    r += samples[i].Value;
                r /= samplesCount;
            }

            return r;
        }

        public void AppendData(StringBuilder sb)
        {
            var avg = GetAverage();

            if (avg > 1024.0*1024.0)
                sb.AppendFormat("{0,18}: {1:F2} MiB\n", Name, avg / 1024.0 / 1024.0);
            else
                sb.AppendFormat("{0,18}: {1:F2}\n", Name, avg);
        }
    }


    private List<ProfilerRecorderData> _data;
    private float _lastSampleRequest = 0;

    public static ProfilerManager GetDefault(GameObject self)
    {
        return self.GetDefaultManager<ProfilerManager>("ProfilerManager");
    }

    public void GetLastData(StringBuilder sb)
    {
        if (_data == null)
        {
            StartProfiling();
            sb.AppendLine("Starting Profiling...");
            return;
        }

        _lastSampleRequest = Time.time;

        foreach (var data in _data)
        {
            data.AppendData(sb);
        }
    }

    public void GetProfilerHandleData(StringBuilder sb)
    {
        List<ProfilerRecorderHandle> handles = new List<ProfilerRecorderHandle>();
        ProfilerRecorderHandle.GetAvailable(handles);

        foreach (var handle in handles)
        {
            var desc = ProfilerRecorderHandle.GetDescription(handle);
            sb.AppendLine($"{desc.Category}:{desc.Name}  {desc.DataType}");
        }
    }

    private void StartProfiling()
    {
        _lastSampleRequest = Time.time;
        _data = new List<ProfilerRecorderData>();

        _data.Add(ProfilerRecorderData.Init("UsedTexBytes", ProfilerCategory.Render, "Used Textures Bytes"));
        _data.Add(ProfilerRecorderData.Init("TotalUsedMem", ProfilerCategory.Memory, "Total Used Memory"));
        _data.Add(ProfilerRecorderData.Init("GfxUsedMem", ProfilerCategory.Memory, "Gfx Used Memory"));
        _data.Add(ProfilerRecorderData.Init("TextureMem", ProfilerCategory.Memory, "Texture Memory"));
        _data.Add(ProfilerRecorderData.Init("MeshMem", ProfilerCategory.Memory, "Mesh Memory"));

        _data.Add(ProfilerRecorderData.Init("UsedTextures", ProfilerCategory.Render, "Used Textures Bytes"));
        _data.Add(ProfilerRecorderData.Init("RenderTextures", ProfilerCategory.Render, "Render Textures Bytes"));
        _data.Add(ProfilerRecorderData.Init("UsedBuffers", ProfilerCategory.Render, "Used Buffers Bytes"));
        _data.Add(ProfilerRecorderData.Init("VideoMemory", ProfilerCategory.Render, "Video Memory Bytes"));

        _data.Add(ProfilerRecorderData.Init("GfxPresentFrame", ProfilerCategory.Render, "Gfx.PresentFrame"));
        _data.Add(ProfilerRecorderData.Init("Main Thread", ProfilerCategory.Internal, "Main Thread"));
        _data.Add(ProfilerRecorderData.Init("Render Thread", ProfilerCategory.Internal, "Render Thread"));
    }

    private void StopProfiling()
    {
        if (_data == null)
            return;

        foreach (var data in _data)
        {
            data.Recorder.Stop();
            data.Recorder.Dispose();
        }

        _data = null;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (_data != null)
        {
            var elapsed = Time.time - _lastSampleRequest;
            if (elapsed > 10.0f)
            {
                Debug.Log($"Stopping profilers");
                StopProfiling();
            }
        }
    }

    private void OnDestroy()
    {
        StopProfiling();
    }
}
