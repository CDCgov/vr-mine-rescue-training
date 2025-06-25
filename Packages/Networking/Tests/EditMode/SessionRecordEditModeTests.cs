using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Google.Protobuf;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    public class SessionRecordEditModeTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void MFireSerializationTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator MFireSerializationTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }


        [Test]
        public void TestCodedInputStream()
        {
            VRNTransformData source = new VRNTransformData();
            VRNTransformData test = new VRNTransformData();

            var sourcePos = new Vector3(3, 5, 7);
            var sourceRot = Quaternion.Euler(10, 20, 30);

            source.Position = sourcePos.ToVRNVector3();
            source.Rotation = sourceRot.ToVRNQuaternion();

            MemoryStream memStream;
            using (memStream = new MemoryStream(1000))
            {
                source.WriteDelimitedTo(memStream);
                memStream.Position = 0;

                CodedInputStream reader = new CodedInputStream(memStream, true);

                reader.ReadMessage(test);

                Assert.That(test.Position != null);
                Assert.That(test.Rotation != null);

                var testPos = test.Position.ToVector3();
                var testRot = test.Rotation.ToQuaternion();
                
                Assert.That(Vector3.Distance(testPos, sourcePos), Is.LessThan(0.01f));
                Assert.That((1.0f - Quaternion.Dot(sourceRot, testRot)), Is.LessThan(0.01f));
            }
        }

        [Test]
        public void TestReadByteString()
        {
            using (var memStream = new MemoryStream(1000))
            {
                //var binWriter = new BinaryWriter(memStream, System.Text.Encoding.UTF8, true);
                double source = 29837.3756;
                var sourceBytes = System.BitConverter.GetBytes(source);

                memStream.Write(sourceBytes, 0, sourceBytes.Length);
                memStream.Flush();
                memStream.Position = 0;
                Debug.Log($"Wrote {sourceBytes.Length} bytes to stream");

                CodedInputStream reader = new CodedInputStream(memStream, true);
                var dbl = reader.ReadDouble();
                Debug.Log($"Read double value {dbl}");

                //memStream.Position = 0;
                //reader = new CodedInputStream(memStream, true);
                //ByteString testByteString = reader.ReadBytes();
                
                //var testBytes = testByteString.ToByteArray();

                //Assert.That(testByteString.Length, Is.EqualTo(sourceBytes.Length));
                //Assert.That(testByteString.Length, Is.EqualTo(testBytes.Length));

                //var test = System.BitConverter.ToDouble(testBytes, 0);
                //var diff = test - source;
                //Assert.That(diff, Is.LessThan(0.01));
            }
        }

        [Test]
        public void TestLogWriting()
        {
            using (SessionLog log = new SessionLog())
            {

                VRNLogHeader header = new VRNLogHeader();
                header.ActiveScene = "TestSceneName";
                header.SessionName = "TestSessionName";
                header.TeleportTarget = "TestTeleportTarget";

                log.CreateLog("test-log.vrminelog", header);

                VRNTextMessage msg = new VRNTextMessage();

                msg.Message = "TestMessage1";
                log.WriteLog(VRNPacketType.TextMessage, msg, true);

                msg.Message = "TestMessage2";
                log.WriteLog(VRNPacketType.TextMessage, msg, true);

                msg.Message = "TestMessage3";
                log.WriteLog(VRNPacketType.TextMessage, msg, true);

                log.CloseLog();
            }
        }

        [Test]
        public void TestLogWriteMemStream()
        {
            SessionLog log = new SessionLog();
            var filename = "test-log.vrminelog";

            VRNLogHeader header = new VRNLogHeader();
            header.ActiveScene = "TestSceneName";
            header.SessionName = "TestSessionName";
            header.TeleportTarget = "TestTeleportTarget";

            log.CreateLog(filename, header);

            VRNTextMessage msg = new VRNTextMessage();

            MemoryStream memStream = new MemoryStream();
            var leadingData = System.Text.Encoding.UTF8.GetBytes("INVALID");
            var testData = System.Text.Encoding.UTF8.GetBytes("HelloWorld");

            memStream.Write(leadingData, 0, leadingData.Length);
            var pos = memStream.Position;
            memStream.Write(testData, 0, testData.Length);

            //move to start of real data
            memStream.Position = pos;

            //write log
            log.WriteLog(VRNPacketType.Unknown, memStream, true);
            log.CloseLog();
            log.Dispose();

            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            VRNLogHeader.Parser.ParseDelimitedFrom(fs);

            var vrnHeader = VRNHeader.Parser.ParseDelimitedFrom(fs);
            Debug.Log($"Read message header {vrnHeader.PacketType.ToString()} size {vrnHeader.PacketSize}");

            byte[] data = new byte[vrnHeader.PacketSize];
            fs.Read(data, 0, vrnHeader.PacketSize);

            var str = System.Text.Encoding.UTF8.GetString(data);
            Debug.Log($"Read data {str}");

            Assert.That(str, Is.EqualTo("HelloWorld"));
            fs.Dispose();
        }

        [Test]
        public void TestLogRead()
        {

            Debug.Log(Directory.GetCurrentDirectory());

            TestLogWriting();

            SessionLog log = new SessionLog();
            log.LoadLog("test-log.vrminelog");

            Debug.Log($"Number of players found: {log.NumPlayers}");

            Assert.That(log.MessageCount, Is.EqualTo(3));
            Assert.That(log.LogHeader.SessionName, Is.EqualTo("TestSessionName"));
        }

        //[UnityTest]
        //public IEnumerator TestLogReadAsync()
        //{
        //    CancellationTokenSource cancelSource = new CancellationTokenSource();
        //    Debug.Log("Starting load...");

        //    var logTask = SessionLog.LoadLogAsync("ObjectInteraction3.vrminelog",
        //        (progress) => Debug.Log($"Progress: {progress:F2}"),
        //        cancelSource.Token, false);

        //    while (!logTask.IsCompleted)
        //    {
        //        yield return new WaitForSecondsRealtime(0.1f);
        //    }

        //    var log = logTask.Result;

        //    Debug.Log($"loaded message count: {log.MessageCount}");
        //}


        //[Test]
        //public async void TestLogReadAsyncCancel()
        //{
        //    bool caughtCancel = false;
        //    try
        //    {
        //        CancellationTokenSource cancelSource = new CancellationTokenSource();
        //        Debug.Log("Starting load...");

        //        var task = SessionLog.LoadLogAsync("session-log-test3.vrminelog",
        //            (progress) => Debug.Log($"Progress: {progress:F2}"),
        //            cancelSource.Token, false);

        //        await Task.Delay(0);

        //        cancelSource.Cancel();
        //        await task;
        //    }
        //    catch (TaskCanceledException ex)
        //    {
        //        Debug.Log($"Caught task cancelled exception: {ex.Message}");
        //        caughtCancel = true;
        //    }

        //    Assert.IsTrue(caughtCancel);

        //}

        [Test]
        public void TestPlayerRepresentationTimeSeries()
        {
            SessionTimeSeries<PlayerRepresentation> ts = new SessionTimeSeries<PlayerRepresentation>();

            PlayerRepresentation pr = new PlayerRepresentation();
            pr.Head = new ObjectPositionData();
            pr.LeftController = new ObjectPositionData();
            pr.RightController = new ObjectPositionData();
            pr.RigOffset = new ObjectPositionData();

            pr.LeftController.Rotation = Quaternion.identity;
            pr.RightController.Rotation = Quaternion.identity;
            pr.RigOffset.Rotation = Quaternion.identity;
            pr.CalibrationRot = Quaternion.identity;

            for (int i = 0; i < 100; i++)
            {
                pr.Head.Position = new Vector3(i, i, i);
                pr.Head.Rotation = Quaternion.identity;

                PlayerRepresentation entry = new PlayerRepresentation();
                pr.CopyTo(entry);
                ts.AddSequentialEntry(entry, i);
            }

            PlayerRepresentation result = new PlayerRepresentation();
            Vector3 diff;

            if (!ts.InterpolateData(5, ref result))
                throw new System.Exception($"Couldn't find entry for ts 5");

            Debug.Log(result.Head.Position);
            diff = result.Head.Position - new Vector3(5, 5, 5);
            Assert.That(diff.magnitude, Is.LessThan(0.05f));

            if (!ts.InterpolateData(10.5f, ref result))
                throw new System.Exception($"Couldn't find entry for ts 10.5");

            Debug.Log(result.Head.Position);
            Vector3 v1 = new Vector3(10, 10, 10);
            Vector3 v2 = new Vector3(11, 11, 11);
            Vector3 expected = Vector3.Lerp(v1, v2, 0.5f);
            diff = expected - result.Head.Position;
            //Assert.That(diff.magnitude, Is.LessThan(0.05f));
        }

        void WriteTestLog()
        {
            SessionLog log = new SessionLog();

            VRNLogHeader header = new VRNLogHeader();
            header.ActiveScene = "TestSceneName";
            header.SessionName = "TestSessionName";
            header.TeleportTarget = "StartingTeamStop";

            log.CreateLog("test-log.vrminelog", header);

            VRNTextMessage msg = new VRNTextMessage();
            VRNTeleportAll teamstop = new VRNTeleportAll();

            msg.Message = "TestMessage1";
            log.WriteLog(VRNPacketType.TextMessage, msg, true);

            
            teamstop.TeleportTarget = "TeamStop1";
            log.WriteLog(VRNPacketType.TeleportAll, teamstop, true);

            msg.Message = "TestMessage2";
            log.WriteLog(VRNPacketType.TextMessage, msg, true);


            teamstop.TeleportTarget = "TeamStop2";
            log.WriteLog(VRNPacketType.TeleportAll, teamstop, true);

            msg.Message = "TestMessage3";
            log.WriteLog(VRNPacketType.TextMessage, msg, true);


            teamstop.TeleportTarget = "TeamStop3";
            log.WriteLog(VRNPacketType.TeleportAll, teamstop, true);

            log.CloseLog();
        }

        [Test]
        public void TestTeamstopList()
        {
            WriteTestLog();

            using (SessionLog log = new SessionLog())
            {
                log.LoadLog("test-log.vrminelog");



            }
        }


        [Test]
        public void TestFolderScan()
        {
            var systemManager = SystemManager.GetDefault();
            var logs = SessionLog.ScanFolder(systemManager.SystemConfig.SessionLogsFolder);
            if (logs == null)
            {
                Debug.Log($"No log files found");
                return;
            }

            foreach (var metadata in logs)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine($"Found log: {metadata.Filename}");
                sb.AppendLine($"session: {metadata.SessionName}");
                sb.AppendLine($"scene: {metadata.SceneName}");
                sb.AppendLine($"teleport: {metadata.TeleportTarget}");
                sb.AppendLine($"duration: {metadata.Duration:F1}");
                sb.AppendLine($"num messages: {metadata.NumMessages}");
                if (metadata.LogStartTime != null)
                {
                    sb.AppendLine($"Found Log start time: {metadata.LogStartTime.ToString()}");
                }

                Debug.Log(sb.ToString());
                //Debug.Log($"Found log {metadata.SceneName}::{metadata.Duration:F1}::{metadata.NumMessages}");
            }
        }

        private void InitializeRandomTransform(Transform t)
        {
            t.localPosition = Random.insideUnitSphere * Random.Range(0, 10);
            t.localRotation = Random.rotation;
        }

        [Test]
        public void TestTransformCalculation()
        {
            //Compute transform to POI space (POI Anchor-> Rig -> CalOffset-> HeadPos)
            var anchor = new GameObject("anchor").transform;
            var rig = new GameObject("rig").transform;
            var offset = new GameObject("offset").transform;
            var head = new GameObject("head").transform;

            head.SetParent(offset);
            offset.SetParent(rig);
            rig.SetParent(anchor);

            InitializeRandomTransform(anchor);
            InitializeRandomTransform(rig);
            InitializeRandomTransform(offset);
            InitializeRandomTransform(head);

            var headMat = Matrix4x4.TRS(head.localPosition, head.localRotation, Vector3.one);
            var offsetMat = Matrix4x4.TRS(offset.localPosition, offset.localRotation, Vector3.one);
            var rigMat = Matrix4x4.TRS(rig.localPosition, rig.localRotation, Vector3.one);
            var anchorMat = Matrix4x4.TRS(anchor.localPosition, anchor.localRotation, Vector3.one);

            Debug.Log($"Head Pos {head.position} local: {head.localPosition}");

            //single matrix calculation
            var mat = anchorMat * rigMat * offsetMat;
            var pos = mat.MultiplyPoint(head.localPosition);
            var dist = Vector3.Distance(pos, head.position);
            Debug.Log($"Single Calculated: {pos} distance: {dist}");
            Assert.That(dist, Is.LessThan(0.01f));


            //defer anchor matrix
            var matToAnchor = rigMat * offsetMat;
            mat = anchorMat * matToAnchor;
            pos = mat.MultiplyPoint(head.localPosition);
            
            dist = Vector3.Distance(pos, head.position);
            Debug.Log($"Defer Calculated: {pos} distance: {dist}");
            Assert.That(dist, Is.LessThan(0.01f));

            //calculate anchor space matrix
            matToAnchor = rigMat * offsetMat;
            
            pos = matToAnchor.MultiplyPoint(head.localPosition);
            pos = anchor.TransformPoint(pos);

            dist = Vector3.Distance(pos, head.position);
            Debug.Log($"Anchor Calculated: {pos} distance: {dist}");
            Assert.That(dist, Is.LessThan(0.01f));


            GameObject.DestroyImmediate(anchor.gameObject);
            /*GameObject.DestroyImmediate(rig.gameObject);
            GameObject.DestroyImmediate(offset.gameObject);
            GameObject.DestroyImmediate(head.gameObject);*/
        }

    }
}
