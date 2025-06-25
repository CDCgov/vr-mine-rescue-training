using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Google.Protobuf;

namespace Tests
{
    public class MFireSerializationTests
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
        public void TestAddRemoveJunctions()
        {
            VentGraph ventGraph = new VentGraph();

            for (int i = 0; i < 5; i++)
            {
                var junc = new VentJunction();
                junc.WorldPosition = Random.onUnitSphere;

                ventGraph.AddJunction(junc);
            }

            ventGraph.RemoveJunction(2);
            Assert.That(ventGraph.NumJuncions, Is.EqualTo(4));

            //for (int i = 1; i < 5; i++)
            //{
            //    var from = ventGraph.FindJunction(i - 1);
            //    var to = ventGraph.FindJunction(i);

            //    AddTestAirway(ventGraph, from, to);
            //}

        }


        [Test]
        public void TestAddRemoveAirways()
        {
            VentGraph ventGraph = new VentGraph();
            List<int> junctionIDs = new List<int>();

            for (int i = 0; i < 5; i++)
            {
                var junc = new VentJunction();
                junc.WorldPosition = Random.onUnitSphere;
                

                int juncID = ventGraph.AddJunction(junc);
                junctionIDs.Add(juncID);
            }

            for (int i = 1; i < 5; i++)
            {
                var from = ventGraph.FindJunction(junctionIDs[i-1]);
                var to = ventGraph.FindJunction(junctionIDs[i]);

                AddTestAirway(ventGraph, from, to);
            }

            ventGraph.RemoveAirway(2);
            Assert.That(ventGraph.NumAirways, Is.EqualTo(3));

        }


        [Test]
        public void TestVentGraphMarshaling()
        {
            var ventGraph = BuildTestGraph();

            VRNVentGraph vrnGraph = new VRNVentGraph();
            ventGraph.SaveTo(vrnGraph);

            VentGraph loadedGraph = new VentGraph();
            loadedGraph.LoadFrom(vrnGraph);

            CompareGraphs(ventGraph, loadedGraph);
        }

        [Test]
        public void TestVentGraphSerialization()
        {
            var ventGraph = BuildTestGraph();

            VRNVentGraph vrnGraph = new VRNVentGraph();
            ventGraph.SaveTo(vrnGraph);

            MemoryStream ms = new MemoryStream();
            vrnGraph.WriteDelimitedTo(ms);

            ms.Seek(0, SeekOrigin.Begin);

            var vrnGraph2 = VRNVentGraph.Parser.ParseDelimitedFrom(ms);
            VentGraph loadedGraph = new VentGraph();
            loadedGraph.LoadFrom(vrnGraph2);

            CompareGraphs(ventGraph, loadedGraph);
        }

        [Test]
        public void TestFindNearbyAirways()
        {
            var ventGraph = BuildTestGraph();

            VentAirway a1, a2;
            float d1, d2;

            if (!ventGraph.FindNearbyAirways(Vector3.zero, out a1, out a2, out d1, out d2))
                throw new System.Exception("FindTwoCloestsAirways failed");

            Debug.Log($"Two Closest Airways: {a1.AirwayID} @ {d1:F2} and {a2.AirwayID} @ {d2:F2}");
        }

        private void CompareGraphs(VentGraph g1, VentGraph g2)
        {
            if (g1.NumJuncions != g2.NumJuncions)
                throw new System.Exception($"Junction count doesn't match : {g1.NumJuncions} vs {g2.NumJuncions}");
            if (g1.NumAirways != g2.NumAirways)
                throw new System.Exception($"Airway count doesn't match : {g1.NumAirways} vs {g2.NumAirways}");

            foreach (var junc in g1.GetJunctions())
            {
                var junc2 = g2.FindJunction(junc.JunctionID);
                if (junc2 == null)
                    throw new System.Exception($"Couldn't find junction {junc.JunctionID}");

            }

            foreach (var airway in g1.GetAirways())
            {
                var airway2 = g2.FindAirway(airway.AirwayID);
                if (airway2 == null)
                    throw new System.Exception($"Couldn't find airway {airway.AirwayID}");
            }
        }

        private VentGraph BuildTestGraph()
        {
            VentGraph ventGraph = new VentGraph();
            List<int> junctionIDs = new List<int>();

            for (int i = 0; i < 5; i++)
            {
                var junc = new VentJunction();
                junc.WorldPosition = Random.onUnitSphere;

                var juncID = ventGraph.AddJunction(junc);
                junctionIDs.Add(juncID);
            }

            for (int i = 1; i < 5; i++)
            {
                var from = ventGraph.FindJunction(junctionIDs[i - 1]);
                var to = ventGraph.FindJunction(junctionIDs[i]);

                AddTestAirway(ventGraph, from, to);
            }

            return ventGraph;
        }

        private void AddTestAirway(VentGraph graph, VentJunction from, VentJunction to)
        {
            var airway = new VentAirway();
            airway.Start = from;
            airway.End = to;
            airway.AddedResistance = Random.Range(0, 100);

            graph.AddAirway(airway);
        }

        [Test]
        public void TestVectorFieldModification()
        {
            Texture2D field = new Texture2D(10, 10, TextureFormat.RGFloat, false);

            int rows = 15;
            int cols = 10;

            var data = new float[rows * cols * 2];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0;
            }

            data[2] = 5;
            data[3] = 10;

            //data[3 * cols * 2 + 5 * 2] = 11;
            //data[3 * cols * 2 + 5 * 2 + 1] = 12;
            VentGraph.SetFieldData(data, cols, 5, 3, 11, 12);

            field.SetPixelData<float>(data, 0);

            Color c;
            c = field.GetPixel(0, 0);
            Debug.Log($"0,0: {c.r:F2}, {c.g:F2}, {c.b:F2}");
            Assert.That(c.r, Is.EqualTo(0));
            Assert.That(c.g, Is.EqualTo(0));


            c = field.GetPixel(1, 0);
            Debug.Log($"1,0: {c.r:F2}, {c.g:F2}, {c.b:F2}");
            Assert.That(c.r, Is.EqualTo(5));
            Assert.That(c.g, Is.EqualTo(10));

            c = field.GetPixel(0, 1);
            Debug.Log($"0,1: {c.r:F2}, {c.g:F2}, {c.b:F2}");

            c = field.GetPixel(5, 3);
            Debug.Log($"5,3: {c.r:F2}, {c.g:F2}, {c.b:F2}");
            Assert.That(c.r, Is.EqualTo(11));
            Assert.That(c.g, Is.EqualTo(12));

        }


    }
}
