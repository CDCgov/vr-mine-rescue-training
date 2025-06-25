using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using OpusDotNet;

public class MicrophoneTest : MonoBehaviour
{
    //private const int SAMPLE_BUFFER_SIZE = 480; //10ms sample buffers, 48khz
    private const int SAMPLE_BUFFER_SIZE = 2880; //60ms sample buffers, 48khz
    private const int NUM_SAMPLE_BUFFERS = 100*5;

    private List<AudioClip> _clips;
    private AudioSource _audioSource;
    private List<float[]> _sampleBuffers;
    private int _currentBuffer = 0;

    //OpusEncoder _encoder;
    //OpusDecoder _decoder;

    MicrophoneReader _micReader;

    private List<byte[]> _encDataList;

    byte[] _decbuffer;
    byte[] _encbuffer;
    int _encPos = 0;

    private void Awake()
    {
        _decbuffer = new byte[48000*5];
        _encbuffer = new byte[48000*10];

        _encDataList = new List<byte[]>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        //Debug.Log($"Sample Buffer Size: {SAMPLE_BUFFER_SIZE}");
        if (Microphone.devices == null || Microphone.devices.Length <= 0)
        {
            enabled = false;
            return;
        }

        foreach (var mic in Microphone.devices)
        {
            Debug.Log(mic);
        }

        Debug.Log($"Opening Mic {Microphone.devices[0]}");
        _micReader = new MicrophoneReader(Microphone.devices[0]);

        //initialize buffers
        _sampleBuffers = new List<float[]>(NUM_SAMPLE_BUFFERS);
        for (int i = 0; i < NUM_SAMPLE_BUFFERS; i++)
        {
            _sampleBuffers.Add(new float[SAMPLE_BUFFER_SIZE]);
        }


        //_clips = new List<AudioClip>();
        //foreach (var mic in Microphone.devices)
        //{
        //    Debug.Log($"Mic: {mic}");

        //    var clip = Microphone.Start(mic, true, 3, 44100);
        //    _clips.Add(clip);
        //}

        




        //using (var encoder = new OpusEncoder(OpusDotNet.Application.Audio, 48000, 2)
        //{
        //    Bitrate = 128000, // 128 kbps
        //    VBR = true // Variable bitrate
        //})
        //using (var decoder = new OpusDecoder(48000, 2))
        //{
        //    // 40 ms of silence at 48 KHz (2 channels).
        //    byte[] inputPCMBytes = new byte[40 * 48000 / 1000 * 2 * 2];
        //    byte[] opusBytes = encoder.Encode(inputPCMBytes, inputPCMBytes.Length, out int encodedLength);
        //    byte[] outputPCMBytes = decoder.Decode(opusBytes, encodedLength, out int decodedLength);


        //}
    }

    //private void OnEnable()
    //{
    //    _encoder = new OpusEncoder(OpusDotNet.Application.Audio, 48000, 1)
    //    {
    //        Bitrate = 128000, // 128 kbps
    //        VBR = true // Variable bitrate
    //    };

    //    _decoder = new OpusDecoder(48000, 1);
    //}

    //private void OnDisable()
    //{
    //    _encoder.Dispose();
    //    _decoder.Dispose();
    //}

    private float[] ByteToFloat(byte[] data, int count)
    {
        var output = new float[count / 2];

        for (int i = 0; i < count - 1; i += 2)
        {
            var b1 = data[i];
            var b2 = data[i + 1];

            float sample = (float)(b1 | (b2 << 8));
            output[i / 2] = sample / 32767;
        }

        return output;
    }

    private byte[] FloatToByte(float[] data)
    {
        var output = new byte[data.Length * 2];

        for (int i = 0; i < data.Length; i++)
        {
            var sample = data[i];

            sample *= 32767;
            int isample = (int)sample;
            byte b1 = (byte)(isample & 0x0f);
            byte b2 = (byte)(isample >> 8);

            output[i * 2] = b1;
            output[i * 2 + 1] = b2;
        }

        return output;
    }

    /*
    // Update is called once per frame
    void Update()
    {
        //Debug.Log(_clips[0].length);

        //Debug.Log(Microphone.GetPosition(Microphone.devices[0]).ToString());

        var key = KeyCode.E;

        if (Input.GetKeyDown(key))
        {
            Debug.Log("Starting Recording...");
            _currentBuffer = 0;
            _micReader.Clear(); 
            _encPos = 0;
            _encDataList.Clear();
        }
        if (Input.GetKey(key))
        {
            while (true)
            {
                var buffer = _sampleBuffers[_currentBuffer];
                if (!_micReader.ReadSamples(buffer.Length, 0, buffer))
                    break;
                //Debug.Log($"Filled buffer {_currentBuffer}");
                _currentBuffer++;

                var encbuf = new byte[1024];

                var pcmbytes = FloatToByte(buffer);
                int bytesEncoded = _encoder.Encode(pcmbytes, pcmbytes.Length, encbuf, encbuf.Length);

                System.Buffer.BlockCopy(encbuf, 0, _encbuffer, _encPos, bytesEncoded);
                //encbuf.CopyTo(_encbuffer, _encPos, );
                _encPos += bytesEncoded;
            }
            //// 40 ms of silence at 48 KHz (1 channel).
            //byte[] inputPCMBytes = new byte[40 * 48000 / 1000 * 2 * 1];
            //byte[] opusBytes = _encoder.Encode(inputPCMBytes, inputPCMBytes.Length, out int encodedLength);
            //byte[] outputPCMBytes = _decoder.Decode(opusBytes, encodedLength, out int decodedLength);
        }
        if (Input.GetKeyUp(key) && _currentBuffer > 0)
        {
            Debug.Log($"Filled {_currentBuffer} buffers, playing...");
            var output = new float[_currentBuffer * SAMPLE_BUFFER_SIZE];
            for (int i = 0; i < _currentBuffer; i++)
            {
                var buffer = _sampleBuffers[i];
                buffer.CopyTo(output, i * SAMPLE_BUFFER_SIZE);
            }


            var declen = _decoder.Decode(_encbuffer, _encPos, _decbuffer, _decbuffer.Length);
            var decaudio = ByteToFloat(_decbuffer, declen);

            var clip = AudioClip.Create("test", output.Length, 1, 48000, false);
            //clip.SetData(output, 0);
            clip.SetData(decaudio, 0);
            _audioSource.clip = clip;
            _audioSource.Play();
        }
    } */
}
