using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NIOSH_EditorLayers;
using System;
using UnityEngine.Serialization;

namespace NIOSH_MineCreation
{
    public class MineLayerTile : LayerControlledClass, IScenarioEditorMouseDrag
    {


        public const float MineTileSnapDistance = 3.0f;
        //public const float MineTileSearchRadius = 20.0f;

        public Placer Placer;

        public GameObject ReturnTile;
        //public static MineSettings Settings;
        public MineLayerTile[] Connections;

        public MineLayerTileManager MineLayerTileManager;

        public SegmentGeometry SegmentGeometry
        {
            get
            {
                return _geometry;
            }
        }

        //public UnityAction TileMoved;
        //public bool InitPlaced = false;
        //public static Vector3 BuildScaler;

        private bool _inEditMode = false;
        //private PlacablePrefab _cachedPlacablePrefab;
        private SegmentGeometry _geometry;
        private Vector3 _placedPosition;
        private bool _placedPositionValid = false;
        private bool _isScaledToSettings = false;
        private Collider[] _mineTileResults;
        private int _numMineTileResults;
        private bool _mineTileSnapped;
        private PlacablePrefab _placeable;
        private bool _isOriginTile;
        private int _raycastMineTilesMask;

        private void Awake()
        {
            _raycastMineTilesMask = LayerMask.GetMask("MineSegments");

            _mineTileResults = new Collider[40];
            _numMineTileResults = 0;

            _geometry = GetComponent<MineSegment>().SegmentGeometry;
            Connections = new MineLayerTile[_geometry.SegmentConnections.Length];

            _placeable = GetComponent<PlacablePrefab>();
        }

        private new void Start()
        {
            base.Start();

            if (ScenarioSaveLoad.IsScenarioEditor)
            {
                if (MineLayerTileManager == null)
                    MineLayerTileManager = MineLayerTileManager.GetDefault();

                if (Placer == null)
                    Placer = Placer.GetDefault();
            }

            _isScaledToSettings = true;
            _placeable = GetComponent<PlacablePrefab>();

            if (_placeable != null && _placeable.IsPlaced)
                SetReturnPoint(transform.position);
            //if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "ScenarioTemplateMFIRE")
            //{
            //    ComponentInfo_MineSegment ms = GetComponentInChildren<ComponentInfo_MineSegment>();
            //    if (ms != null && Settings != null)
            //    {
            //        ms.BoltSpacing = Settings.BoltSpacing;
            //        StartCoroutine(DelayedBoltSpawn(ms));
            //        //ms.SpawnBolts();
            //    }
            //}
        }

        public void SpawnBolts()
        {
            ComponentInfo_MineSegment ms = GetComponentInChildren<ComponentInfo_MineSegment>();
            var settings = ScenarioSaveLoad.Settings.MineSettings;
            if (ms != null && settings != null)
            {
                ms.BoltSpacing = settings.BoltSpacing;                
                ms.SpawnBolts();
            }
        }

        IEnumerator DelayedBoltSpawn(ComponentInfo_MineSegment ms)
        {
            yield return 0;
            ms.SpawnBolts();
        }

        public void ScaleToSettings()
        {
            if (_isScaledToSettings)
                return;

            PillarTile pillarTile;
            var settings = ScenarioSaveLoad.Settings;

            if (TryGetComponent(out pillarTile))
            {
                pillarTile.ScalePillarTile(settings.MineSettings);
            }
            else
            {
                transform.localScale = Vector3.Scale(transform.localScale, settings.MineScale);
            }

            _isScaledToSettings = true;
        }

        public void ChangeModeToEdit(bool startPlaced)
        {
            if (_inEditMode)
                return;

            //gameObject.TryGetComponent(out _cachedPlacablePrefab);

            if(_placeable == null && !gameObject.TryGetComponent<PlacablePrefab>(out _placeable))
                _placeable = gameObject.AddComponent<PlacablePrefab>();

            _placeable.OnPlaced += TilePlaced;

            if (_placeable.IsPlaced || startPlaced)
                SetReturnPoint(transform.position);

            //DisconnectAllTiles();
            //TileMoved = null;

            if (startPlaced)
                _placeable.SetPlaced();
            else
                ClearConnections();

            gameObject.isStatic = false;

            _inEditMode = true;
        }

        public void ChangeModeToNonEdit()
        {
            if (!_inEditMode)
                return;

            if (_placeable != null)
                _placeable.OnPlaced -= TilePlaced;

            gameObject.isStatic = true;

            _inEditMode = false;

            //gameObject.layer = LayerMask.NameToLayer("MineSegments");
        }

        public void SetReturnPoint(Vector3 position)
        {
            //InitPlaced = true;
            _placedPosition = position;
            _placedPositionValid = true;

            //_cachedPlacablePrefab = newPlacable;
            //_cachedPlacablePrefab.OnPlaced += TilePlaced;
        }

        public void ReturnToLastValid()
        {
            transform.position = _placedPosition;
        }

        private void DestroyTile()
        {
            if (Placer != null)
            {
                if (Placer.SelectedObject == gameObject)
                    Placer.DeselectObject();
            }

            gameObject.SetActive(false);
            Destroy(gameObject);

            if (Placer != null)
            {
                Placer.RaiseSceneObjectListChanged();
            }
        }

        private void TilePlaced(bool isPlaced)
        {
            //Debug.Log($"Tile {gameObject.name} placed: {isPlaced}");

            if (MineLayerTileManager != null)
            {
                //MineLayerTileManager.RebuildTileConnections();
                //MineLayerTileManager.ClearTileConnections(this);
                //MineLayerTileManager.CheckTileConnections(this);
            }

            if (isPlaced)
            {
                //CheckConnections();

                //this is called on load, connection checks should be handled prior to this
                SetReturnPoint(transform.position);

                //if (!CheckValidPlacement())
                //{
                //    if (_placedPositionValid)
                //    {
                //        ReturnToLastValid();
                //    }
                //    else
                //    {
                //        //Debug.Log("DESTROY HERE");
                //        DestroyTile();
                //    }
                //}
                //else
                //{
                //    SetReturnPoint(transform.position);
                //    //TileMoved?.Invoke();
                //    //Destroy(ReturnTile);
                //    //ReturnTile = null;
                //}
            }
            else
            {
                if (MineLayerTileManager != null)
                {
                    MineLayerTileManager.ClearTileConnections(this);
                }
                else
                {
                    ClearConnections();
                }
                //DisconnectAllTiles();

                //TileMoved?.Invoke();
            }
        }

        //private void DisconnectAllTiles()
        //{
        //    if (Connections == null)
        //        return;

        //    for (int i = 0; i < Connections.Length; i++)
        //    {
        //        if (Connections[i] != null)
        //        {
        //            Connections[i].TileMoved -= CheckConnections;
        //        }

        //        Connections[i] = null;
        //    }
        //}

        public void HandlePlacedNoSnap()
        {
            if (_placedPositionValid)
            {
                //transform.position = _placedPosition;
                ReturnToLastValid();
                _placeable.SetPlaced();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private new void OnDestroy()
        {
            base.OnDestroy();

            //DisconnectAllTiles();
            //TileMoved?.Invoke();

            //if(ReturnTile != null)
            //{
            //    ReturnTile.SetActive(true);
            //}
        }

        //public void MakeConnection(MineLayerTile otherTile)
        //{
        //    CheckConnections();
        //    otherTile.TileMoved += CheckConnections;
        //}

        private bool CheckValidPlacement()
        {
            for (int i = 0; i < Connections.Length; i++)
            {
                if (Connections[i])
                    return true;
            }

            return false;
        }


        //private void CheckConnections()
        //{
        //    Ray ray;
        //    RaycastHit hit;

        //    for (int i = 0; i < _geometry.SegmentConnections.Length; i++)
        //    {
        //        try
        //        {
        //            ray = new Ray((transform.position - (_geometry.SegmentConnections[i].Normal / 100.0f)) + Vector3.Scale(_geometry.SegmentConnections[i].Centroid, transform.localScale), _geometry.SegmentConnections[i].Normal / 99.0f);
        //            /*if(_geometry.SegmentConnections.Length == 3)
        //            {
        //                if (i == 0)
        //                    Debug.DrawRay((transform.position - (_geometry.SegmentConnections[i].Normal / 100.0f)) + Vector3.Scale(_geometry.SegmentConnections[i].Centroid, transform.localScale), _geometry.SegmentConnections[i].Normal, Color.green, 50.0f);
        //                else
        //                    Debug.DrawRay((transform.position - (_geometry.SegmentConnections[i].Normal / 100.0f)) + Vector3.Scale(_geometry.SegmentConnections[i].Centroid, transform.localScale), _geometry.SegmentConnections[i].Normal, Color.red, 50.0f);
        //            }*/
        //            if (Physics.Raycast(ray, out hit, 1.1f, LayerMask.GetMask("MineSegments", "SelectedObject")))
        //            {
        //                if (Connections[i] == null)
        //                {
        //                    Connections[i] = hit.transform.GetComponent<MineLayerTile>();
        //                    Connections[i].MakeConnection(this);
        //                    Connections[i].TileMoved += CheckConnections;
        //                }
        //            }
        //            else
        //            {
        //                if (Connections[i] != null)
        //                    Connections[i].TileMoved -= CheckConnections;

        //                Connections[i] = null;
        //            }
        //        }
        //        catch(Exception e)
        //        {
        //            Debug.Log("Still have connections: " + Connections.Length + " : " + e);
        //        }
        //    }
        //}

        public void RemoveConnectionTo(MineLayerTile tile)
        {
            if (Connections == null)
                return;

            for (int i = 0; i < Connections.Length; i++)
            {
                if (Connections[i] == tile)
                {
                    Connections[i] = null;
                }
            }
        }

        public void ClearConnections()
        {
            if (Connections == null)
                return;

            for (int i = 0; i < Connections.Length; i++)
            {
                Connections[i] = null;
            }
        }

        public bool IsConnectionOccupied(int connIndex)
        {
            if (Connections[connIndex] == null)
                return false;
            else
                return true;
        }

        public int GetConnectionIndexFromPosition(Vector3 queryPosition)
        {
            //First we need to determine which of the connection points the query point is closest to
            float closestDistance = -1.0f;
            int closestConnectionIndex = -1;

            for (int i = 0; i < _geometry.SegmentConnections.Length; i++)
            {
                Vector3 connectionPosition = transform.position + _geometry.SegmentConnections[i].Centroid;
                float distanceToConnection = Vector3.Distance(connectionPosition, queryPosition);

                if (closestDistance < 0)
                {
                    closestDistance = distanceToConnection;
                    closestConnectionIndex = i;
                }
                else if (distanceToConnection < closestDistance)
                {
                    closestDistance = distanceToConnection;
                    closestConnectionIndex = i;
                }
            }

            if (Connections[closestConnectionIndex] != null)
            {
                return -1;
            }
            else
            {
                return closestConnectionIndex;
            }
        }

        public SegmentConnection GetConnection(int index)
        {
            return _geometry.SegmentConnections[index];
        }

        public int GetConnectionIndexFromConnectionID(string id)
        {
            switch (id.Substring(id.Length - 1))
            {
                case "A":
                    id = id.Substring(0, id.Length - 1) + "B";
                    break;

                case "B":
                    id = id.Substring(0, id.Length - 1) + "A";
                    break;
            }

            for (int i = 0; i < _geometry.SegmentConnections.Length; i++)
            {
                if (_geometry.SegmentConnections[i].ConnectionID == id)
                {
                    return i;
                }
            }

            return -1;
        }

        public Vector3 GetCentroidWorldPosition(int centroidIndex)
        {
            return transform.position + Vector3.Scale(_geometry.SegmentConnections[centroidIndex].Centroid, transform.localScale);
        }

        public Vector3 GetSnappingPositionForCentroid(int centroidIndex, Vector3 snapToCentroidPos)
        {
            Vector3 snapPoint = snapToCentroidPos;

            snapPoint -= Vector3.Scale(_geometry.SegmentConnections[centroidIndex].Centroid, transform.localScale);

            return snapPoint;
        }

        protected override void OnLayerChanged(LayerManager.EditorLayer newLayer)
        {
            if(newLayer == LayerManager.EditorLayer.Mine)
            {
                ChangeModeToEdit(true);
            }
            else
            {
                ChangeModeToNonEdit();
            }

            if(newLayer == LayerManager.EditorLayer.Ventilation)
            {
                // Make default vent node? hide/show vent nodes?
            }
        }

        public void StartMouseDrag(Placer placer)
        {            
            _mineTileSnapped = false;

            //SetReturnPoint(_placeable);
        }

        public void ProcessMouseDrag(ScenarioCursorData prev, ScenarioCursorData current)
        {
            //transform.localScale = Vector3.one;
            if (MineLayerTileManager == null || MineLayerTileManager.GetPlacedTileCount() <= 0)
            {
                _isOriginTile = true;
                if (Vector3.Distance(transform.position, Vector3.zero) < MineTileSnapDistance)
                {
                    _mineTileSnapped = true;

                    transform.position = Vector3.zero;
                    return;
                }
            }
            else if(MineLayerTileManager.GetPlacedTileCount() == 1 && !transform.GetComponent<PlacablePrefab>().IsPlaced)// This check is here to account for the
            {                                                                                                            // edge case of rotating the first mine tile
                if (Vector3.Distance(transform.position, Vector3.zero) < MineTileSnapDistance)                           // which causes MineLayerTileManager to have one phantom 
                {                                                                                                        // mine tile. Suggest eventually overhauling rotation code.
                    _mineTileSnapped = true;
                    transform.position = Vector3.zero;
                    return;
                }
            }
            else
            {
                _isOriginTile = false;
            }

            if (SearchForClosestTileConnection(current.GroundPos, out var closestTile, out int s_index,
                out int t_index))
            {
                //Debug.Log($"Snapping to {closestTile.name} {s_index}:{t_index}");

                _mineTileSnapped = true;
                transform.position = GetSnappingPositionForCentroid(
                    s_index, closestTile.GetCentroidWorldPosition(t_index));
            }
            else 
            {
                //Debug.Log($"No nearby connections");
                var dist = Vector3.Distance(current.GroundPos, _placedPosition);
                
                if (_placedPositionValid && dist < MineTileSnapDistance)
                {
                    ReturnToLastValid();
                }
                else
                {
                    transform.position = current.GroundPos;
                }
                
                _mineTileSnapped = false;

            }
            //Debug.Log($"MineLayerTile: ProcessMouseDrag {transform.position}");
        }

        public void CompleteMouseDrag()
        {
            Debug.Log($"MineLayerTile ${name} dropped - snapped:{_mineTileSnapped}");
            if (!_mineTileSnapped)
            {
                if (_placedPositionValid)
                {
                    ReturnToLastValid();
                    _placeable.SetPlaced();
                }
                else if(_isOriginTile)
                {
                    _placeable.SetPlaced();
                }
                else
                    DestroyTile();
            }
            
            if (MineLayerTileManager != null)
            {
                MineLayerTileManager.RebuildTileConnections();
            }

            if (_mineTileSnapped)
            {
                SetReturnPoint(transform.position);
                _placeable.SetPlaced();
            }


            //Debug.Log($"Mine tile {gameObject.name} dropped: Connections:");
            //foreach (var connection in Connections)
            //{
            //    if (connection != null)
            //        Debug.Log($"Connection: {connection.name}");
            //}
        }


        public void SearchForNearbyTiles(Vector3 pos, Collider[] results, out int numResults)
        {
            numResults = 0;

            if (MineLayerTileManager == null)
                return;

            var radius = GetBoundingRadius() * 3.0f;
            MineLayerTileManager.SearchForNearbyTiles(pos, radius, results, out numResults);

            //numResults = Physics.OverlapSphereNonAlloc(pos, radius, results,
            //    _raycastMineTilesMask, QueryTriggerInteraction.Collide);

            //if (numResults == results.Length)
            //    Debug.LogError($"SearchForNearbyTiles: Met or exceeded max number of results (radius: {radius:F2})");
        }

        private SegmentConnection[] GetSegmentConnections(MineSegment seg)
        {
            //if (!obj.TryGetComponent<MineSegment>(out var sourceSeg))
            //    return null;

            if (seg == null || seg.SegmentGeometry == null)
                return null;

            var sourceConnections = seg.SegmentGeometry.SegmentConnections;
            if (sourceConnections == null || sourceConnections.Length <= 0)
                return null;

            return sourceConnections;

        }
        public float GetBoundingRadius()
        {
            float radius = 20.0f;
            if (TryGetComponent<BoxCollider>(out var collider))
            {
                var max = collider.bounds.extents;
                radius = Mathf.Max(max.x, max.y, max.z);
            }

            return radius;
        }

        private bool SearchForClosestTileConnection(Vector3 pos, 
            out MineLayerTile closestTile, out int sourceConnIndex, out int targetConnIndex)
        {
            var source = this;
            float minDist = float.MaxValue;
            closestTile = null;
            sourceConnIndex = -1;
            targetConnIndex = -1;

            SearchForNearbyTiles(pos, _mineTileResults, out _numMineTileResults);

            if (_numMineTileResults <= 0)
                return false;

            var sourceSeg = gameObject.GetComponent<MineSegment>();
            var sourceConnections = GetSegmentConnections(sourceSeg);

            for (int i = 0; i < _numMineTileResults; i++)
            {
                var collider = _mineTileResults[i];
                if (collider == null)
                    continue;

                if (collider.gameObject == source.gameObject)
                    continue;

                if (!collider.TryGetComponent<MineLayerTile>(out var tile))
                    continue;

                var targetSeg = collider.gameObject.GetComponent<MineSegment>();
                var targetConnections = GetSegmentConnections(targetSeg);
                if (targetConnections == null || targetConnections.Length <= 0)
                    continue;

                //calculate offset from segments current location to mouse ground position
                var offset = pos - transform.position;

                for (int s_i = 0; s_i < sourceConnections.Length; s_i++)
                {
                    //var s_pos = pos + sourceConnections[s_i].Centroid;
                    var s_pos = sourceConnections[s_i].GetWorldSpaceCentroid(sourceSeg.transform) + offset;

                    for (int t_i = 0; t_i < targetConnections.Length; t_i++)
                    {
                        if (tile.IsConnectionOccupied(t_i))
                        {
                            //Debug.DrawLine(s_pos, targetConnections[t_i].GetWorldSpaceCentroid(targetSeg.transform), Color.red);
                            continue;
                        }

                        if (!SegmentConnection.DoConnectionsMatch(sourceConnections[s_i].ConnectionID,
                            targetConnections[t_i].ConnectionID))
                            continue;

                        //var t_pos = collider.transform.position + targetConnections[t_i].Centroid;
                        var t_pos = targetConnections[t_i].GetWorldSpaceCentroid(targetSeg.transform);
                        var dist = Vector3.Distance(s_pos, t_pos);

                        if (dist < minDist)
                        {
                            minDist = dist;
                            sourceConnIndex = s_i;
                            targetConnIndex = t_i;
                            closestTile = tile;
                        }
                    }
                }
            }

            //if (closestTile != null)
            //{
            //    Debug.Log($"Closest Connection: {closestTile.name} {sourceConnIndex}:{targetConnIndex} dist {minDist:F2}");
            //}

            if (minDist < MineTileSnapDistance && sourceConnIndex >= 0 && targetConnIndex >= 0)
                return true;

            return false;
        }
    }
}