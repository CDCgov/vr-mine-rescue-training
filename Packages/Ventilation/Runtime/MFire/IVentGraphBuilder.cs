using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using MFireProtocol;
using System.Runtime.Serialization;

public interface IVentGraphBuilder
{
    bool BuildVentGraph(VentGraph ventGraph);
    bool UpdateVentGraph(VentGraph ventGraph);

}
