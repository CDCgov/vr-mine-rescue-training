using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MineRescueTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void MineRescueTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator MineRescueTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }


        

        [Test]
        public void TestCalibrationCalculation()
        {
            var rootObj = new GameObject("RootObject");
            var rigObj = new GameObject("RigObject");
            var offsetObj = new GameObject("OffsetObject");
            var headObj = new GameObject("HeadObject");

            Vector3 test1 = new Vector3(3, 4, 5);

            try
            {
                rigObj.transform.SetParent(rootObj.transform, false);
                offsetObj.transform.SetParent(rigObj.transform, false);
                headObj.transform.SetParent(offsetObj.transform, false);

                offsetObj.transform.localPosition = test1;
                Assert.AreEqual(headObj.transform.position, test1);
            }
            finally
            {
                GameObject.DestroyImmediate(rootObj);
                GameObject.DestroyImmediate(rigObj);
                GameObject.DestroyImmediate(offsetObj);
                GameObject.DestroyImmediate(headObj);
            }
        }
    }
}
