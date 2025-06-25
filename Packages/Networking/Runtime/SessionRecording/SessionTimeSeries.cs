using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ISessionTimeSeriesData<T>
{
    //public float Timestamp { get; set; }
    void Interpolate(T next, float interp, ref T result);
    void CopyTo(T dest);
}

//public class TimeSeriesComparer<T> : IComparer<SessionTimeSeriesData<T>>
//{
//    public int Compare(SessionTimeSeriesData<T> x, SessionTimeSeriesData<T> y)
//    {
//        if (x.Timestamp == y.Timestamp)
//            return 0;
//        else if (x.Timestamp < y.Timestamp)
//            return -1;

//        return 1;
//    }
//}

public class SessionTimeSeries<T> where T : ISessionTimeSeriesData<T>
{
    private List<T> _data;
    private List<float> _timestamps;

    public SessionTimeSeries( )
    {
        _data = new List<T>();
        _timestamps = new List<float>();
    }

    public float ComputeTimestampBin(float timestamp, float binSize)
    {
        int bin = (int)(timestamp / binSize);

        return (float)(bin) * binSize;
    }

    /// <summary>
    /// add an entry, assumes it is already sorted and a later timestamp than current entries
    /// </summary>
    public void AddSequentialEntry(T data, float timestamp)
    {
        _data.Add(data);
        _timestamps.Add(timestamp);
    }

    public float GetLastTimestamp()
    {
        if (_timestamps == null || _timestamps.Count <= 0)
            return -1;

        return _timestamps[_timestamps.Count - 1];
    }

    public T GetData(int index)
    {
        if (index < 0 || index >= _data.Count)
            return default(T);

        return _data[index];
    }

    public float GetTimestamp(int index)
    {
        if (index < 0 || index >= _timestamps.Count)
            return -1;

        return _timestamps[index];
    }

    public T GetLastData()
    {
        if (_data == null || _data.Count <= 0)
            return default(T);

        return _data[_data.Count - 1];
    }

    public IEnumerable<KeyValuePair<float, T>> GetAllData()
    {
        for (int i = 0; i < _data.Count; i++)
        {
            yield return new KeyValuePair<float, T>(_timestamps[i], _data[i]);
        }
    }

    public int GetClosestIndex(float timestamp)
    {
        if (_data.Count <= 0)
            return -1;

        if (_data.Count == 1)
        {
            return 0;
        }

        int index = _timestamps.BinarySearch(timestamp);

        //if negative return value is the bitwise complement of the next largest item        
        if (index < 0)
        {
            index = ~index;

            //if (index >= 1 && _timestamps[index] > timestamp)
            if (index >= 1)
                index--;
        }

        if (index >= _data.Count)
            return _data.Count - 1;

        return index;
    }

    public T GetClosestData(float timestamp)
    {
        int index = GetClosestIndex(timestamp);

        if (index < 0)
            return default(T);

        return _data[index];
    }

    public bool InterpolateData(float timestamp, ref T result)
    {
        if (_data.Count <= 0)
            return false;

        int index = _timestamps.BinarySearch(timestamp);

        if (index >= 0)
        {
            _data[index].CopyTo(result);
            return true;
        }

        if (_data.Count == 1)
        {
            _data[0].CopyTo(result);
            return true;
        }

        //return value is the bitwise complement of the next largest item        
        int index2 = ~index;
        int index1 = index2 - 1;

        //for now just return most recent
        if (index1 >= 0)
            _data[index1].CopyTo(result);
        else
            _data[index2].CopyTo(result);
        return true;

        if (index2 <= 0)
            return false;

        T data1 = _data[index1];
        T data2 = _data[index2];

        float ts1 = _timestamps[index1];
        float ts2 = _timestamps[index2];

        if (timestamp > ts2 || timestamp < ts1)
        {
            Debug.LogError($"SessionTimeSeries: Timestamp out of expected range, ts:{timestamp:F1}, ts1:{ts1:F1} ts2:{ts2:F1}");
        }
        float t = timestamp - ts1;
        float range = ts2 - ts1;
        float interp = 0;
        if (range != 0)
            interp = t / range;

        interp = Mathf.Clamp(interp, 0, 1);

        data1.Interpolate(data2, interp, ref result);
        return true;
    }
}
