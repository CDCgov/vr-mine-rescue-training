using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomXRSocket))]
public class PlayerEquipmentSocket : MonoBehaviour
{
    public VRNPlayerEquipmentType EquipmentType;
    public bool LeftHanded;
    public bool RightHanded;

    public Transform AssociatedVisual;
}
