using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using Unity.Networking.Transport.Error;
using Google.Protobuf;
using Unity.Jobs;

public class TransportSendQueue : System.IDisposable
{

    private Queue<PacketData> _packetQueue;
    private Queue<NativeArray<byte>> _bufferQueue;

    private int _maxPacketSize;
    private int _maxQueueSize;
    private bool _disposed;
    private int _failureCount = 0;

    public int Count
    {
        get { return _packetQueue.Count; }
    }

    private struct PacketData
    {
        public NetworkPipeline pipeline;        
        public NativeArray<byte> data;
        public int length;
        public NetworkConnection conn;
    };

    public TransportSendQueue(int maxPacketSize, int maxQueueSize)
    {
        _maxPacketSize = maxPacketSize;
        _maxQueueSize = maxQueueSize;

        _packetQueue = new Queue<PacketData>();
        _bufferQueue = new Queue<NativeArray<byte>>();

        for (int i = 0; i < maxQueueSize; i++)
        {
            var buffer = new NativeArray<byte>(_maxPacketSize, Allocator.Persistent);
            _bufferQueue.Enqueue(buffer);
        }
    }

    private bool CheckBufferAvailable()
    {
        if (_bufferQueue.Count <= 0)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            Debug.LogError($"TransportSendQueue: Queue full! Packet has been dropped {st.ToString()}");
            return false;
        }
        return true;
    }

    public void Enqeuue(NetworkPipeline pipeline, NativeArray<byte> data, NetworkConnection conn)
    {
        if (data == null || data.Length <= 0)
            return;

        if (!CheckBufferAvailable())
            return;

        var buffer = _bufferQueue.Dequeue();

        try
        {
            if (data.Length > _maxPacketSize)
                throw new System.Exception($"Packet size exceeds maximum of {_maxPacketSize}");

            var subArray = buffer.GetSubArray(0, data.Length);
            subArray.CopyFrom(data);
            
            var packet = new PacketData
            {
                pipeline = pipeline,
                data = buffer,
                length = data.Length,
                conn = conn,
            };

            _packetQueue.Enqueue(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"TransportSendQueue: Error enqueing packet {ex.Message} {ex.StackTrace}");
            _bufferQueue.Enqueue(buffer);
        }

    }

    public void Enqeuue(NetworkPipeline pipeline, byte[] data, int length, NetworkConnection conn)
    {
        if (data == null || data.Length < length || length <= 0)
            return;

        if (!CheckBufferAvailable())
            return;

        var buffer = _bufferQueue.Dequeue();
        try
        {
            buffer.CopyFrom(data);
            var packet = new PacketData
            {
                pipeline = pipeline,
                data = buffer,
                length = length,
                conn = conn,
            };

            _packetQueue.Enqueue(packet);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"TransportSendQueue: Error enqueing packet {ex.Message} {ex.StackTrace}");
            _bufferQueue.Enqueue(buffer);
        }
    }

    public void SendQueuedPackets(NetworkDriver driver)
    {
        //Debug.Log($"PacketQueue Count: {_packetQueue.Count}");
        if (_packetQueue.Count <= 0)
            return;

        int sendCount = 0;

        while (_packetQueue.Count > 0)
        {
            var packet = _packetQueue.Peek();
            int result = 0;

            try
            {
                //Debug.Log($"Sending queued packet {packet.length} bytes");
                if (!Send(driver, packet.pipeline, packet.data, packet.length, packet.conn, out result))
                {
                    if (_failureCount < 100)
                    {
                        //error sending packet, try again next frame
                        //Debug.Log($"TransportSendQueue sent {sendCount} with {_packetQueue.Count} remaining packets");
                        _failureCount++;
                        return;
                    }
                    else
                    {
                        //too many failures, skip this packet
                        var status = (StatusCode)result;
                        Debug.LogError($"TransportSendQueue: Couldn't send packet size: {packet.length} error: {status.ToString()}");
                    }
                }

                //remove from queue and return buffer to pool
                _packetQueue.Dequeue();
                _bufferQueue.Enqueue(packet.data);
                _failureCount = 0;
                sendCount++;

            }
            catch (System.Exception ex)
            {
                Debug.LogError($"TransportSendQueue: Error sending packet: {ex.Message} {ex.StackTrace}");
            }

        }
    }

    private bool Send(NetworkDriver driver, NetworkPipeline pipeline, 
        NativeArray<byte> buffer, int length, NetworkConnection conn, out int result)
    {
        //int result;
        var slice = buffer.GetSubArray(0, length);

        DataStreamWriter writer;
        result = driver.BeginSend(pipeline, conn, out writer);
        //Debug.Log($"BeginSend: {result}");
        if (!CheckResult(result))
            return false;

        if (!writer.WriteBytes(slice))
        {
            driver.AbortSend(writer);
            return false;
        }

        result = driver.EndSend(writer);
        //Debug.Log($"EndSend: {result}");
        if (!CheckResult(result))
            return false;

        return true;
    }

    private bool CheckResult(int result)
    {
        if (result < 0)
        {
            var statusCode = (StatusCode)result;
            //throw new System.Exception($"Network error {statusCode.ToString()}");
            return false;
        }

        return true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            while (_bufferQueue.Count > 0)
            {
                var buffer =_bufferQueue.Dequeue();
                buffer.Dispose();
            }

            while (_packetQueue.Count > 0)
            {
                var packet = _packetQueue.Dequeue();
                packet.data.Dispose();
            }

            
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TransportSendQueue()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        System.GC.SuppressFinalize(this);
    }
}
