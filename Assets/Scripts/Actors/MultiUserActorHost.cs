using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using Google.Protobuf;

public class MultiUserActorHost : ActorHost
{
    public float AimSpeed = 150.0f;
    private Transform[] Caps;
    private Transform[] Hands;
    public GameObject CapLampPrefab;
    

    private Transform _camParent;
    private Transform _steeringCap;

    private float _lastCamY;
    //private ViconHost _vicHost;

    private bool _showBody = false;


    protected override void Start()
    {
        Debug.Assert(HeadTransform != null);
        _rigidbody = GetComponent<Rigidbody>();

        MasterControl.SceneControl.AddActorHost(this);

        _animator = GetComponentInChildren<Animator>();
        _netObj = GetComponent<NetworkedObject>();
        //_vicHost = GameObject.FindObjectOfType<ViconHost>();
        //if(_vicHost == null)
        //{
        //    Debug.LogError("No vicon host in scene");
        //}
        //InputLookEuler = transform.rotation.eulerAngles;

        //if (_vicHost != null)
        //{
        //    /*
        //    int numCaps = _vicHost.HeadPositions.Length;
        //    Caps = new Transform[numCaps];
        //    for (int i = 0; i < numCaps; i++)
        //    {
        //        //GameObject capLamp = new GameObject("Cap_" + i);
        //        GameObject capLamp = Instantiate(CapLampPrefab);
        //        capLamp.transform.parent = transform;
        //        capLamp.transform.localPosition = _vicHost.HeadPositions[i];
        //        capLamp.transform.rotation = _vicHost.HeadRotations[i];
        //        //Light spot = capLamp.AddComponent<Light>();
        //        //spot.type = LightType.Spot;
        //        //spot.range = 50;
        //        //spot.spotAngle = 30;
        //        capLamp.AddComponent<LookGizmo>();
        //        Caps[i] = capLamp.transform;
        //        float ratio = 1f / (float)numCaps;
        //        Debug.Log(ratio);
        //        float hue = (i * ratio);
        //        Debug.Log(hue + " hue");
        //        if(hue > 1)
        //        {
        //            hue = 1;
        //        }
        //        capLamp.GetComponentInChildren<MeshRenderer>().material.color = Color.HSVToRGB(hue, 1, 1);
        //        TextMesh tm = capLamp.GetComponentInChildren<TextMesh>();
        //        if(tm != null)
        //        {
        //            int playerNum = i + 1;//Change from 0 base to 1 base number
        //            tm.color = Color.HSVToRGB(hue, 1, 1);
        //            tm.text = playerNum.ToString();
        //        }
        //        if (MasterControl.ActiveClientRole == ClientRole.MultiUser)
        //        {
        //            GameObject cone = capLamp.transform.Find("CapCone").gameObject;
        //            if(cone != null)
        //            {
        //                cone.SetActive(false);
        //            }
        //        }
        //    }
        //    */
        //}

        /*
        if(Caps.Length > 0)
        {
            _steeringCap = Caps[0];
        }*/
        //ShowBody(_showBody);
    }

    private void ReinitalizeCapsArray()
    {
        //if (_vicHost == null || _vicHost.HeadPositions == null ||
        //    _vicHost.HeadPositions.Length <= 0)
        //{
        //    Caps = null;
        //    return;
        //}

        //int numCaps = _vicHost.HeadPositions.Length;
        //Caps = new Transform[numCaps];
        //Hands = new Transform[numCaps];
        //for (int i = 0; i < numCaps; i++)
        //{
        //    Caps[i] = CreateCapLamp(i, numCaps).transform;
        //    Hands[i] = CreateHand(i, numCaps).transform;
        //}

        //_steeringCap = Caps[0];
    }

    private GameObject CreateHand(int handIndex, int numHands)
    {
        GameObject hand = Instantiate(HandSprite);
        hand.transform.parent = transform;
        hand.transform.position = Vector3.zero;
        hand.transform.rotation = Quaternion.identity;
        hand.SetActive(false);
        return hand;
    }

    private GameObject CreateCapLamp(int capIndex, int numCaps)
    {
        GameObject capLamp = Instantiate(CapLampPrefab);
        capLamp.transform.parent = transform;
        capLamp.transform.position = Vector3.zero;
        capLamp.transform.rotation = Quaternion.identity;

        //capLamp.transform.localPosition = _vicHost.HeadPositions[i];
        //capLamp.transform.rotation = _vicHost.HeadRotations[i];
        //Light spot = capLamp.AddComponent<Light>();
        //spot.type = LightType.Spot;
        //spot.range = 50;
        //spot.spotAngle = 30;
        //capLamp.AddComponent<LookGizmo>();
        //Caps[i] = capLamp.transform;
        float ratio = 1f / (float)numCaps;
        
        float hue = ((float)capIndex * ratio);
        
        if (hue > 1)
        {
            hue = 1;
        }
        capLamp.GetComponentInChildren<MeshRenderer>().material.color = Color.HSVToRGB(hue, 1, 1);
        TextMesh tm = capLamp.GetComponentInChildren<TextMesh>();
        if (tm != null)
        {
            int playerNum = capIndex + 1;//Change from 0 base to 1 base number
            tm.color = Color.HSVToRGB(hue, 1, 1);
            tm.text = playerNum.ToString();
        }
        if (MasterControl.ActiveClientRole == ClientRole.MultiUser)
        {
            GameObject cone = capLamp.transform.Find("CapCone").gameObject;
            if (cone != null)
            {
                cone.SetActive(false);
            }
        }

        return capLamp;
    }

    protected override void Update()
    {
        if (_netObj.HasAuthority)
        {
            InputMoveVector.y = 0;
            //_animWalkSpeed = InputMoveVector.magnitude;

            //Vector3 motion = InputMoveVector * Speed * Time.deltaTime;
            if (Input.GetButton("Fire1"))
            {
                Vector3 moveVector = _steeringCap.transform.forward;
                moveVector.y = 0;
                Vector3 motion = moveVector * Speed * Time.deltaTime;
                motion = transform.rotation * motion;
                transform.position += motion;
            }
            //if (_vicHost != null)
            //{
            //    _vicHost.UpdateVicon();

            //    if (Caps == null || Caps.Length != _vicHost.HeadPositions.Length)
            //    {
            //        ReinitalizeCapsArray();
            //    }

            //    for (int i = 0; i < Caps.Length; i++)
            //    {
            //        Vector3 lPos = _vicHost.HeadPositions[i];
            //        lPos.y = lPos.y - 1;
            //        Caps[i].transform.localPosition = lPos;
            //        Caps[i].transform.localRotation = _vicHost.HeadRotations[i];
            //        CapCast(Caps[i], i);
            //    }
            //}            
        }

        //transform.rotation = Quaternion.AngleAxis(InputLookEuler.y, Vector3.up);
        //HeadTransform.localRotation = Quaternion.AngleAxis(InputLookEuler.x, Vector3.right);

        if (_animator != null)
        {

            //_animator.SetFloat("Walking", _animWalkSpeed);
        }
    }

    public override void WriteObjState(CodedOutputStream writer)
    {
        //writer.Write(ActorName);

        //if (Caps == null)
        //{
        //	writer.Write((Int32)0);
        //}
        //else
        //{
        //	writer.Write(Caps.Length);
        //	//_vicHost.UpdateVicon();
        //	for (int i = 0; i < Caps.Length; i++)
        //	{


        //		/*Vector3 lPos = _vicHost.HeadPositions[i];
        //		lPos.y = lPos.y - 1;
        //		Caps[i].transform.localPosition = lPos;
        //		Caps[i].transform.rotation = _vicHost.HeadRotations[i];*/
        //		//writer.Write(_vicHost.HeadPositions[i]);
        //		//writer.Write(_vicHost.HeadRotations[i]);
        //		writer.Write(Caps[i].position);
        //		writer.Write(Caps[i].rotation);

        //		//writer.Write(Caps[i].position);
        //		//writer.Write(Caps[i].rotation);
        //	}

        //}
        //writer.Write(transform.position);
        ////writer.Write(_animWalkSpeed);
        //writer.Write(transform.rotation);
        ////writer.Write(HeadTransform.rotation);
        //writer.Write(InputLookEuler);
    }

    public override void SyncObjState(CodedInputStream reader)
    {
        InitializeActorHost();

        //ActorName = reader.ReadString();
        //int numCaps = reader.ReadInt32();
        
        //if (numCaps > 0 && (Caps == null || Caps.Length != numCaps))
        //{
        //	ReinitalizeCapsArray();
        //}

        //for (int i = 0; i < numCaps; i++)
        //{
        //	if (Caps != null)
        //	{
        //		//Vector3 lPos = reader.ReadVector3();
        //		//lPos.y = lPos.y - 1;
        //		if (i<Caps.Length && Caps[i] != null)
        //		{
        //			Vector3 read = reader.ReadVector3();                    
        //			Caps[i].position = read;
        //		}
        //		else
        //		{
        //			reader.ReadVector3();
        //		}
        //		if (i < Caps.Length && Caps[i] != null)
        //		{
        //			Caps[i].rotation = reader.ReadQuaternion();
        //		}
        //		else
        //		{
        //			reader.ReadQuaternion();
        //		}
        //	}
        //	else
        //	{
        //		reader.ReadVector3();
        //		reader.ReadQuaternion();
        //	}
        //}
        //Vector3 rootPos = reader.ReadVector3();
        ////_animWalkSpeed = reader.ReadSingle();

        //Quaternion rootRot = reader.ReadQuaternion();
        ////Quaternion headRot = reader.ReadQuaternion();
        //Vector3 lookEuler = reader.ReadVector3();
        //transform.position = rootPos;
        //InputLookEuler = lookEuler;
        //transform.rotation = rootRot;
        ////HeadTransform.rotation = headRot;

    }

    public override void EnableCamera(bool bEnable)
    {
        Debug.LogFormat("Enabling Camera on Actor: {0}", bEnable);
        if (bEnable)
        {

            if (_camera == null)
            {
                _camera = Resources.Load<GameObject>("MainCamera360");
                _camera = Instantiate<GameObject>(_camera);

                GameObject obj = new GameObject("CamParent");
                _camParent = obj.transform;
                if (HeadTransform != null && HeadTransform.parent != null)
                {
                    _camParent.SetParent(HeadTransform.parent, false);
                    _camParent.localPosition = HeadTransform.localPosition;
                }
                else
                    _camParent.SetParent(transform, false);
                
                _camera.transform.SetParent(_camParent, false);
                _camera.transform.localPosition = new Vector3(0, 0, 0);
                _lastCamY = _camera.transform.position.y;
            }
        }
        else
        {
            if (_camera != null)
                _camera.SetActive(false);
        }

        ShowBody(!bEnable); // hide body when in first-person camera mode
    }

    private void GamepadAim()
    {
        float h = Input.GetAxis("AimHorizontal");
        float v = Input.GetAxis("AimVertical") * 0.45f;

        //Debug.LogFormat("{0:F2}, {1:F2}", h, v);

        Vector3 currentRot = HeadTransform.rotation.eulerAngles;

        float oldpitch = currentRot.x;

        currentRot.x += v * AimSpeed * Time.deltaTime;
        if (currentRot.x > 25 && currentRot.x < 335)
            currentRot.x = oldpitch;
        currentRot.y += h * AimSpeed * Time.deltaTime;
        currentRot.z = 0;

        HeadTransform.rotation = Quaternion.Euler(currentRot);
    }

    protected override void ShowBody(bool bShow)
    {
        if (BodyModel != null)
        {
            BodyModel.SetActive(bShow);
            //if (Caps != null)
            //{
            //	foreach (Transform capLamp in Caps)
            //	{
            //		if (capLamp == null)
            //			continue;

            //		Transform cone = capLamp.Find("CapCone");
            //		//capLamp.GetComponentInChildren<MeshRenderer>().enabled = bShow;
            //		cone.gameObject.SetActive(bShow);
            //	}
            //}
        }

        _showBody = bShow;
    }

    //protected override void Update()
    //{
    //       if (Input.GetKey(KeyCode.F12))
    //       {
    //           RotateCaps();
    //       }

    //       GamepadAim();

    //	float h = Input.GetAxis("Horizontal");
    //	float v = Input.GetAxis("Vertical");

    //	//Vector3 motion = new Vector3(h, 0, v);
    //	Vector3 forward, right;
    //	Vector3 motion = Vector3.zero;

    //	forward = HeadTransform.forward;
    //	forward.y = 0;
    //	forward.Normalize();
    //	right = Quaternion.AngleAxis(90, Vector3.up) * forward;
    //	motion = forward * v + right * h;
    //	motion *= Speed * Time.deltaTime;

    //	Debug.DrawLine(HeadTransform.position, HeadTransform.position + HeadTransform.forward * 10);
    //	/*
    //	if (Turbo)
    //		motion *= 5 * Speed * Time.deltaTime;
    //	else
    //		motion *= Speed * Time.deltaTime; */		



    //	transform.position += motion;

    //	//hack to prevent camera vertical motion
    //	Vector3 camPos = _camera.transform.position;
    //	camPos.y = _lastCamY;
    //	_camera.transform.position = camPos;

    //	/*
    //	if (Input.GetButtonDown("L3"))
    //	{
    //		Turbo = !Turbo;
    //	} */


    //	//	if (Input.GetButtonDown("Fire1"))
    //	//	{
    //	//		Debug.Log("Fire1");

    //	//		RaycastHit hit;
    //	//		if (Physics.Raycast(CapLamp.transform.position, CapLamp.transform.forward, out hit))
    //	//		{
    //	//			int expIndex = Random.Range(0, _explosions.Length - 1);

    //	//			GameObject explosion = GameObject.Instantiate<GameObject>(_explosions[expIndex]);
    //	//			explosion.transform.position = hit.point;

    //	//			GameObject.Destroy(explosion, 10.0f);

    //	//			/*GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //	//               go.transform.position = hit.point;
    //	//               Debug.Log(hit.collider.gameObject.name); */

    //	//		}

    //	//	}

    //	//	if (Input.GetButtonDown("Fire2"))
    //	//	{
    //	//		Debug.Log("Fire2");

    //	//		RaycastHit hit;
    //	//		if (Physics.Raycast(CapLamp.transform.position, CapLamp.transform.forward, out hit))
    //	//		{
    //	//			int expIndex = Random.Range(0, _explosions.Length - 1);

    //	//			GameObject flame = GameObject.Instantiate<GameObject>(_flamethrower);
    //	//			flame.transform.SetParent(_spawnParent.transform, true);
    //	//			flame.transform.position = hit.point;
    //	//			flame.transform.rotation = Quaternion.FromToRotation(new Vector3(-1, 0, 0), hit.normal);

    //	//			GameObject.Destroy(flame, 7.0f);

    //	//			//GameObject.Destroy(explosion, 10.0f);

    //	//			/*GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //	//               go.transform.position = hit.point;
    //	//               Debug.Log(hit.collider.gameObject.name); */

    //	//		}
    //	//	}

    //	//	if (Input.GetButtonDown("Fire3"))
    //	//	{
    //	//		Debug.Log("Fire3");
    //	//	}

    //	if (Input.GetButtonDown("Fire4"))
    //	{
    //		Debug.Log("Moving to next spawn point");
    //		Transform spawnPoint = MasterControl.GetSceneControl().GetPlayerSpawn(NetworkManager.Instance.GetClientID());

    //		transform.position = spawnPoint.position;
    //		transform.rotation = spawnPoint.rotation;			
    //	}


    //	//	if (Input.GetButtonDown("RShoulder"))
    //	//	{
    //	//		Debug.Log("RShoulder");
    //	//		RaycastHit hit;
    //	//		if (Physics.Raycast(CapLamp.transform.position, CapLamp.transform.forward, out hit))
    //	//		{
    //	//			GameObject obj = GameObject.Instantiate<GameObject>(_special);
    //	//			obj.transform.SetParent(_spawnParent.transform, true);
    //	//			obj.transform.position = hit.point;
    //	//			obj.transform.rotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), hit.normal);
    //	//			obj.transform.rotation *= Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), Vector3.up);
    //	//		}
    //	//	}

    //	//	if (Input.GetButtonDown("LShoulder"))
    //	//	{
    //	//		Debug.Log("LShoulder");
    //	//		SpawnSpaceship();
    //	//		_lshoulderDown = Time.time;
    //	//	}

    //	//	if (Input.GetButton("LShoulder"))
    //	//	{
    //	//		if (Time.time - _lshoulderDown > 2)
    //	//		{
    //	//			SpawnSpaceship();
    //	//		}
    //	//	}
    //	//	else
    //	//		_lshoulderDown = -1;


    //	//	if (Input.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Space))
    //	//	{
    //	//		foreach (Transform child in _spawnParent.transform)
    //	//		{
    //	//			Destroy(child.gameObject);
    //	//		}
    //	//	}
    //}

    private void RotateCaps()
    {
        foreach(Transform cap in Caps)
        {            
            cap.RotateAround(transform.position, transform.up, Time.deltaTime * 10);
        }
    }

    public void DisableCones()
    {
        foreach (Transform capLamp in Caps)
        {
            Transform cone = capLamp.Find("CapCone");
            Debug.Log("Found cap");
            //capLamp.GetComponentInChildren<MeshRenderer>().enabled = bShow;
            cone.gameObject.SetActive(false);
        }        
    }

    public void CapCast(Transform capLamp, int capIndex)
    {
        RaycastHit hit;
        if (Physics.Raycast(capLamp.position, capLamp.forward, out hit, 10f))
        {
            Interactable inter = hit.collider.GetComponent<Interactable>();
            if (inter != null)
            {
                if (Hands[capIndex] != null)
                {
                    Hands[capIndex].gameObject.SetActive(true);
                    Hands[capIndex].forward = hit.normal;
                    Hands[capIndex].position = hit.point + hit.normal.normalized * 0.01f;
                    //HandSprite.transform.Translate(new Vector3(0, 0, 0.001f), Space.Self);
                }
                if (Input.GetButtonUp("Fire2")) //UPDATE TO WHATEVER INTERACTION DEVICE WE CHOOSE?
                {
                    inter.Interact(this);
                }
            }
            else
            {
                if (Hands[capIndex] != null)
                {
                    Hands[capIndex].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (HandSprite != null)
            {
                Hands[capIndex].gameObject.SetActive(false);
            }
        }
    }
}