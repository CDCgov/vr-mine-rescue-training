using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using MFireProtocol;

public class SceneControl : MonoBehaviour
{

    public static SceneControl CurrentSceneControl = null;

    public static event UnityAction SceneLoaded;
    public static event UnityAction InitializeSegments;
    public static event UnityAction BeginSimulation;

    public ActorHost CurrentActorHost;

    public Transform ResearcherSpawnPosition;
    public Transform[] PlayerSpawnPositions;

    public List<ActorHost> ActiveActors;
    public event UnityAction ActiveActorsChanged;

    public NetworkedObject[] SceneNetworkedObjects;
    public float VentParticleSpeedMultiplier = 1.0f;

    //private Dictionary<int, NetworkedObject> _sceneNetObjectCache;

    private GameObject _mfireVisParent;
    private bool _updateMFireVis = false;

    private MineNetwork _network;
    private Mesh _sphereMesh;
    private Material _sphereMat;
    private MaterialPropertyBlock _sphereMatProps;

    public HotkeyToggleGroup[] HotkeyGroups;

    public Gradient VentContaminantGradient;
    public Gradient VentMethaneGradient;

    private MFireServerControl _serverControl;

    void Awake()
    {
        ActiveActors = new List<ActorHost>();
        //_sceneNetObjectCache = new Dictionary<int, NetworkedObject>();

        _sphereMesh = ProcSphere.GenSphere();
        _sphereMat = Resources.Load<Material>("UnlitColoredNotInstanced");
        _sphereMatProps = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        CurrentSceneControl = this;
    }

    private void OnDisable()
    {
        if (CurrentSceneControl == this)
            CurrentSceneControl = null;
    }

    public Transform GetResearcherSpawn()
    {
        return ResearcherSpawnPosition;
    }

    public Transform GetPlayerSpawn(int playerIndex)
    {
        playerIndex = playerIndex % PlayerSpawnPositions.Length;
        return PlayerSpawnPositions[playerIndex];
    }

    public void AddActorHost(ActorHost actor)
    {
        ActiveActors.Add(actor);
        RaiseActiveActorsChanged();
    }

    public void RemoveActorHost(ActorHost actor)
    {
        ActiveActors.Remove(actor);
        RaiseActiveActorsChanged();
    }

    public NetworkedObject GetObjBySceneID(int sceneID)
    {

        if (sceneID >= 0 && sceneID < SceneNetworkedObjects.Length)
            return SceneNetworkedObjects[sceneID];
        else
            return null;
    }

    void Start()
    {
        _serverControl = FindObjectOfType<MFireServerControl>();
        if (_serverControl != null)
            _serverControl.MFireSimulationUpdated += OnMFireSimUpdated;

        RaiseSceneLoaded();
        RaiseInitializeSegments();
        RaiseBeginSimulation();
    }

    private void OnMFireSimUpdated()
    {
        _updateMFireVis = true;
    }

    private Dictionary<int, ParticleSystem> _mfireVisMap;

   // void UpdateMFireVis3()
   // {
   //     MineNetwork network = MineNetwork.FindSceneMineNetwork();
   //     if (network == null)
   //         return;
   //     if (_serverControl == null)
   //         return;

   //     if (_mfireVisParent == null)
   //     {
   //         _mfireVisParent = new GameObject("MFireVisParent");
   //         _mfireVisMap = new Dictionary<int, ParticleSystem>();
   //     }

   //     GameObject visPrefab = Resources.Load<GameObject>("MFireParticleVis2");

   //     foreach (KeyValuePair<int, MineSegmentLink> kvp in network._airwayMap)
   //     {
   //         MineSegmentLink link = kvp.Value;
   //         int airwayNo = kvp.Key;

   //         MFAirway airway = _serverControl.GetAirway(airwayNo);

   //         if (link == null || airway == null)
   //         {
   //             Debug.Log("MFire Airway map has bad entry!");
   //             continue;
   //         }

   //         MineSegment fromSeg;
   //         MineSegment toSeg;

   //         //float speed = (float)(airway.FlowRate / 10000.0);

   //         //compute speed in feet per second
   //         float actualSpeed = (float)(airway.FlowRate / airway.CrossSectionalArea);
   //         //convert to meters per second
   //         actualSpeed *= Constants.FeetToMeters;

   //         float speed = actualSpeed * VentParticleSpeedMultiplier;

   //         //Debug.LogFormat("MFire Airway Speed: {0:F2}", speed);
   //         /*
			//if (speed < 0.5f)
			//	continue; */

   //         //MFire adds a short "work airway" at the fire for some reason, need to bypass this for now since the junction has no association
   //         //the new airway has a start junction that is invalid / not associated, and an end junction of where the original airway ended
   //         //the original airway's end junction is changed to the added "work junction"
   //         int endJunc = airway.EndJunction;
   //         if (network.FindMFireJunction(endJunc) == null)
   //         {
   //             MFAirway a = _serverControl.FindAirwayWithStartJunction(endJunc);
   //             if (a != null)
   //             {
   //                 endJunc = a.EndJunction;
   //             }
   //         }

   //         /*
			//if (airway.FlowDirection <= 0)
			//{
			//	fromSeg = network.FindMFireJunction(airway.StartJunction);
			//	toSeg = network.FindMFireJunction(endJunc);
			//}
			//else
			//{
			//	fromSeg = network.FindMFireJunction(endJunc);
			//	toSeg = network.FindMFireJunction(airway.StartJunction);
			//}*/

   //         fromSeg = network.FindMFireJunction(airway.StartJunction);
   //         toSeg = network.FindMFireJunction(endJunc);

   //         if (fromSeg == null || toSeg == null)
   //         {
   //             Debug.LogFormat("MFire airway has invalid start/end junction! Airway {0} Start {1} End {2}", airway.Number, airway.StartJunction, airway.EndJunction);
   //             continue;
   //         }

   //         Vector3 dir = toSeg.transform.position - fromSeg.transform.position;
   //         float dist = dir.magnitude;

   //         ParticleSystem ps = null;
   //         bool bResimulate = false;
   //         if (!_mfireVisMap.TryGetValue(airway.Number, out ps))
   //         {
   //             var visObj = Instantiate<GameObject>(visPrefab, _mfireVisParent.transform);
   //             visObj.transform.position = fromSeg.transform.position + (Vector3.up * 0.75f);
   //             visObj.transform.rotation = Quaternion.FromToRotation(Vector3.forward, dir);

   //             ps = visObj.GetComponent<ParticleSystem>();
   //             _mfireVisMap[airway.Number] = ps;
   //             bResimulate = true;
   //         }
   //         else
   //         {
   //             Transform xform = ps.gameObject.transform;

   //             Vector3 targetPos = fromSeg.transform.position + (Vector3.up * 0.75f);
   //             Vector3 delta = xform.position - targetPos;
   //             if (delta.magnitude > 0.1f)
   //             {
   //                 xform.position = targetPos;
   //                 xform.rotation = Quaternion.FromToRotation(Vector3.forward, dir);
   //                 bResimulate = true;
   //             }
   //         }

   //         /*
			//Color color = Color.yellow;

			//float forwardComp = Vector3.Dot(dir, Vector3.forward);
			//if (forwardComp > 0.25f)
			//	color = Color.green;
			//else if (forwardComp < -0.25f)
			//	color = Color.red;			
			//	*/

   //         Color color = Color.white;
   //         MFJunction fromJunc = _serverControl.GetJunction(fromSeg.MFireJunction);
   //         MFJunction toJunc = _serverControl.GetJunction(toSeg.MFireJunction);

   //         color = VentMethaneGradient.Evaluate((float)fromJunc.CH4Concentration);
   //         Color endColor = VentMethaneGradient.Evaluate((float)toJunc.CH4Concentration);

   //         //spawnrate = (count / length) * speed
   //         float spawnRate = (0.5f / 0.25f) * speed;

   //         var psMain = ps.main;

   //         float speedChange = Mathf.Abs(psMain.startSpeed.constant - speed);
   //         if (speedChange > 0.1f)
   //             bResimulate = true;

   //         psMain.startSpeed = speed;
   //         psMain.startLifetime = dist / speed;
   //         psMain.startColor = Color.white;

   //         var lifeColor = ps.colorOverLifetime;
   //         Gradient lifeGrad = lifeColor.color.gradient;
   //         /*
			//lifeGrad.colorKeys = new GradientColorKey[2];
			//lifeGrad.colorKeys[0] = new GradientColorKey(Color.red, 0);
			//lifeGrad.colorKeys[1] = new GradientColorKey(Color.blue, 1); */
   //         lifeGrad.colorKeys = new GradientColorKey[] { new GradientColorKey(color, 0), new GradientColorKey(endColor, 1) };

   //         lifeColor.color = lifeGrad;

   //         var psEmit = ps.emission;
   //         psEmit.rateOverTime = spawnRate;

   //         var psShape = ps.shape;

   //         Vector3 scale = psShape.scale;
   //         Vector3 pos = psShape.position;

   //         if (speed < 0.5f)
   //         {
   //             scale.z = dist;
   //             pos.z = dist / 2;
   //             psMain.startLifetime = 5;
   //             psEmit.rateOverTime = 10;
   //         }
   //         else
   //         {
   //             scale.z = 0.2f;
   //             pos.z = 0.1f;
   //         }

   //         psShape.scale = scale;
   //         psShape.position = pos;


   //         if (bResimulate)
   //         {
   //             ps.Simulate(20, false, true);
   //             ps.Play();
   //         }
   //     }

   // }

    /*
	void UpdateMFireVis2()
	{
		MineNetwork network = MineNetwork.FindSceneMineNetwork();
		if (network == null)
			return;

		if (_mfireVisParent == null)
		{
			_mfireVisParent = new GameObject("MFireVisParent");
			_mfireVisMap = new Dictionary<int, ParticleSystem>();
		}

		GameObject visPrefab = Resources.Load<GameObject>("MFireParticleVis1");

		foreach (KeyValuePair<int, MineSegmentLink> kvp in network._airwayMap)
		{
			MineSegmentLink link = kvp.Value;
			int airwayNo = kvp.Key;

			MFAirway airway = MFireServerControl.GetAirway(airwayNo);

			if (link == null || airway == null)
			{
				Debug.Log("MFire Airway map has bad entry!");
				continue;
			}

			MineSegment fromSeg;
			MineSegment toSeg;

			float speed = (float)(airway.FlowRate / 10000.0);
		
			//MFire adds a short "work airway" at the fire for some reason, need to bypass this for now since the junction has no association
			//the new airway has a start junction that is invalid / not associated, and an end junction of where the original airway ended
			//the original airway's end junction is changed to the added "work junction"
			int endJunc = airway.EndJunction;
			if (network.FindMFireJunction(endJunc) == null)
			{
				MFAirway a = MFireServerControl.FindAirwayWithStartJunction(endJunc);
				if (a != null)
				{
					endJunc = a.EndJunction;
				}
			}

			if (airway.FlowDirection <= 0)
			{
				fromSeg = network.FindMFireJunction(airway.StartJunction);
				toSeg = network.FindMFireJunction(endJunc);
			}
			else
			{
				fromSeg = network.FindMFireJunction(endJunc);
				toSeg = network.FindMFireJunction(airway.StartJunction);
			}

			if (fromSeg == null || toSeg == null)
			{
				Debug.LogFormat("MFire airway has invalid start/end junction! Airway {0} Start {1} End {2}", airway.Number, airway.StartJunction, airway.EndJunction);
				continue;
			}

			Vector3 dir = toSeg.transform.position - fromSeg.transform.position;
			float dist = dir.magnitude;

			ParticleSystem ps = null;
			bool bNewPS = false;
			if (!_mfireVisMap.TryGetValue(airway.Number, out ps))
			{
				var visObj = Instantiate<GameObject>(visPrefab, _mfireVisParent.transform);
				visObj.transform.position = fromSeg.transform.position + (Vector3.up * 0.75f);
				visObj.transform.rotation = Quaternion.FromToRotation(Vector3.forward, dir);

				ps = visObj.GetComponent<ParticleSystem>();
				_mfireVisMap[airway.Number] = ps;
				bNewPS = true;
			}			

			//spawnrate = (count / length) * speed
			float spawnRate = (1.0f / 0.25f) * speed;

			var psMain = ps.main;
			psMain.startSpeed = speed;
			psMain.startLifetime = dist / speed;

			var psEmit = ps.emission;
			psEmit.rateOverTime = spawnRate;

			if (bNewPS)
			{
				ps.Simulate(20, false, true);
				ps.Play();
			}			
		}

	}

	void UpdateMFireVis()
	{
		MineNetwork network = MineNetwork.FindSceneMineNetwork();
		if (network == null)
			return;

		if (_mfireVisParent == null)
			_mfireVisParent = GameObject.Find("MFireVisParent");

		if (_mfireVisParent != null)
		{
			GameObject.Destroy(_mfireVisParent);			
		}

		_mfireVisParent = new GameObject("MFireVisParent");

		GameObject visPrefab = Resources.Load<GameObject>("MFireParticleVis1");

		foreach (KeyValuePair<int, MineSegmentLink> kvp in network._airwayMap)
		{
			MineSegmentLink link = kvp.Value;
			int airwayNo = kvp.Key;

			MFAirway airway = MFireServerControl.GetAirway(airwayNo);

			if (link == null || airway == null)
			{
				Debug.Log("MFire Airway map has bad entry!");
				continue;
			}

			MineSegment fromSeg;
			MineSegment toSeg;

			float speed = (float)(airway.FlowRate / 10000.0);
			//Debug.LogFormat("MFire Airway Speed: {0:F2}", speed);
			if (speed < 0.5f)
				continue;

			//MFire adds a short "work airway" at the fire for some reason, need to bypass this for now since the junction has no association
			//the new airway has a start junction that is invalid / not associated, and an end junction of where the original airway ended
			//the original airway's end junction is changed to the added "work junction"
			int endJunc = airway.EndJunction;
			if (network.FindMFireJunction(endJunc) == null)
			{
				MFAirway a = MFireServerControl.FindAirwayWithStartJunction(endJunc);
				if (a != null)
				{
					endJunc = a.EndJunction;
				}
			}

			if (airway.FlowDirection <= 0)
			{
				fromSeg = network.FindMFireJunction(airway.StartJunction);
				toSeg = network.FindMFireJunction(endJunc);
			}
			else
			{
				fromSeg = network.FindMFireJunction(endJunc);
				toSeg = network.FindMFireJunction(airway.StartJunction);
			}

			if (fromSeg == null || toSeg == null)
			{
				Debug.LogFormat("MFire airway has invalid start/end junction! Airway {0} Start {1} End {2}", airway.Number, airway.StartJunction, airway.EndJunction);
				continue;
			}

			var visObj = Instantiate<GameObject>(visPrefab, _mfireVisParent.transform);
			ParticleSystem ps = visObj.GetComponent<ParticleSystem>();

			Vector3 dir = toSeg.transform.position - fromSeg.transform.position;
			float dist = dir.magnitude;

			//spawnrate = (count / length) * speed
			float spawnRate = (1.0f / 0.25f) * speed;

			var psMain = ps.main;
			psMain.startSpeed = speed;
			psMain.startLifetime = dist / speed;

			var psEmit = ps.emission;
			psEmit.rateOverTime = spawnRate;

			ps.Simulate(20, false, true);
			ps.Play();

			visObj.transform.position = fromSeg.transform.position + (Vector3.up * 0.75f);
			visObj.transform.rotation = Quaternion.FromToRotation(Vector3.forward, dir);
		}

		
	} */

    void ClearMFireVis()
    {
        if (_mfireVisParent != null)
        {
            Destroy(_mfireVisParent);
            _mfireVisParent = null;
        }
    }

    void Update()
    {
        if (_network == null)
        {
            _network = MineNetwork.FindSceneMineNetwork();
        }

        //if (_updateMFireVis)
        //{
        //    _updateMFireVis = false;
        //    if (_mfireVisParent != null)
        //        UpdateMFireVis3();
        //}

        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        //    {
        //        Debug.Log("Spawning MFire Vis");

        //        if (_mfireVisParent == null)
        //            UpdateMFireVis3();
        //        else
        //            ClearMFireVis();
        //    }
        //}

        if (HotkeyGroups != null && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            foreach (HotkeyToggleGroup group in HotkeyGroups)
            {
                if (Input.GetKeyDown(group.KeyCode) && group.GameObjects != null)
                {
                    foreach (var obj in group.GameObjects)
                    {
                        obj.SetActive(!obj.activeSelf);
                    }
                }
            }
        }

     //   if (_network != null && _network._junctionMap != null && _serverControl != null)
     //   {
     //       foreach (KeyValuePair<int, MineSegment> kvp in _network._junctionMap)
     //       {
     //           MFJunction junc = _serverControl.GetJunction(kvp.Key);
     //           MineSegment seg = kvp.Value;

     //           Matrix4x4 mat;

     //           if (VentContaminantGradient == null)
     //           {
     //               VentContaminantGradient = new Gradient();
     //           }

     //           if (junc != null && seg != null)
     //           {
     //               /*if (junc.ContamConcentration > 0)
					//{
					//	Debug.LogFormat("Contam in {0} : {1:F6}", junc.Number, junc.ContamConcentration);
					//}*/
     //               float scale = (float)(junc.ContamConcentration * 15.0);
     //               Color color = Color.white;
     //               color = VentContaminantGradient.Evaluate((float)(junc.ContamConcentration));

     //               if (float.IsNaN(scale) || float.IsInfinity(scale))
     //                   scale = 0;
     //               scale = Mathf.Clamp(scale, 0, 2.5f);
     //               mat = Matrix4x4.TRS(seg.transform.position, Quaternion.identity, new Vector3(scale, scale, scale));
     //               _sphereMatProps.SetColor("_Color", color);
     //               Graphics.DrawMesh(_sphereMesh, mat, _sphereMat, 0, null, 0, _sphereMatProps);
     //           }
     //       }
     //   }
    }

    private void RaiseActiveActorsChanged()
    {
        UnityAction action = ActiveActorsChanged;
        if (action != null)
            action();
    }

    private void RaiseSceneLoaded()
    {
        var handler = SceneLoaded;
        if (handler != null)
            handler();
    }

    private void RaiseInitializeSegments()
    {
        var handler = InitializeSegments;
        if (handler != null)
            handler();
    }

    private void RaiseBeginSimulation()
    {
        var handler = BeginSimulation;
        if (handler != null)
            handler();
    }
}