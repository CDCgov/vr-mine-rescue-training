using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhoneCable))]
public class PhoneCableEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PhoneCable phoneCable = (PhoneCable)target;

        if(GUILayout.Button("Add Branch Cable"))
        {
            EditorUtility.SetDirty(phoneCable);
            Transform branches = phoneCable.transform.Find("Branches");
            if (branches == null)
            {
                GameObject branchesGO = new GameObject();
                branchesGO.name = "Branches";
                branchesGO.transform.parent = phoneCable.transform;
                branches = branchesGO.transform;
            }
            
            GameObject obj = new GameObject("HangingCable");
            obj.AddComponent<MeshRenderer>();
            obj.AddComponent<MeshFilter>();
            obj.transform.parent = branches;

            HangingCable cable = obj.AddComponent<HangingCable>();
            HangingCable sourceCable = phoneCable.GetComponent<HangingCable>();
            if (sourceCable != null) {
                cable.CableDiameter = sourceCable.CableDiameter;
                cable.CableHangerWidth = sourceCable.CableHangerWidth;
                //cable.CableMaterial = sourceCable.CableMaterial;
                cable.DefaultCableSlope = sourceCable.DefaultCableSlope;
            }

            var selection = new Object[1];
            selection[0] = cable.gameObject;
            Selection.objects = selection;
        }

        if(GUILayout.Button("Populate Phone Nodes"))
        {
            EditorUtility.SetDirty(phoneCable);

            Component[] existingNodes = phoneCable.gameObject.GetComponentsInChildren(typeof(PhoneNode));
            foreach(Component com in existingNodes)
            {
                DestroyImmediate(com.gameObject);
            }

            HangingCable cable = phoneCable.GetComponent<HangingCable>();
            
            List<HangingCable> branchPoints = new List<HangingCable>();
            Transform branches = phoneCable.transform.Find("Branches");
            Transform phoneNodeParent = phoneCable.transform.Find("PhoneNodes");
            HangingCable[] children = null;
            if (branches != null)
            {                
                children = phoneCable.GetComponentsInChildren<HangingCable>();
                foreach(HangingCable child in children)
                {
                    if (child != cable)
                    {
                        branchPoints.Add(child);
                    }
                    else
                    {
                        Debug.Log("lol");                        
                    }
                }
            }

            if(phoneNodeParent == null)
            {
                GameObject phoneParentGo = new GameObject();
                phoneParentGo.name = "PhoneNodes";
                phoneParentGo.transform.parent = phoneCable.transform;
                phoneNodeParent = phoneParentGo.transform;
            }

            if (cable != null)
            {
                PopulateNodes(cable, phoneNodeParent, phoneCable.JunctionBoxMat, branchPoints.ToArray());
            }
        }
    }

    PhoneNode PopulateNodes(HangingCable cable, Transform parent, Material junctionMat,HangingCable[] branchPoints = null)
    {
        PhoneNode firstNode = null;
        PhoneNode priorNode = null;
        List<PhoneNode> spawnedNodes = new List<PhoneNode>();
        
        for (int i = 0; i < cable.CableNodes.Count; i++)
        {
            
            GameObject spawnedNode = new GameObject();
            spawnedNode.transform.parent = parent;
            spawnedNode.transform.position = cable.CableNodes[i].Position;
            spawnedNode.name = "PhoneNode";
            PhoneNode phone = spawnedNode.AddComponent<PhoneNode>();
            phone.NextPhones = new List<PhoneNode>();
            phone.PriorPhones = new List<PhoneNode>();
            spawnedNodes.Add(phone);
            if (branchPoints != null || i > 0)
            {
                if (priorNode != null)
                {
                    phone.PriorPhones.Add(priorNode);
                    priorNode.NextPhones.Add(phone);
                }
                priorNode = phone;
            }
            //if (branchPoints == null && i == 0)
            //{
                
            //    firstNode = phone;
            //    GameObject junctionBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //    Vector3 pos = phone.transform.position;
            //    pos.y = pos.y - 0.0419f;
            //    junctionBox.transform.position = pos;
            //    junctionBox.transform.parent = parent;
            //    junctionBox.transform.localScale = new Vector3(0.05f, 0.03f, 0.05f);

            //    junctionBox.GetComponent<Renderer>().material = junctionMat;
            //    int count = 0;
            //    if (parent != null)
            //    {
            //        foreach (Transform child in parent)
            //        {
            //            if (child.name.Contains("Junction Box"))
            //            {
            //                count++;
            //            }
            //        }
            //    }
            //    junctionBox.name = "Junction Box " + count.ToString("00");
            //    //if (branchPoints == null)
            //    //{
            //    //    DestroyImmediate(phone.gameObject, false);
            //    //}
            //}
        }

        if (branchPoints != null)
        {
            foreach (HangingCable branch in branchPoints)
            {
                //Didn't do a straight compare, to account for floating point imprecisions that can happen
                for (int i = 0; i < spawnedNodes.Count; i++)
                {
                    if (Vector3.Distance(branch.CableNodes[0].Position, spawnedNodes[i].transform.position) < 0.01f)
                    {
                        //PhoneNode splitPoint = PopulateNodes(branch, parent, junctionMat);
                        PopulateBranch(spawnedNodes[i], branch, junctionMat);
                        //spawnedNodes[i].NextPhones.Add(splitPoint);
                        //if(splitPoint.PriorPhones == null)
                        //{
                        //    splitPoint.PriorPhones = new List<PhoneNode>();
                        //}
                        //splitPoint.PriorPhones.Add(spawnedNodes[i]);
                    }
                }
            }

        }
        return firstNode;
    }

    void PopulateBranch(PhoneNode source, HangingCable branch, Material junctionMaterial)
    {
        //PhoneNode first = null;

        GameObject junctionBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 pos = source.transform.position;
        pos.y = pos.y - 0.0419f;
        junctionBox.transform.position = pos;
        junctionBox.transform.parent = source.transform;
        junctionBox.transform.localScale = new Vector3(0.05f, 0.03f, 0.05f);

        junctionBox.GetComponent<Renderer>().material = junctionMaterial;

        List<PhoneNode> phones = new List<PhoneNode>();
        Transform findBranch = branch.transform.Find("BranchNodes");
        GameObject branchNodes;
        if (findBranch != null)
        {
            branchNodes = findBranch.gameObject;
        }
        else
        {
            branchNodes = new GameObject();
        }
        branchNodes.name = "BranchNodes";
        branchNodes.transform.parent = branch.transform;
        for (int i = 1; i < branch.CableNodes.Count; i++)
        {
            GameObject spawn = new GameObject();
            spawn.name = "Branch_Phone Node";
            spawn.transform.position = branch.CableNodes[i].Position;
            spawn.transform.parent = branchNodes.transform;
            PhoneNode phNode = spawn.AddComponent<PhoneNode>();
            phNode.PriorPhones = new List<PhoneNode>();
            phNode.NextPhones = new List<PhoneNode>();            

            if (i == 1)
            {
                source.NextPhones.Add(phNode);
                phNode.PriorPhones.Add(source);
                phones.Add(phNode);
            }
            else
            {
                phones[i - 2].NextPhones.Add(phNode);
                phNode.PriorPhones.Add(phones[i - 2]);
                phones.Add(phNode);
            }
        }
    }
}