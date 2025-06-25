using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneReader 
{
    //private List<AudioClip> _clips;

    string _device;
    AudioClip _clip;
    int _readPos = 0;
    float[] _buffer = null;


    public MicrophoneReader(string device)
    {
        _device = device;
        //_clips = new List<AudioClip>();
        //foreach (var mic in Microphone.devices)
        //{
        //    Debug.Log($"Mic: {mic}");

        //    var clip = Microphone.Start(mic, true, 3, 44100);
        //    _clips.Add(clip);
        //    clip.GetData()
        //}

        _clip = Microphone.Start(device, true, 5, 48000);
    }

    public bool ReadSamples(int numSamples, int writeOffset, float[] buffer)
    {
        if (_buffer == null || _buffer.Length != numSamples)
            _buffer = new float[numSamples];

        int pos = Microphone.GetPosition(_device);
        int availSamples = pos - _readPos;
        if (_readPos > pos)
        {
            //buffer wrapped
            
        }

        //Debug.Log($"Available Samples : {availSamples}");

        if (availSamples < numSamples)
            return false;

        //Debug.Log($"Reading {_buffer.Length} bytes starting at {_readPos}");

        //read the data into the buffer
        if (!_clip.GetData(_buffer, _readPos))
            return false;

        _readPos += _buffer.Length;

        //write the data into the circular output buffer
        if (buffer.Length - writeOffset >= numSamples)
        {
            _buffer.CopyTo(buffer, writeOffset);
        }
        else
        {
            return false;
        }

        return true;
    }

    public void Clear()
    {
        _readPos = Microphone.GetPosition(_device);
    }
}
