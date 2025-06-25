using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxMWC : MonoBehaviour
{
	/// <summary>
	/// The user classification of the person wearing this MWC
	/// </summary>
	public ProxMWCUserType UserType;

	/// <summary>
	/// The ID of this prox system, only MWCs with matching IDs will interact
	/// </summary>
	public int ProxSystemID = 0;
} 
