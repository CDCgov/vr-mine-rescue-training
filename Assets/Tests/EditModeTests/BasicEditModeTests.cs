using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

public class BasicEditModeTests
{

    [Test]
    public void BasicEditModeTestsSimplePasses()
    {
        // Use the Assert class to test conditions.
    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator BasicEditModeTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
    }

    [Test]
    public void TestSystemManager()
    {
        var res1 = Resources.Load<SystemManager>("Managers/SystemManager");
        var res2 = Resources.Load<SystemManager>("Managers/SystemManager");

        res1.TestString = "Res1";

        Assert.AreEqual(res1.TestString, res2.TestString);
    }

    [Test]
    public void TestTwistAngleCalculation()
    {
        const float testAngle = 45;
        float angle = 0;

        Quaternion q = Quaternion.AngleAxis(testAngle, Vector3.up);

        angle = Util.TwistAngle(q, Vector3.up);
        Debug.Log($"Test1: {angle:F2}");
        //Assert.That(angle, Is.EqualTo(testAngle).Within(0.01f));

        angle = Util.TwistAngle(q, Vector3.forward);
        Debug.Log($"Test2: {angle:F2}");
        //Assert.That(angle, Is.EqualTo(0).Within(0.01f));

        q = q * Quaternion.AngleAxis(45, Vector3.right);

        angle = Util.TwistAngle(q, Vector3.up);
        Debug.Log($"Test3: {angle:F2}");
        //Assert.That(angle, Is.EqualTo(testAngle).Within(0.01f));

        angle = Util.TwistAngle(q, Vector3.forward);
        Debug.Log($"Test4: {angle:F2}");
        //Assert.That(angle, Is.EqualTo(0).Within(0.01f));
    }

    [Test]
    public void TestLODRegex()
    {
        Regex lodReg = new Regex("^.*?LOD(\\d+)");

        var match = lodReg.Match("Test_Mesh_Name_LOD2_Mesh_LOD7");
        Assert.That(match.Success, Is.True);
        Assert.That(match.Groups[1].Value, Is.EqualTo("2"));

        match = lodReg.Match("GroupName_LOD2_Mesh");
        Assert.That(match.Success, Is.True);
        Assert.That(match.Groups[1].Value, Is.EqualTo("2"));

        match = lodReg.Match("GroupName_LOD2");
        Assert.That(match.Success, Is.True);
        Assert.That(match.Groups[1].Value, Is.EqualTo("2"));

        match = lodReg.Match("GroupName_LOD");
        Assert.That(match.Success, Is.False);
    }

    [Test]
    public void TestMapGridSerialize()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        MapGrid mapGrid = new MapGrid();
        mapGrid.GridOrigin = new Vector2(Random.value, Random.value);

        for (int x = 0; x < mapGrid.Width; x++)
        {
            for (int z = 0; z < mapGrid.Height; z++)
            {
                mapGrid.SetOccupied(x, z, Random.value > 0.5f ? true : false);
            }
        }

        ///////////////////////////////////Serialization Test/////////////////////
        using (MemoryStream memStream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(memStream, Encoding.UTF8, true))
            {
                sw.Start();
                mapGrid.Write(writer);
                sw.Stop();

                Debug.Log($"Writing map grid took {sw.ElapsedMilliseconds}ms");
            }

            memStream.Seek(0, SeekOrigin.Begin);

            //var originalGrid = mapGrid.GridData;            

            using (BinaryReader reader = new BinaryReader(memStream, Encoding.UTF8, true))
            {
                sw.Reset();
                sw.Start();
                var newGrid = new MapGrid(false);
                newGrid.Read(reader);
                sw.Stop();

                Debug.Log($"Reading map grid took {sw.ElapsedMilliseconds}ms");

                Assert.That(mapGrid.Width, Is.EqualTo(newGrid.Width));
                Assert.That(mapGrid.Height, Is.EqualTo(newGrid.Height));
                Assert.That(mapGrid.GridOrigin, Is.EqualTo(newGrid.GridOrigin));

                for (int x = 0; x < mapGrid.Width; x++)
                {
                    for (int z = 0; z < mapGrid.Height; z++)
                    {
                        var o1 = mapGrid.GetOccupied(x, z);
                        var o2 = newGrid.GetOccupied(x, z);

                        Assert.That(o1, Is.EqualTo(o2));
                    }
                }
            }


        }
    }
}
