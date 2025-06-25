using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISocketableObject
{
    public void OnSocketed(CustomXRSocket socket);
    public void OnRemovedFromSocket(CustomXRSocket socket);

}
