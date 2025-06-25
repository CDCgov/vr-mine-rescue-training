using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using System.Threading;

/// <summary>
/// Class that handles the file I/O of the log.
/// </summary>
public class LogEntry {

    private string _path;
    private BinaryWriter _bwLog;
    private volatile Queue<LogPacket> _packetQueue;
    
    
    private volatile bool _isWorking = false;
    private volatile bool _isAlive = true;
    private volatile bool _performFlush = false;
    private EventWaitHandle _ewh;	
    private Thread _WorkerThread;

    public LogEntry(string projectName, string dataPath)
    {
        _path = Directory.GetParent(dataPath).FullName;
        _path = Path.Combine(_path, "Logs");
        if (!Directory.Exists(_path))
        {
            Directory.CreateDirectory(_path);
        }
        _path = Path.Combine(_path, string.Format("{0}_Log_{1}.dat", projectName, DateTime.Now.ToString("yyyy-MM-ddTHHmmss")));
        _packetQueue = new Queue<LogPacket>();
        
        _ewh = new EventWaitHandle(false, EventResetMode.AutoReset);
        _WorkerThread = new Thread(new ThreadStart(WorkerLoop));
        _WorkerThread.Start();		
        StartLog();
    }

    public void WriteToLog(LogPacket data)
    {		
        lock (_packetQueue)
        {
            _packetQueue.Enqueue(data);
        }
        
        if (!_isWorking)
        {
            _isWorking = true;
            _ewh.Set();
        }
    }

    private void StartLog()
    {
        if(_bwLog != null)
        {
            _bwLog = null;
        }
        FileStream fs = new FileStream(_path, FileMode.Create,FileAccess.Write, FileShare.Write);
        _bwLog = new BinaryWriter(fs);
        
    }

    public void CloseLog(bool loggingTurnedOff = false)
    {
        if (_bwLog != null)
        {
            //while (backgroundWorker.IsBusy) { }
            _isAlive = false;
            _ewh.Set(); //In case the worker thread is waiting.
            _WorkerThread.Join(9000);
            //Clear out any remaining packets leftover on the main thread while the worker was finishing (should never happen, preventative measure)
            while(_packetQueue.Count > 0)
            {
                LogPacket packet = _packetQueue.Dequeue();
                byte[] bytes = packet.GetBytes();
                int count = bytes.Length;				
                if (_bwLog != null)
                {
                    _bwLog.Write(count);
                    _bwLog.Write(bytes, 0, count);
                    //_log.Flush();
                }
            }

            _bwLog.Flush();
            _bwLog.Close();
            _bwLog = null;

            if (loggingTurnedOff)
            {
                DestroyLog(); //Per spec, if logging is turned off no file is saved. Done this way in case logging is turned off @ runtime. -BDM
            }
        }
    }

    public void CloseLog(double timeStamp, bool loggingTurnedOff = false)
    {
        if (_bwLog != null)
        {			
            _isAlive = false;
            _ewh.Set(); //In case the worker thread is waiting.
            _WorkerThread.Join(9000);
            //Clear out any remaining packets leftover on the main thread while the worker was finishing (should never happen, preventative measure)
            while (_packetQueue.Count > 0)
            {
                LogPacket packet = _packetQueue.Dequeue();
                byte[] bytes = packet.GetBytes();
                int count = bytes.Length;
                if (_bwLog != null)
                {
                    _bwLog.Write(count);
                    _bwLog.Write(bytes, 0, count);
                    //_log.Flush();
                }
            }
            StringMessageData closingMessage = new StringMessageData();
            closingMessage.Message = "Log closed";
            closingMessage.TimeStamp = timeStamp;

            byte[] closeBytes = closingMessage.GetBytes();
            int closeCount = closeBytes.Length;
            _bwLog.Write(closeCount);
            _bwLog.Write(closeBytes, 0, closeCount);
            _bwLog.Flush();
            _bwLog.Close();
            _bwLog = null;

            if (loggingTurnedOff)
            {
                DestroyLog(); //Per spec, if logging is turned off no file is saved. Done this way in case logging is turned off @ runtime. -BDM
            }
        }
    }
    /// <summary>
    /// DestroyLog is called if logging was turned off globally. No data was written into the original log file, this is then called to delete the empty text file (the file would only have the "Started" and "Stopped" messages)
    /// </summary>
    public void DestroyLog()
    {
        File.Delete(_path);
    }

    private void WorkerLoop()
    {
        while (_isAlive)
        {
            _ewh.WaitOne();
            _isWorking = true;	
            
            while(_packetQueue.Count > 0)
            {
                LogPacket packet;

                lock (_packetQueue)
                {
                    if (_packetQueue.Count <= 0)
                        continue;

                    packet = _packetQueue.Dequeue();
                }

                byte[] bytes = packet.GetBytes();
                int count = bytes.Length;

                if (_bwLog != null)
                {
                    _bwLog.Write(count);
                    _bwLog.Write(bytes, 0, count);
                    _bwLog.Flush();
                }
            }            
            
            _isWorking = false;			
        }
    }
}
