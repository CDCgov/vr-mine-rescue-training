using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class RoofBoltEditorPlacement : MonoBehaviour 
{
    [MenuItem("CONTEXT/RoofBolt/Move Bolts to Roof")]
    public static void MoveRoofBoltsToRoof()
    {
        Object[] obj = Object.FindObjectsOfType(typeof(GameObject));
        int boltCount = 0;
        foreach(Object o in obj)
        {
            GameObject item = o as GameObject;
            RoofBolt rbRef = item.GetComponent<RoofBolt>();
            float randX = Random.Range(-0.075f, 0.075f);//Randomize placement by +/- 3" (well, 2.95 inces)
            float randZ = Random.Range(-0.075f, 0.075f);
            if (item.activeSelf && rbRef != null)
            {
                item.transform.Translate(randX, 0, randZ);
                RaycastHit hit;
                if(Physics.Raycast(item.transform.position, Vector3.up, out hit))
                {					
                    item.transform.position = hit.point;
                    Quaternion q = Quaternion.FromToRotation(item.transform.up, hit.normal * -1);
                    item.transform.rotation = q * item.transform.rotation;
                    item.transform.Translate(0, rbRef.Thickness, 0);
                    item.name = "Bolt_Single_" + boltCount;
                    boltCount++;
                }
            }
        }
        Debug.Log("Moved roofbolts up.");
    }

    [MenuItem("CONTEXT/RoofBolt/Move Bolts to Roof (No xy translation)")]
    public static void MoveRoofBoltsToRoofNoXZ()
    {
        Object[] obj = Object.FindObjectsOfType(typeof(GameObject));
        int boltCount = 0;
        foreach (Object o in obj)
        {
            GameObject item = o as GameObject;
            RoofBolt rbRef = item.GetComponent<RoofBolt>();
            float randX = 0;//Randomize placement by +/- 3" (well, 2.95 inces)
            float randZ = 0;
            if (item.activeSelf && rbRef != null)
            {
                item.transform.Translate(randX, 0, randZ);
                RaycastHit hit;
                if (Physics.Raycast(item.transform.position, Vector3.up, out hit))
                {
                    item.transform.position = hit.point + hit.normal.normalized*0.01f;
                    Quaternion q = Quaternion.FromToRotation(item.transform.up, hit.normal * -1);
                    item.transform.rotation = q * item.transform.rotation;
                    item.transform.Translate(0, rbRef.Thickness, 0);
                    item.name = "Bolt_Single_" + boltCount;
                    boltCount++;
                }
            }
        }
        Debug.Log("Moved roofbolts up.");
    }

    [MenuItem("CONTEXT/RoofBolt/Randomly Rotate Bolts")]
    public static void RotateBolts()
    {
        Object[] obj = Object.FindObjectsOfType(typeof(GameObject));
        int boltCount = 0;

        foreach(Object o in obj)
        {
            GameObject bolt = o as GameObject;
            RoofBolt rbRef = bolt.GetComponent<RoofBolt>();

            if(bolt.activeSelf && rbRef != null)
            {
                bolt.transform.Rotate(bolt.transform.up, Random.Range(-30, 0));
                boltCount++;
            }
        }
        Debug.Log("Rotated " + boltCount + " roof bolts.");
    }

    [MenuItem("CONTEXT/RoofBolt/Move THIS Bolt Up and Rotate")]
    public static void MoveThisBolt()
    {
        GameObject selectedObject = Selection.activeGameObject;
        selectedObject.transform.Rotate(selectedObject.transform.up, Random.Range(0, 30));
        
        //RoofBolt rbRef = selectedObject.GetComponent<RoofBolt>();
        Vector3 posXposZCorner = selectedObject.transform.TransformPoint(new Vector3(0.07586f, 0, 0.07586f));
        Vector3 posXnegZCorner = selectedObject.transform.TransformPoint(new Vector3(0.07586f, 0, -0.07586f));
        Vector3 negXnegZCorner = selectedObject.transform.TransformPoint(new Vector3(-0.07586f, 0, -0.07586f));
        Vector3 negXposZCorner = selectedObject.transform.TransformPoint(new Vector3(-0.07586f, 0, 0.07586f));

        RaycastHit hit;
        List<Vector3> hitNormals = new List<Vector3>();
        Vector3 savedCorner = Vector3.zero;
        Vector3 savedNormal = Vector3.zero;
        float savedDist = 9999;
        Vector3 hitPoint = Vector3.zero;
        Vector3 math = Vector3.zero;

        //Iterate through the 4 corners to find the normals of intersection
        if(Physics.Raycast(posXposZCorner, selectedObject.transform.up, out hit))
        {
            hitNormals.Add(hit.normal);
            savedCorner = posXposZCorner;
            savedDist = Vector3.Distance(posXposZCorner, hit.point);
            savedNormal = hit.normal;
            hitPoint = hit.point;
            math = new Vector3(0.07586f, 0, 0.07586f);
        }
        if (Physics.Raycast(posXnegZCorner, selectedObject.transform.up, out hit))
        {
            hitNormals.Add(hit.normal);

            
            float dist = Vector3.Distance(posXnegZCorner, hit.point);
            if (dist < savedDist)
            {
                savedDist = dist;
                savedCorner = posXnegZCorner;
                savedNormal = hit.normal;
                hitPoint = hit.point;
                math = new Vector3(0.07586f, 0, -0.07586f);
            }
            
        }
        if (Physics.Raycast(negXnegZCorner, selectedObject.transform.up, out hit))
        {
            hitNormals.Add(hit.normal);
            
            float dist = Vector3.Distance(negXnegZCorner, hit.point);
            if (dist < savedDist)
            {
                savedDist = dist;
                savedCorner = negXnegZCorner;
                savedNormal = hit.normal;
                hitPoint = hit.point;
                math = new Vector3(-0.07586f, 0, -0.07586f);
            }           
        }
        if (Physics.Raycast(negXposZCorner, selectedObject.transform.up, out hit))
        {
            hitNormals.Add(hit.normal);
            float dist = Vector3.Distance(negXposZCorner, hit.point);
            if (dist < savedDist)
            {
                savedDist = dist;
                savedCorner = negXposZCorner;
                savedNormal = hit.normal;
                hitPoint = hit.point;
                math = new Vector3(-0.07586f, 0, 0.07586f);
            }
            
            //savedDist = Vector3.Distance(negXposZCorner, hit.point);
        }

        
        //Vector3 diff = selectedObject.transform.position - savedCorner;
        selectedObject.transform.position = hitPoint - math;
        selectedObject.transform.up = -savedNormal;
        

        //Move the center point up top now
        if (Physics.Raycast(selectedObject.transform.position, selectedObject.transform.up, out hit))
        {
            
            //selectedObject.transform.position = hit.point;
            //Vector3 avg = Vector3.zero;
            //foreach(Vector3 norm in hitNormals)
            //{
            //    avg = avg + norm;
            //}
            //avg = avg / hitNormals.Count;
            //selectedObject.transform.rotation = Quaternion.Euler(avg);

            

            #region OldStuff
            //GameObject posXposZCornerObject = new GameObject();
            //posXposZCornerObject.transform.parent = selectedObject.transform;
            //GameObject posXnegZCornerObject = new GameObject();
            //posXnegZCornerObject.transform.parent = selectedObject.transform;
            //GameObject negXnegZCornerObject = new GameObject();
            //negXnegZCornerObject.transform.parent = selectedObject.transform;
            //GameObject negXposZCornerObject = new GameObject();
            //negXposZCornerObject.transform.parent = selectedObject.transform;
            //posXposZCornerObject.transform.position = posXposZCorner;
            //posXnegZCornerObject.transform.position = posXnegZCorner;
            //negXnegZCornerObject.transform.position = negXnegZCorner;
            //negXposZCornerObject.transform.position = negXposZCorner;

            

            //Quaternion q = Quaternion.FromToRotation(selectedObject.transform.up, hit.normal * -1);
            //selectedObject.transform.rotation = q * selectedObject.transform.rotation;
            //selectedObject.transform.Rotate(selectedObject.transform.up, Random.Range(0, 45), Space.Self);

            //GameObject sphere5 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //GameObject sphere6 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //GameObject sphere7 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //GameObject sphere8 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere5.transform.position = posXposZCornerObject.transform.position;
            //sphere6.transform.position = posXnegZCornerObject.transform.position;
            //sphere7.transform.position = negXnegZCornerObject.transform.position;
            //sphere8.transform.position = negXposZCornerObject.transform.position;
            //sphere5.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            //sphere6.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            //sphere7.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            //sphere8.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            //sphere5.name = "Start";
            //sphere6.name = "Start";
            //sphere7.name = "Start";
            //sphere8.name = "Start";

            ////Check corners aren't clipping into the ceiling			
            //int iterationLimit = 0; //Counts how many times the raycast check and rotation is performed. To prevent infinite while loops.				
            ////posXposZCorner = selectedObject.transform.TransformPoint(posXposZCorner);			
            ////On the bolt's Positive X and Positive Z corner
            //while (!Physics.Raycast(posXposZCornerObject.transform.position, posXposZCornerObject.transform.up, 0.5f))
            //{				
                
            //    selectedObject.transform.Rotate(new Vector3(-0.01f, 0, -0.01f));
                        
                
            //    iterationLimit++;
            //    if(iterationLimit > 500)
            //    {
            //        Debug.LogError("Corner fix failed! " + selectedObject.name);
            //        break;
            //    }
            //}
            
            //if (iterationLimit > 0)
            //{
            //    Debug.Log("Rotated PosX PosZ" + (float)iterationLimit*0.01f);
            //    iterationLimit = 0;
            //}
            ////On the bolt's Positive X and Negative Z corner		
            //while (!Physics.Raycast(posXnegZCornerObject.transform.position, posXnegZCornerObject.transform.up, 0.5f))
            //{
                
            //    selectedObject.transform.Rotate(new Vector3(-0.01f, 0, 0.01f));
                
            //    iterationLimit++;				
            //    if (iterationLimit > 500)
            //    {
            //        Debug.LogError("Corner fix failed! " + selectedObject.name);
            //        break;
            //    }
            //}
            
            //if (iterationLimit > 0)
            //{
            //    Debug.Log("Rotated PosX NegZ" + (float)iterationLimit * 0.01f);
            //    iterationLimit = 0;
            //}
            ////On the bolt's Negative X and Negative Z corner
            
            ////negXnegZCorner = selectedObject.transform.TransformPoint(negXnegZCorner);
            //while (!Physics.Raycast(negXnegZCornerObject.transform.position, negXnegZCornerObject.transform.up, 0.5f))
            //{
                
            //    selectedObject.transform.Rotate(new Vector3(0.01f, 0, 0.01f));
                
            //    iterationLimit++;
                
            //    //negXnegZCorner = selectedObject.transform.position + new Vector3(-0.0763568f, -0.0066f, -0.0763568f);

            //    if (iterationLimit > 500)
            //    {
            //        Debug.LogError("Corner fix failed! " + selectedObject.name);
            //        break;
            //    }
            //}
            
            //if (iterationLimit > 0)
            //{
            //    Debug.Log("Rotated NegX NegZ" + (float)iterationLimit * 0.01f);
            //    iterationLimit = 0;
            //}
            ////On the bolt's Negative X and Positive Z corner
            //while (!Physics.Raycast(negXposZCornerObject.transform.position, negXposZCornerObject.transform.up, 0.5f))
            //{
                
            //    selectedObject.transform.Rotate(new Vector3(0.01f, 0, -0.01f));
            //    iterationLimit++;

            //    if (iterationLimit > 500)
            //    {
            //        Debug.LogError("Corner fix failed! " + selectedObject.name);
            //        break;
            //    }
            //}
            
            //if (iterationLimit > 0)
            //{
            //    Debug.Log("Rotated NegX PosZ" + (float)iterationLimit * 0.01f);
            //    iterationLimit = 0;
            //}

            //GameObject sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //GameObject sphere3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //GameObject sphere4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //sphere1.transform.position = posXposZCornerObject.transform.position;
            //sphere2.transform.position = posXnegZCornerObject.transform.position;
            //sphere3.transform.position = negXnegZCornerObject.transform.position;
            //sphere4.transform.position = negXposZCornerObject.transform.position;
            //sphere1.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            //sphere2.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            //sphere3.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            //sphere4.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            #endregion

            
        }


    }

    [MenuItem("Create Mine/Delete All Roof Bolts", priority = 100)]
    public static void DeleteRoofBolts()
    {
        Object[] obj = Object.FindObjectsOfType(typeof(GameObject));
        foreach(Object o in obj)
        {
            GameObject check = o as GameObject;
            if(check.GetComponent<RoofBolt>() != null)
            {
                DestroyImmediate(check);
            }
        }
    }

    [MenuItem("Create Mine/Mine Segment and Link Test", priority = 100)]
    public static void MineSegAndLinkTest()
    {
        Object[] obj = Object.FindObjectsOfType(typeof(GameObject));

        GameObject bolt = Resources.Load("StationaryEquipment/Bolt_Single") as GameObject;
        int boltCount = 0;		

        GameObject demo = new GameObject();
        demo.name = "Roofbolt parent";
        demo.transform.position = new Vector3(0, 0, 0);

        foreach(Object o in obj)
        {
            GameObject seg = o as GameObject;
            //MineSegment mineSegmentRef = seg.GetComponent<MineSegment>();
            MineSegmentLink mineSegmentLink = seg.GetComponent<MineSegmentLink>();
            if(seg.activeSelf && mineSegmentLink != null)
            {
                //Debug.Log(mineSegmentLink.Segment1.name + " and " + mineSegmentLink.Segment2.name + " Distance: " + Vector3.Distance(mineSegmentLink.Segment1.transform.position, mineSegmentLink.Segment2.transform.position));
                Vector3 lineVector = mineSegmentLink.Segment2.transform.position - mineSegmentLink.Segment1.transform.position;
                Vector3 directionVector = Vector3.Normalize(lineVector);
                Vector3 startPoint = mineSegmentLink.Segment1.transform.position;
                Vector3 endPoint = mineSegmentLink.Segment2.transform.position;
                Vector3 perpendicularDirectionVector = Vector3.Normalize(Vector3.Cross(Vector3.up, lineVector));

                float interval = lineVector.magnitude / Mathf.Round(lineVector.magnitude / 1.2192f); //We want to ensure that there are an even number of roof bolts placed down the length of a segment. We also want them to be close to 4 feet.
                
                //Debug.Log(mineSegmentLink.Segment1.name +"(" + mineSegmentLink.Segment1.transform.position + ") " + " and " + mineSegmentLink.Segment2.name + "(" + mineSegmentLink.Segment2.transform.position + ") " + " Cross Vector: " + Vector3.Cross(Vector3.up, lineVector));
                //Start 2 feet away from the start of Segment 1 ( to properly do 4 foot spacing)
                for (float i = interval / 2; i < lineVector.magnitude; i = i + interval)
                {
                    Vector3 midPoint = startPoint + i * directionVector;
                    midPoint.y = 0.5f;
                    //do four raycasts
                    /*
                    Collider[] posOne = Physics.OverlapSphere(midPoint + (0.6096f * perpendicularDirectionVector), 0.5f);
                    Collider[] posTwo = Physics.OverlapSphere(midPoint + (1.8288f * perpendicularDirectionVector), 0.5f);
                    Collider[] negOne = Physics.OverlapSphere(midPoint + (-0.6096f * perpendicularDirectionVector), 0.5f);
                    Collider[] negTwo = Physics.OverlapSphere(midPoint + (-1.8288f * perpendicularDirectionVector), 0.5f);
                    */
                    Vector3 posOne = midPoint + (0.6096f * perpendicularDirectionVector);
                    Vector3 posTwo = midPoint + (1.8288f * perpendicularDirectionVector);
                    Vector3 negOne = midPoint + (-0.6096f * perpendicularDirectionVector);
                    Vector3 negTwo = midPoint + (-1.8288f * perpendicularDirectionVector);
                    bool posOneHasBolt = false;
                    bool posTwoHasBolt = false;
                    bool negOneHasBolt = false;
                    bool negTwoHasBolt = false;

                    foreach (Transform child in demo.transform)
                    {
                        if(Vector3.Distance(child.position, posOne) < 0.6096f)
                        {
                            posOneHasBolt = true;
                            continue;
                        }

                        if(Vector3.Distance(child.position, posTwo) < 0.6096f)
                        {
                            posTwoHasBolt = true;
                            continue;
                        }

                        if(Vector3.Distance(child.position, negOne) < 0.6096f)
                        {
                            negOneHasBolt = true;
                            continue;
                        }

                        if(Vector3.Distance(child.position, negTwo) < 0.6096f)
                        {
                            negTwoHasBolt = true;
                            continue;
                        }
                    }

                    if (!posOneHasBolt)
                    {
                        GameObject boltInstance = Instantiate(bolt);
                        boltInstance.transform.position = midPoint + (0.6096f * perpendicularDirectionVector);
                        boltInstance.name = "Bolt_" + boltCount;
                        boltInstance.transform.parent = demo.transform;						
                        boltCount++;
                    }

                    if (!posTwoHasBolt)
                    {
                        GameObject boltInstance = Instantiate(bolt);
                        boltInstance.transform.position = midPoint + (1.8288f * perpendicularDirectionVector);
                        boltInstance.name = "Bolt_" + boltCount;
                        boltInstance.transform.parent = demo.transform;						
                        boltCount++;
                    }

                    if (!negOneHasBolt)
                    {
                        GameObject boltInstance = Instantiate(bolt);
                        boltInstance.transform.position = midPoint + (-0.6096f * perpendicularDirectionVector);
                        boltInstance.name = "Bolt_" + boltCount;
                        boltInstance.transform.parent = demo.transform;						
                        boltCount++;
                    }

                    if (!negTwoHasBolt)
                    {
                        GameObject boltInstance = Instantiate(bolt);
                        boltInstance.transform.position = midPoint + (-1.8288f * perpendicularDirectionVector);
                        boltInstance.name = "Bolt_" + boltCount;
                        boltInstance.transform.parent = demo.transform;						
                        boltCount++;
                    }					
                }
            }

            if (seg.name.Contains("1-1Way"))
            {
                for(float i = 0.6f; i<6; i = i + 1.2f)
                {
                    Vector3 midPoint = seg.transform.position + (-i * seg.transform.forward);
                    midPoint.y += 0.5f;

                    Vector3 posOne = midPoint + (0.6096f * seg.transform.right);
                    Vector3 posTwo = midPoint + (1.8288f * seg.transform.right);
                    Vector3 negOne = midPoint + (-0.6096f * seg.transform.right);
                    Vector3 negTwo = midPoint + (-1.8288f * seg.transform.right);
                    bool posOneHasBolt = false;
                    bool posTwoHasBolt = false;
                    bool negOneHasBolt = false;
                    bool negTwoHasBolt = false;

                    foreach (Transform child in demo.transform)
                    {
                        if (Vector3.Distance(child.position, posOne) < 0.6096f)
                        {
                            posOneHasBolt = true;
                            continue;
                        }

                        if (Vector3.Distance(child.position, posTwo) < 0.6096f)
                        {
                            posTwoHasBolt = true;
                            continue;
                        }

                        if (Vector3.Distance(child.position, negOne) < 0.6096f)
                        {
                            negOneHasBolt = true;
                            continue;
                        }

                        if (Vector3.Distance(child.position, negTwo) < 0.6096f)
                        {
                            negTwoHasBolt = true;
                            continue;
                        }
                    }

                    if (!posOneHasBolt)
                    {
                        GameObject boltInstance = Instantiate(bolt);
                        boltInstance.transform.position = midPoint + (0.6096f * seg.transform.right);
                        boltInstance.name = "Bolt_" + boltCount;
                        boltInstance.transform.parent = demo.transform;
                        boltCount++;
                    }

                    if (!posTwoHasBolt)
                    {
                        GameObject boltInstance = Instantiate(bolt);
                        boltInstance.transform.position = midPoint + (1.8288f * seg.transform.right);
                        boltInstance.name = "Bolt_" + boltCount;
                        boltInstance.transform.parent = demo.transform;
                        boltCount++;
                    }

                    if (!negOneHasBolt)
                    {
                        GameObject boltInstance = Instantiate(bolt);
                        boltInstance.transform.position = midPoint + (-0.6096f * seg.transform.right);
                        boltInstance.name = "Bolt_" + boltCount;
                        boltInstance.transform.parent = demo.transform;
                        boltCount++;
                    }

                    if (!negTwoHasBolt)
                    {
                        GameObject boltInstance = Instantiate(bolt);
                        boltInstance.transform.position = midPoint + (-1.8288f * seg.transform.right);
                        boltInstance.name = "Bolt_" + boltCount;
                        boltInstance.transform.parent = demo.transform;
                        boltCount++;
                    }
                }
            }
        }
    }
}