using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NIOSH_MineCreation
{
    public class MineSegmentTileSettings : MonoBehaviour
    {
        public MineTile tileSettings;

        private Vector3 placedPosition;

        private bool initPlaced = false;

        public SegmentGeometry geometry;

        private bool isConnected = false;

        public MineSegmentTileSettings[] connections;

        public UnityAction tileMoved;

        public GameObject forwardsRotObj;
        public GameObject backwardsRotObj;

        private void Start()
        {
            GetComponent<PlacablePrefab>().OnPlaced += TilePlaced;
            connections = new MineSegmentTileSettings[geometry.SegmentConnections.Length];
        }

        public void SetMineTile(MineTile newSettings)
        {
            tileSettings = newSettings;
        }

        private void TilePlaced(bool isPlaced)
        {
            if (isPlaced)
            {
                CheckConnections();

                if (!CheckValidPlacement())
                {
                    if (initPlaced)
                    {
                        transform.position = placedPosition;
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    initPlaced = true;
                    placedPosition = transform.position;
                    tileMoved?.Invoke();
                }
            }
            else
            {
                for (int i = 0; i < connections.Length; i++)
                {
                    connections[i] = null;
                }

                tileMoved?.Invoke();
            }
        }

        public GameObject RotateTile(bool rotateForwards)
        {
            if (rotateForwards)
            {
                if (forwardsRotObj == null)
                    return gameObject;

                Destroy(gameObject);
                return Instantiate(forwardsRotObj, transform.position, transform.rotation);
            }
            else
            {
                if (backwardsRotObj == null)
                    return gameObject;

                Destroy(gameObject);
                return Instantiate(backwardsRotObj, transform.position, transform.rotation);
            }
        }

        public void HandlePlacedNoSnap()
        {
            if (initPlaced)
            {
                transform.position = placedPosition;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            tileMoved?.Invoke();
        }

        public void MakeConnection(MineSegmentTileSettings otherTile)
        {
            CheckConnections();
            otherTile.tileMoved += CheckConnections;
        }

        private void CheckConnections()
        {
            Ray ray;
            RaycastHit hit;

            for(int i = 0; i < geometry.SegmentConnections.Length; i++)
            {
                ray = new Ray(transform.position + geometry.SegmentConnections[i].Centroid - geometry.SegmentConnections[i].Normal, geometry.SegmentConnections[i].Normal);
                /*if(i == 0)
                    Debug.DrawRay(transform.position + geometry.SegmentConnections[i].Centroid - geometry.SegmentConnections[i].Normal, geometry.SegmentConnections[i].Normal, Color.green, 5.0f);
                else
                    Debug.DrawRay(transform.position + geometry.SegmentConnections[i].Centroid - geometry.SegmentConnections[i].Normal, geometry.SegmentConnections[i].Normal, Color.red, 5.0f);*/

                if (Physics.Raycast(ray, out hit, 1.1f, LayerMask.GetMask("MineSegments", "SelectedObject")))
                {
                    if(connections[i] == null)
                    {
                        connections[i] = hit.transform.GetComponent<MineSegmentTileSettings>();
                        connections[i].MakeConnection(this);
                        connections[i].tileMoved += CheckConnections;
                    }
                }
                else
                {
                    if (connections[i] != null)
                        connections[i].tileMoved -= CheckConnections;

                    connections[i] = null;
                }
            }
        }

        private bool CheckValidPlacement()
        {
            for(int i = 0; i < connections.Length; i++)
            {
                if (connections[i])
                    return true;
            }

            return false;
        }

        public int GetConnectionIndexFromPosition(Vector3 queryPosition)
        {
            //First we need to determine which of the connection points the query point is closest to
            float closestDistance = -1.0f;
            int closestConnectionIndex = -1;

            for(int i = 0; i < geometry.SegmentConnections.Length; i++)
            {
                Vector3 connectionPosition = transform.position + geometry.SegmentConnections[i].Centroid;
                float distanceToConnection = Vector3.Distance(connectionPosition, queryPosition);

                //Debug.Log("Checking segment " + i + " located at " + connectionPosition + " with query " + queryPosition);

                if (closestDistance < 0)
                {
                    closestDistance = distanceToConnection;
                    closestConnectionIndex = i;

                    //Debug.Log("Closest by default: " + distanceToConnection);
                }
                else if(distanceToConnection < closestDistance)
                {
                    closestDistance = distanceToConnection;
                    closestConnectionIndex = i;

                    //Debug.Log(queryPosition + " is closer to " + connectionPosition + ": " + distanceToConnection);
                }

                //Debug.Log("distance: " + distanceToConnection);
            }

            if (connections[closestConnectionIndex] != null)
            {
                //Debug.Log("Not returning connection because the closest one already has a connection established: index " + closestConnectionIndex);
                return -1;
            }
            else
            {
                //Debug.Log("closest index is " + closestConnectionIndex);
                return closestConnectionIndex;//transform.position + geometry.SegmentConnections[closestConnectionIndex].Centroid;
            }
        }

        public SegmentConnection GetConnection(int index)
        {
            return geometry.SegmentConnections[index];
        }

        public int GetConnectionIndexFromConnectionID(string id)
        {
            switch(id.Substring(id.Length - 1))
            {
                case "A":
                    id = id.Substring(0, id.Length - 1) + "B";
                    break;

                case "B":
                    id = id.Substring(0, id.Length - 1) + "A";
                    break;
            }

            for (int i = 0; i < geometry.SegmentConnections.Length; i++)
            {
                if (geometry.SegmentConnections[i].ConnectionID == id)
                {
                    return i;
                }
            }

            return -1;
        }

        public Vector3 GetCentroidWorldPosition(int centroidIndex)
        {
            return transform.position + geometry.SegmentConnections[centroidIndex].Centroid;
        }

        public Vector3 GetSnappingPositionForCentroid(int centroidIndex, Vector3 snapToCentroidPos)
        {
            Vector3 snapPoint = snapToCentroidPos;

            snapPoint -= geometry.SegmentConnections[centroidIndex].Centroid;

            return snapPoint;
        }
    }
}