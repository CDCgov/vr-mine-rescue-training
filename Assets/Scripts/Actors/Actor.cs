using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ActorType
{
	Player,
	NPC
}

public enum Posture
{
	Standing,
	Crouching,
	Prone,
	Walk,
	Run,
	DuckWalk,
	Crawl
}

/// <summary>
/// Class that handles human avatars within the mine. Can either be player controlled or AI controlled. (NPC)
/// </summary>
public class Actor : MineElement {

	//Public fields
	public ActorType Type;
	public Dictionary<string, float> HealthIssues;
	public HashSet<string> Capabilities;
	public PersonCharacteristics PersonCharacteristics;
	public Posture CurrentPosture;
	public float Stamina;

	private GameObject _actorObj;	

	/// <summary>
	/// create the gameobject with associated ActorHost. Should chose which host prefab based on actor data
	/// </summary>	
	public override GameObject Spawn()
	{
		GameObject obj = Resources.Load<GameObject>("Actor");
		obj = GameObject.Instantiate<GameObject>(obj);

		ActorHost host = obj.GetComponent<ActorHost>();
		host.BindToActor(this);		

		_actorObj = obj;
		return obj;
	}

	public override void Despawn()
	{
		if (_actorObj != null)
		{
			GameObject.Destroy(_actorObj);
			_actorObj = null;
		}
	}
}
