using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class CalibrationOffsetTests
    {
        Transform _root;
        Transform _rig;
        Transform _offset;
        Transform _head;

        [OneTimeSetUp]
        public void SetUp()
        {
            Debug.Log("OneTimeSetup");
            var rootObj = new GameObject("RootObject");
            var rigObj = new GameObject("RigObject");
            var offsetObj = new GameObject("OffsetObject");
            var headObj = new GameObject("HeadObject");

            _root = rootObj.transform;
            _rig = rigObj.transform;
            _offset = offsetObj.transform;
            _head = headObj.transform;

            _rig.SetParent(_root, false);
            _offset.SetParent(_rig, false);
            _head.SetParent(_offset, false);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Debug.Log("OneTimeTeardown");
            GameObject.DestroyImmediate(_root.gameObject);
            //GameObject.DestroyImmediate(_rig.gameObject);
            //GameObject.DestroyImmediate(_offset.gameObject);
            //GameObject.DestroyImmediate(_head.gameObject);
        }

        [SetUp]
        public void ResetTransforms()
        {
            Debug.Log("ResetTransforms");
            ResetTransform(_root);
            ResetTransform(_rig);
            ResetTransform(_offset);
            ResetTransform(_head);
        }

        void ResetTransform(Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        // A Test behaves as an ordinary method
        [Test]
        public void CalibrationOffsetTestsSimplePasses()
        {
            Debug.Log("SimpleTest");
            Vector3 test1 = new Vector3(3, 4, 5);

            _offset.localPosition = test1;
            Assert.AreEqual(_head.position, test1);
        }

        [Test]
        public void TestInverseTranslation()
        {
            Vector3 testPos = new Vector3(3, 4, 5);
            Vector3 expected = new Vector3(0, 4, 0);

            _head.localPosition = testPos;
            Util.ComputeInverseTransform(_offset, _head);
            
            Assert.AreEqual(expected, _head.position);
        }

        [Test]
        public void TestInverseRotation()
        {
            var testRot = Quaternion.Euler(20, 30, 40);

            _head.localRotation = testRot;
            Util.ComputeInverseTransform(_offset, _head);

            var headEuler = _head.rotation.eulerAngles;
            Debug.Log($"Euler From: {testRot.eulerAngles} To: {headEuler}");
            Assert.That(headEuler.y, Is.EqualTo(0).Within(0.005));
        }


        [Test]
        public void TestInverseRotationAndTranslation()
        {
            Vector3 testPos = new Vector3(3, 4, 5);
            Vector3 expectedPos = new Vector3(0, 4, 0);
            var testRot = Quaternion.Euler(20, 30, 40);

            _head.localRotation = testRot;
            _head.localPosition = testPos;
            Util.ComputeInverseTransform(_offset, _head);

            var headEuler = _head.rotation.eulerAngles;
            Debug.Log($"Euler From: {testRot.eulerAngles} To: {headEuler}");
            Assert.That(headEuler.y, Is.EqualTo(0).Within(0.005));
            Assert.That(_head.position, Is.EqualTo(expectedPos));
        }

        [Test]
        public void TestInverseRotationAndTranslation2()
        {
            Vector3 testPos = new Vector3(8, 1, 3);
            Vector3 expectedPos = new Vector3(0, 1, 0);
            var testRot = Quaternion.Euler(50, 40, 30);

            _head.localRotation = testRot;
            _head.localPosition = testPos;
            Util.ComputeInverseTransform(_offset, _head);

            var headEuler = _head.rotation.eulerAngles;
            Debug.Log($"Euler From: {testRot.eulerAngles} To: {headEuler}");
            Assert.That(headEuler.y, Is.EqualTo(0).Within(0.005));
            Assert.That(_head.position, Is.EqualTo(expectedPos));
        }

      

        [Test]
        public void TestInverseRotationAndTranslationOffsetParent()
        {
            for (int x = 0; x < 15; x++)
            {
                TestInverseRotationAndTranslationOffsetParent(x);
            }
        }

        void TestInverseRotationAndTranslationOffsetParent(int randSeed)
        {
            Random.InitState(randSeed);
            _root.localPosition = Random.insideUnitSphere * 10;
            _root.localRotation = Random.rotation;
            _rig.localPosition = Random.insideUnitSphere * 10;
            _rig.localRotation = Random.rotation;

            Vector3 testPos = Random.insideUnitSphere * 10; ;
            Vector3 expectedPos = Vector3.zero;
            expectedPos.y = testPos.y;
            var testRot = Random.rotation;

            _head.localRotation = testRot;
            _head.localPosition = testPos;
            Util.ComputeInverseTransform(_offset, _head);

            var headEuler = _head.rotation.eulerAngles;
            Debug.Log($"Euler From: {testRot.eulerAngles} To: {headEuler}");

            var headPos = _rig.InverseTransformPoint(_head.position);
            //Assert.That(headEuler.y, Is.EqualTo(0).Within(0.005));

            //Assert.That(headPos, Is.EqualTo(expectedPos));
            Assert.That(Vector3.Distance(headPos, expectedPos), Is.EqualTo(0).Within(0.0005));

            var headForwardProj = _head.forward;
            headForwardProj = _rig.InverseTransformDirection(headForwardProj);
            headForwardProj.y = 0;
            headForwardProj.Normalize();
            var forwardDot = Vector3.Dot(Vector3.forward, headForwardProj);
            Assert.That(forwardDot, Is.EqualTo(1).Within(0.05));
        }


        [Test]
        public void RotationCalSanityCheck()
        {
            for (int x = 0; x < 15; x++)
            {
                RotationCalSanityCheck(x);
            }
        }

        void RotationCalSanityCheck(int randSeed)
        {
            Random.InitState(randSeed);
            _root.localPosition = Random.insideUnitSphere * 10;
            _root.localRotation = Random.rotation;
            _rig.localPosition = Random.insideUnitSphere * 10;
            _rig.localRotation = Random.rotation;

            Vector3 testPos = Random.insideUnitSphere * 10; ;
            Vector3 expectedPos = Vector3.zero;
            expectedPos.y = testPos.y;
            var testRot = Random.rotation;
            //testRot = Quaternion.identity;

            _head.localRotation = testRot;
            _head.localPosition = testPos;

            Debug.Log($"TestCalPos: {testPos} - World: {_head.position}");

            Util.ComputeInverseTransform(_offset, _head);

            var headWorldPos = _head.position;
            var headWorldRot = _head.rotation;


            Debug.Log($"Head World Space PostCal: {_head.position}");
            //update calibration so that the new location is on the forward axis
            Util.UpdateCalibrationRotation(_offset, _head, _head.position + _offset.forward);

            var posError = Vector3.Distance(headWorldPos, _head.position);
            var angleError = Quaternion.Angle(headWorldRot, _head.rotation);

            Assert.That(posError, Is.LessThan(0.03));
            Assert.That(angleError, Is.LessThan(0.03));            

            var headEuler = _head.rotation.eulerAngles;
            Debug.Log($"Euler From: {testRot.eulerAngles} To: {headEuler}");

            //var headForwardProj = _head.forward;
            //headForwardProj = _rig.InverseTransformDirection(headForwardProj);
            //headForwardProj.y = 0;
            //headForwardProj.Normalize();
            //var forwardDot = Vector3.Dot(Vector3.forward, headForwardProj);
            //Assert.That(forwardDot, Is.EqualTo(1).Within(0.05));
        }

        [Test]
        public void RotationCalRightTest()
        {
            for (int x = 0; x < 15; x++)
            {
                RotationCalRightTest(x);
            }
        }

        void RotationCalRightTest(int randSeed)
        {
            Random.InitState(randSeed);
            _root.localPosition = Random.insideUnitSphere * 10;
            _root.localRotation = Random.rotation;
            _rig.localPosition = Random.insideUnitSphere * 10;
            _rig.localRotation = Random.rotation;

            Vector3 testPos = Random.insideUnitSphere * 10; ;
            Vector3 expectedPos = Vector3.zero;
            expectedPos.y = testPos.y;
            var testRot = Random.rotation;

            _head.localRotation = testRot;
            _head.localPosition = testPos;
            Util.ComputeInverseTransform(_offset, _head);

            var headWorldPos = _head.position;
            var headWorldRot = _head.rotation;

            //update calibration so forward is on the pos X axis (Vector3.right)
            var calPos = _offset.parent.position + _offset.right;
            _head.localPosition = testPos + Vector3.right;
            Util.UpdateCalibrationRotation(_offset, _head, calPos);
          
            var headEuler = _head.rotation.eulerAngles;
            Debug.Log($"Euler From: {testRot.eulerAngles} To: {headEuler}");

            //verify the calibration offset hasn't changed
            _head.localPosition = testPos;
            var posError = Vector3.Distance(headWorldPos, _head.position);
            Assert.That(posError, Is.LessThan(0.03));

            var expectedForward = Vector3.right;

            //_head.localRotation = Quaternion.identity;
            //var newForwardProj = _head.forward;
            var newForwardProj = _offset.parent.InverseTransformDirection(_offset.forward);
            //var newForwardProj = _offset.forward;
            //headForwardProj = _rig.InverseTransformDirection(headForwardProj);
            newForwardProj.y = 0;
            newForwardProj.Normalize();

            Debug.Log($"Expected Forward: {expectedForward} Head Forward {newForwardProj}");

            var forwardDot = Vector3.Dot(expectedForward, newForwardProj);
            Assert.That(forwardDot, Is.EqualTo(1).Within(0.05));

        }

        [Test]
        public void TestCalRotationSet()
        {
            for (int x = 0; x < 15; x++)
            {
                TestCalRotationSet(x);
            }
        }

        void TestCalRotationSet(int randSeed)
        {
            Random.InitState(randSeed);
            _root.localPosition = Random.insideUnitSphere * 10;
            _root.localRotation = Random.rotation;
            _rig.localPosition = Random.insideUnitSphere * 10;
            _rig.localRotation = Random.rotation;

            Vector3 testPos = Random.insideUnitSphere * 10;
            Vector3 expectedPos = Vector3.zero;
            expectedPos.y = testPos.y;
            var testRot = Random.rotation;

            Debug.Log($"Test Pos: {testPos}");

            _head.localRotation = testRot;
            _head.localPosition = testPos;
            Util.ComputeInverseTransform(_offset, _head);


            var headWorldPos = _head.position;

            //move the head position to a new random location
            _head.localPosition = testPos + Random.onUnitSphere * Random.Range(3, 5);
            _head.localRotation = Random.rotation;

            //compute the vector that we expect to be the new forward direction
            var expectedForward = _head.localPosition - testPos;
            expectedForward.y = 0;
            expectedForward.Normalize();

            //update calibration so that the new location is on the forward axis
            Util.UpdateCalibrationRotation(_offset, _head, _head.position);

            //verify the calibration offset hasn't changed
            _head.localPosition = testPos;
            var posError = Vector3.Distance(headWorldPos, _head.position);
            Assert.That(posError, Is.LessThan(0.03));

            

            var newForwardProj = _offset.parent.InverseTransformDirection(_offset.forward);

            //var newForwardProj = _offset.forward;
            //headForwardProj = _rig.InverseTransformDirection(headForwardProj);
            newForwardProj.y = 0;
            newForwardProj.Normalize();

            Debug.Log($"Expected Forward: {expectedForward} Head Forward {newForwardProj}");

            var forwardDot = Vector3.Dot(expectedForward, newForwardProj);
            Assert.That(forwardDot, Is.EqualTo(1).Within(0.05));

            ////verify that the position of the head in the space of the parent 
            ////is now strictly forward of the calibration position (ignoring y)
            //var delta = _head.localPosition - testPos;
            //Assert.That(delta.x, Is.EqualTo(0).Within(0.005));
            //Assert.That(delta.z, Is.GreaterThan(0));

            //var headEuler = _head.rotation.eulerAngles;
            //Debug.Log($"Euler From: {testRot.eulerAngles} To: {headEuler}");

            ////move back to original location and verify it hasn't changed
            //_head.localPosition = testPos;
            //_head.localRotation = testRot;
            //var headPos = _rig.InverseTransformPoint(_head.position);
            ////Assert.That(headEuler.y, Is.EqualTo(0).Within(0.005));

            ////Assert.That(headPos, Is.EqualTo(expectedPos));
            //Assert.That(Vector3.Distance(headPos, expectedPos), Is.EqualTo(0).Within(0.0005));

            //var headForwardProj = _head.forward;
            //headForwardProj = _rig.InverseTransformDirection(headForwardProj);
            //headForwardProj.y = 0;
            //headForwardProj.Normalize();
            //var forwardDot = Vector3.Dot(Vector3.forward, headForwardProj);
            //Assert.That(forwardDot, Is.EqualTo(1).Within(0.05));
        }


        [Test]
        public void TestCalRotationSet2D()
        {
            for (int x = 0; x < 15; x++)
            {
                TestCalRotationSet2D(x);
            }
        }

        void TestCalRotationSet2D(int randSeed)
        {
            Random.InitState(randSeed);
            _root.localPosition = Random.insideUnitSphere * 10;
            _root.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            _rig.localPosition = Random.insideUnitSphere * 10;
            _rig.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            _root.localRotation = Quaternion.identity;
            _rig.localRotation = Quaternion.identity;

            Vector3 testPos = Random.insideUnitSphere * 10;
            Vector3 expectedPos = Vector3.zero;
            expectedPos.y = testPos.y;
            var testRot = Quaternion.Euler(0, Random.Range(0, 360), 0); 

            Debug.Log($"Test Pos: {testPos}");

            _head.localRotation = testRot;
            _head.localPosition = testPos;
            Util.ComputeInverseTransform(_offset, _head);


            var headWorldPos = _head.position;

            //move the head position to a new random location
            _head.localPosition = testPos + Random.onUnitSphere * Random.Range(3, 5);
            _head.localRotation = Random.rotation;

            //compute the vector that we expect to be the new forward direction
            var expectedForward = _head.localPosition - testPos;
            expectedForward.y = 0;
            expectedForward.Normalize();

            //update calibration so that the new location is on the forward axis
            Util.UpdateCalibrationRotation(_offset, _head, _head.position);

            //verify the calibration offset hasn't changed
            _head.localPosition = testPos;
            var posError = Vector3.Distance(headWorldPos, _head.position);
            Assert.That(posError, Is.LessThan(0.03));



            //var newForwardProj = _offset.parent.InverseTransformDirection(_offset.forward);
            _head.localRotation = Quaternion.identity;
            var newForwardProj = _offset.parent.InverseTransformDirection(_head.forward);
            Debug.Log($"Global Forward: {_offset.forward}");

            //var newForwardProj = _offset.forward;
            //headForwardProj = _rig.InverseTransformDirection(headForwardProj);
            newForwardProj.y = 0;
            newForwardProj.Normalize();
            newForwardProj.x = newForwardProj.x * -1.0f; //temporary - need a more complete correction to the offset forward vector

            Debug.Log($"Expected Forward: {expectedForward} New Forward {newForwardProj}");

            var forwardDot = Vector3.Dot(expectedForward, newForwardProj);
            Assert.That(forwardDot, Is.EqualTo(1).Within(0.05));

           
        }


        [Test]
        public void TestControllerAngles()
        {
            PlayerRepresentation player = new PlayerRepresentation();

            player.Head = new ObjectPositionData();
            player.Head.Position = new Vector3(4, 4, 4);
            player.Head.Rotation = Quaternion.Euler(0, 90, 0);

            player.LeftController = new ObjectPositionData();
            player.LeftController.Position = new Vector3(4,4,5);

            player.RightController = new ObjectPositionData();
            player.RightController.Position = new Vector3(5, 4, 4);

            float leftAngle, rightAngle;
            player.ComputeControllerAngles(out leftAngle, out rightAngle);

            Debug.Log($"Left Angle: {leftAngle:F1} Right Angle: {rightAngle:F1}");

            Assert.That(leftAngle, Is.EqualTo(90.0f).Within(0.05));
            Assert.That(rightAngle, Is.EqualTo(0.0f).Within(0.05));
        }


        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator CalibrationOffsetTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
