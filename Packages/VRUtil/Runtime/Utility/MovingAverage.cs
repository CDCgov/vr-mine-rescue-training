using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MovingAverage
{
    public float Average
    {
        get
        {
            if (_samplesChanged)
                ComputeAverage();

            return _average;
        }
    }

    public bool HasSamples
    {
        get
        {
            return _numSamples > 0 ? true : false;
        }
    }

    private float[] _samples;
    private float _average;

    private int _curIndex;
    private int _numSamples;
    private bool _samplesChanged;

    public MovingAverage(int numSamples)
    {
        _samples = new float[numSamples];

        Reset();
    }

    public void Reset()
    {
        _average = 0;
        _curIndex = 0;
        _numSamples = 0;
        _samplesChanged = true;

        for (int i = 0; i < _samples.Length; i++)
        {
            _samples[i] = 0;
        }
    }

    void ComputeAverage()
    {
        if (_numSamples <= 0)
        {
            _average = 0;
            return;
        }

        if (_numSamples > _samples.Length)
            _numSamples = _samples.Length;

        float sum = 0;
        for (int i = 0; i < _numSamples; i++)
        {
            sum += _samples[i];
        }
        _average = sum / (float)_numSamples;
    }

    public void AddSample(float val)
    {
        if (_curIndex >= _samples.Length)
            _curIndex = 0;

        _numSamples++;
        _samples[_curIndex] = val;

        _curIndex++;
    }
}