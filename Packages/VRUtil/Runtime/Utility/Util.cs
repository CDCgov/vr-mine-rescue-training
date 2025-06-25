using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine.EventSystems;
using UnityEngine.AddressableAssets;
using System;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using System.IO;
using Unity.Collections;

public enum VRMineLogType
{
    Debug,
    Info,
    Warning,
    Error
}

public struct MeshData
{
    public Transform transform;
    public Bounds bounds;
    public Renderer renderer;
    public MeshFilter meshfilter;
    public Matrix4x4 matrix;
}

public class MeshPreviewData
{
    public List<MeshData> MeshData;
}

public static class Util
{
    public static bool EnableDontDestroyOnLoad = true;

    public const int GIZMO_LAYER = 7;

    public static bool IsPointerOverUI
    {
        get
        {
            if (EventSystem.current == null)
                return false;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                //var pointerModule = EventSystem.current.currentInputModule as PointerInputModule;
                //if (pointerModule != null)
                //{
                //    pointerModule.getl
                //}

                var inputModule = EventSystem.current.currentInputModule as InputSystemUIInputModule;
                if (inputModule != null)
                {
                    var raycastResult = inputModule.GetLastRaycastResult(0);
                    if (raycastResult.gameObject != null && raycastResult.gameObject.layer == GIZMO_LAYER)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }

    public static bool IsUserEnteringText
    {
        get
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                UnityEngine.UI.InputField inputField;
                TMPro.TMP_InputField tmpInputField;

                if (EventSystem.current.currentSelectedGameObject.TryGetComponent<UnityEngine.UI.InputField>(out inputField) ||
                    EventSystem.current.currentSelectedGameObject.TryGetComponent<TMPro.TMP_InputField>(out tmpInputField))
                {

                    return true;
                }
            }

            return false;
        }
    }

    public static void DontDestroyOnLoad(UnityEngine.Object obj)
    {
        if (!EnableDontDestroyOnLoad)
            return;

        GameObject.DontDestroyOnLoad(obj);
    }

    public static System.Guid ToGuid(this ByteString byteStringID)
    {
        return new System.Guid(byteStringID.ToByteArray());
    }

    public static ByteString ToByteString(this System.Guid guid)
    {
        return ByteString.CopyFrom(guid.ToByteArray());
    }

    public static Vector2 XZProjection(this Vector3 vec)
    {
        return new Vector2(vec.x, vec.z);
    }

    public static Vector3 ToVector3(this VRNVector3 vec)
    {
        return new Vector3
        {
            x = vec.X,
            y = vec.Y,
            z = vec.Z,
        };
    }

    public static VRNVector3 ToVRNVector3(this Vector3 vec)
    {
        return new VRNVector3
        {
            X = vec.x,
            Y = vec.y,
            Z = vec.z,
        };
    }

    public static void CopyTo(this Vector3 source, VRNVector3 target)
    {
        if (target == null)
        {
            return;
        }

        target.X = source.x;
        target.Y = source.y;
        target.Z = source.z;
    }

    public static void ResetTransformData(this VRNTransformData data)
    {
        if (data.Position == null)
            data.Position = new VRNVector3();
        if (data.Rotation == null)
            data.Rotation = new VRNQuaternion();

        data.Position.X = 0;
        data.Position.Y = 0;
        data.Position.Z = 0;

        data.Rotation.X = 0;
        data.Rotation.Y = 0;
        data.Rotation.Z = 0;
        data.Rotation.W = 0;
    }

    public static void ResetData(this VRNVector3 vec)
    {
        if (vec == null)
            return;

        vec.X = 0;
        vec.Y = 0;
        vec.Z = 0;
    }

    public static void ResetVRNVector3(ref VRNVector3 vec)
    {
        if (vec == null)
            vec = new VRNVector3();
        vec.X = 0;
        vec.Y = 0;
        vec.Z = 0;
    }

    public static void ResetData(this VRNQuaternion q)
    {
        if (q == null)
            return;

        q.X = 0;
        q.Y = 0;
        q.Z = 0;
        q.W = 0;
    }

    public static void ResetVRNQuaternion(ref VRNQuaternion q)
    {
        q.X = 0;
        q.Y = 0;
        q.Z = 0;
        q.W = 0;
    }

    public static Quaternion ToQuaternion(this VRNQuaternion quat)
    {
        return new Quaternion
        {
            w = quat.W,
            x = quat.X,
            y = quat.Y,
            z = quat.Z,
        };
    }

    public static void CopyTo(this Quaternion source, VRNQuaternion target)
    {
        if (target == null)
        {
            return;
        }

        target.X = source.x;
        target.Y = source.y;
        target.Z = source.z;
        target.W = source.w;
    }

    public static VRNQuaternion ToVRNQuaternion(this Quaternion quat)
    {
        return new VRNQuaternion
        {
            W = quat.w,
            X = quat.x,
            Y = quat.y,
            Z = quat.z,
        };
    }

    public static bool Raycast(this Plane plane, Ray ray, out Vector3 pt)
    {
        pt = Vector3.zero;

        if (!plane.Raycast(ray, out float enter))
            return false;

        pt = ray.origin + ray.direction * enter;
        return true;
    }

    /// <summary>
    /// Set the parents transform to the inverse of the childs
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    public static void ComputeInverseTransform(Transform parent, Transform child)
    {
        parent.localRotation = Quaternion.identity;
        parent.localPosition = Vector3.zero;

        //compute the calibration values to cancel out the current local position
        //and rotation of the HMD

        var euler = child.localRotation.eulerAngles;
        euler.x = 0;
        euler.z = 0;
        var calRot = Quaternion.Inverse(Quaternion.Euler(euler));

        parent.localRotation = calRot;

        var calOffset = child.localPosition * -1.0f;
        calOffset.y = 0;
        //calOffset = child.TransformDirection(calOffset);
        //calOffset = parent.InverseTransformDirection(calOffset);

        //offset should be calculated in the parent's space (world space if null)
        calOffset = parent.TransformDirection(calOffset);
        if (parent.parent != null)
        {
            calOffset = parent.parent.InverseTransformDirection(calOffset);
        }

        parent.localPosition = calOffset;
    }

    /// <summary>
    /// Update the parent transform's rotation so the child's position is on the positive Z axis
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    public static void UpdateCalibrationRotation(Transform parent, Transform child, Vector3 calPtWorldSpace)
    {
        //back-compute the calibration offset vector
        var calOffset = parent.localPosition;
        if (parent.parent != null)
        {
            //back to world space if we are in the parent's parent space
            calOffset = parent.parent.TransformDirection(calOffset);
        }
        //from world space to the parent's space
        calOffset = parent.InverseTransformDirection(calOffset);
        var calPos = calOffset * -1.0f;
        calPos.y = 0;
        Debug.Log($"CalPos Calculated: {calPos}");

        //modify the rotation
        var euler = Quaternion.Inverse(parent.localRotation).eulerAngles;
        euler.x = 0;
        euler.z = 0;

        Debug.Log($"Original Cal Rot: {euler}");

        //compute a Y axis rotation so that the world space vector from parent to cal pt
        //is along the forward axis

        Debug.Log($"ParentPos: {parent.parent.position} CalPos: {parent.TransformPoint(Vector3.zero)}");
        //Vector3 newForward = calPtWorldSpace - parent.parent.position;
        //newForward = parent.parent.InverseTransformDirection(newForward);
        //Vector3 curForward = parent.parent.InverseTransformDirection(parent.forward);

        //var localPos = child.localPosition;
        //var localPos = child.InverseTransformPoint(calPtWorldSpace) + child.localPosition;
        var localPos = parent.InverseTransformPoint(calPtWorldSpace);

        var newForward = localPos - calPos;
        var curForward = Vector3.forward;

        newForward.y = 0;
        curForward.y = 0;

        newForward.Normalize();
        Debug.Log($"curForward: {curForward} newForward: {newForward}");
        //euler.y = euler.y + Vector3.Angle(newForward, curForward);
        //euler.y = Vector3.Angle(newForward, curForward);

        var calQuat = Quaternion.LookRotation(newForward, Vector3.up);
        //var calQuat = Quaternion.FromToRotation(Vector3.forward, newForward);
        //var calQuat = Quaternion.FromToRotation(newForward, Vector3.forward);

        //euler.y = Vector3.Angle(Vector3.forward, newForward);
        euler.y = calQuat.eulerAngles.y;

        Debug.Log($"New Cal Rot: {euler}");

        //var calRot = Quaternion.Inverse(Quaternion.Euler(euler));
        var calRot = Quaternion.Inverse(calQuat);
        //Debug.Log($"calRot: {calRot}");

        parent.localRotation = calRot;

        //recompute the offset
        calOffset = parent.TransformDirection(calOffset);
        if (parent.parent != null)
        {
            calOffset = parent.parent.InverseTransformDirection(calOffset);
        }

        parent.localPosition = calOffset;

    }

    public static string GetColoredText(this Vector3 vec)
    {
        return string.Format("<color=red>{0,-5:F2}</color> <color=#00FF2BFF>{1,-5:F2}</color> <color=#00D4FFFF>{2,-5:F2}</color>", vec.x, vec.y, vec.z);
    }

    public static string GetColoredText(this Quaternion q)
    {
        Vector3 vec = q.eulerAngles;
        return string.Format("<color=red>{0,-5:F2}</color> <color=#00ff00ff>{1,-5:F2}</color> <color=#00D4FFFF>{2,-5:F2}</color>", vec.x, vec.y, vec.z);
    }

    public static void Log(string message, VRMineLogType logType, params object[] parameters)
    {
        string log = string.Format(message, parameters);

        if (logType == VRMineLogType.Error)
            Debug.LogError(log);
        else
            Debug.Log(log);
    }

    public static void Log(string message, params object[] parameters)
    {
        Log(message, VRMineLogType.Debug, parameters);
    }

    public static GameObject InstantiateResource(string resourceName)
    {
        GameObject obj = Resources.Load<GameObject>(resourceName);
        if (obj == null)
            return null;

        return GameObject.Instantiate<GameObject>(obj);
    }

    public static MeshPreviewData BuildPreviewData(GameObject obj)
    {
        MeshPreviewData data = new MeshPreviewData();
        data.MeshData = new List<MeshData>();

        AddMeshData(obj.transform, Matrix4x4.identity, data.MeshData);

        return data;
    }

    private static void AddMeshData(Transform obj, Matrix4x4 sourceMat, List<MeshData> dataList)
    {
        if (obj == null)
            return;

        Matrix4x4 baseMat = sourceMat * Matrix4x4.TRS(-obj.position, Quaternion.identity, Vector3.one);

        foreach (Renderer rend in obj.GetComponentsInChildren<Renderer>(true))
        {
            MeshData data = new MeshData();
            data.transform = rend.transform;
            data.bounds = rend.bounds;
            data.renderer = rend;
            data.meshfilter = rend.GetComponent<MeshFilter>();
            data.matrix = baseMat * rend.transform.localToWorldMatrix;

            dataList.Add(data);
        }
    }

    public static void DrawPreview(Transform sourceTransform, MeshPreviewData previewData)
    {
        if (previewData == null || previewData.MeshData == null)
            return;

        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Matrix4x4 localToWorld = sourceTransform.localToWorldMatrix;

        foreach (MeshData data in previewData.MeshData)
        {
            if (data.meshfilter != null)
            {
                Matrix4x4 mat = localToWorld * data.matrix;
                //Gizmos.DrawMesh(data.meshfilter.sharedMesh, data.transform.position, data.transform.rotation, data.transform.lossyScale);

                data.renderer.sharedMaterial.SetPass(0);
                Gizmos.matrix = mat;
                Gizmos.DrawMesh(data.meshfilter.sharedMesh);

                //Graphics.DrawMesh(data.meshfilter.sharedMesh, data.transform.position + Vector3.up, data.transform.rotation, data.renderer.sharedMaterial, 0);

                data.renderer.sharedMaterial.SetPass(0);
                Graphics.DrawMeshNow(data.meshfilter.sharedMesh, mat);
                //Graphics.DrawMeshNow(data.meshfilter.sharedMesh, data.transform.position + Vector3.up, data.transform.rotation);
            }
        }
    }

    public static void DestoryAllChildren(Transform parent)
    {
        if (Application.isPlaying)
        {
            foreach (Transform child in parent)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        else
            DestroyAllChildrenImmediate(parent);
    }

    public static void DestroyAllChildrenImmediate(Transform parent)
    {
        /*foreach (Transform child in parent)
		{
			GameObject.DestroyImmediate(child.gameObject);
		}*/

        //stupid hack to actually destroy all children
        LinkedList<Transform> children = new LinkedList<Transform>();

        foreach (Transform child in parent)
        {
            children.AddLast(child);
        }

        foreach (Transform child in children)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
    }

    public static float XZDistance(this Vector3 v1, Vector3 v2)
    {
        Vector3 v = v2 - v1;
        return Mathf.Sqrt(v.x * v.x + v.z * v.z);
    }

    public static float XZMagnitude(this Vector3 v)
    {
        return Mathf.Sqrt(v.x * v.x + v.z * v.z);
    }



    /// <summary>
    /// Compute the world-space bounds of the game object including all 
    /// child renderers
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Bounds ComputeBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<MeshRenderer>();

        if (renderers.Length <= 0)
        {
            return new Bounds();
        }
        else if (renderers.Length == 1)
        {
            Debug.LogFormat("Bounds from one renderer {0}", renderers[0].name);
            return renderers[0].bounds;
        }

        //2 or more renderers
        Bounds b = renderers[0].bounds;

        Debug.LogFormat("Bounds from renderer {0}", renderers[0].name);

        for (int i = 1; i < renderers.Length; i++)
        {
            Debug.LogFormat("Bounds from renderer {0}", renderers[i].name);
            b.Encapsulate(renderers[i].bounds);
        }

        return b;
    }

    public static IEnumerable<T> GetAllInterfaces<T>(List<T> list)
    {
        int numInterfaces = list.Count;
        for (int i = 0; i < numInterfaces; i++)
        {
            if (list[i] == null)
                continue;

            yield return list[i];

            //check the interfaces haven't changed
            if (numInterfaces != list.Count)
                break;
        }
    }


    private static List<GameObject> _rootObjectCache = new List<GameObject>(100);
    private static Dictionary<string, GameObject> _managerCache = new Dictionary<string, GameObject>();

    public static T GetDefaultManager<T>(this GameObject self, string objName, bool createNew = true) where T : Component
    {
        T manager = null;

        //first check the object's scene (for testing compatiblity / running multiple independent scenes simultaneously)
        if (self != null && self.scene != null)
        {
            self.scene.GetRootGameObjects(_rootObjectCache);
            for (int i = 0; i < _rootObjectCache.Count; i++)
            {
                if (_rootObjectCache[i].TryGetComponent<T>(out manager))
                {
                    if (objName != null)
                        _managerCache[objName] = manager.gameObject;

                    return manager;
                }
            }
        }

        if (objName != null && _managerCache.TryGetValue(objName, out var cachedManagerObj))
        {
            T cachedManager = null;

            if (cachedManagerObj != null)
                cachedManagerObj.TryGetComponent<T>(out cachedManager);

            if (cachedManagerObj == null || cachedManager == null || !cachedManagerObj.activeSelf)
                _managerCache.Remove(objName);
            else
            {
                return cachedManager;
            }
        }

        manager = GameObject.FindObjectOfType<T>();
        if (manager != null)
        {
            if (objName != null)
                _managerCache[objName] = manager.gameObject;

            return manager;
        }

        var obj = GameObject.Find(objName);
        if (obj == null)
        {
            if (createNew)
            {
                obj = new GameObject(objName);
                //obj.tag = "Manager";
            }
            else
                return null;
        }

        manager = obj.GetComponent<T>();
        if (manager == null)
        {
            if (createNew)
                manager = obj.AddComponent<T>();
            else
                return null;
        }

        if (objName != null)
            _managerCache[objName] = manager.gameObject;

        return manager;
    }

    public static Vector3 ClampComponents(this Vector3 v, float min, float max)
    {
        v.x = Mathf.Clamp(v.x, min, max);
        v.y = Mathf.Clamp(v.y, min, max);
        v.z = Mathf.Clamp(v.z, min, max);

        return v;
    }


    public static IEnumerator LoadAddressablesByLabel<T>(string label, Action<T> callback, Action<int> numAddressablesCallback = null)
    {
        Application.backgroundLoadingPriority = ThreadPriority.High;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        var handle1 = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        yield return handle1;

        if (numAddressablesCallback != null)
            numAddressablesCallback(handle1.Result.Count);

        //foreach (var loc in handle1.Result)
        //{
        //    Debug.Log($"Found Resource Location {loc.PrimaryKey}");
        //}

        Debug.Log($"Resource location load for {label} took {sw.ElapsedMilliseconds}ms");
        sw.Reset();
        sw.Start();

        var handle2 = Addressables.LoadAssetsAsync<T>(handle1.Result, callback);
        yield return handle2;
        
        Debug.Log($"Asset load for {label} took {sw.ElapsedMilliseconds}ms");
        sw.Reset();
        sw.Start();

        Addressables.Release(handle1);
        //Addressables.Release(handle2);
        Debug.Log($"Handle release for {label} took {sw.ElapsedMilliseconds}ms");
        Application.backgroundLoadingPriority = ThreadPriority.Normal;
    }

    public static Vector3 ClosestPointOnLine(Vector3 pt, Ray ray)
    {
        var v1 = pt - ray.origin;
        var dir = ray.direction.normalized;

        float distOnLine = Vector3.Dot(v1, dir);

        Vector3 ptOnLine = ray.origin + dir * distOnLine;

        return ptOnLine;
    }    

    public static float DistOnRay(Vector3 pt, Ray ray)
    {
        var v1 = pt - ray.origin;
        var dir = ray.direction.normalized;

        float distOnLine = Vector3.Dot(v1, dir);

        return distOnLine;
    }

    public static Vector3 ClosestPointOnLineSegment(Vector3 pt, Ray ray, float length)
    {
        var v1 = pt - ray.origin;
        var dir = ray.direction.normalized;

        float distOnLine = Vector3.Dot(v1, dir);

        if (distOnLine <= 0)
            return ray.origin;
        else if (distOnLine > length)
            distOnLine = length;

        Vector3 ptOnLine = ray.origin + dir * distOnLine;

        return ptOnLine;
    }


    public static float DistanceToLine(Vector3 pt, Ray ray)
    {
        var ptOnLine = ClosestPointOnLine(pt, ray);

        return Vector3.Distance(pt, ptOnLine);
    }

    //part of swing-twist decomposition
    //see https://stackoverflow.com/questions/3684269/component-of-a-quaternion-rotation-around-an-axis
    // https://www.euclideanspace.com/maths/geometry/rotations/for/decomposition/
    // https://github.com/TheAllenChou/unity-cj-lib/blob/master/Unity%20CJ%20Lib/Assets/CjLib/Script/Math/QuaternionUtil.cs
    public static float TwistAngle(Quaternion q, Vector3 axis)
    {
        //q.ToAngleAxis(out float angle, out Vector3 qAxis);

        //var dot = Vector3.Dot(axis, qAxis);
        var r = new Vector3(q.x, q.y, q.z);
        var proj = Vector3.Project(r, axis);
        var twist = new Quaternion(proj.x, proj.y, proj.z, q.w);
        twist.Normalize();

        twist.ToAngleAxis(out float twistAngle, out Vector3 twistAxis);
        //Debug.Log($"Twist Axis: {twistAxis} r: {r} axis: {axis}");

        if (Vector3.Dot(axis, twistAxis) < 0.1f)
        {
            //axis flipped, negate angle
            twistAngle *= -1.0f;
        }
        
        return twistAngle;
    }


    //modified from https://github.com/TheAllenChou/unity-cj-lib/blob/master/Unity%20CJ%20Lib/Assets/CjLib/Script/Math/QuaternionUtil.cs
    //License: MIT
    public static void DecomposeSwingTwist
    (
      Quaternion q,
      Vector3 twistAxis,
      out Quaternion swing,
      out Quaternion twist
    )
    {
        Vector3 r = new Vector3(q.x, q.y, q.z); // (rotaiton axis) * cos(angle / 2)

        // singularity: rotation by 180 degree
        if (r.sqrMagnitude < 1.0e-9f)
        {
            Vector3 rotatedTwistAxis = q * twistAxis;
            Vector3 swingAxis = Vector3.Cross(twistAxis, rotatedTwistAxis);

            if (swingAxis.sqrMagnitude > 1.0e-9f)
            {
                float swingAngle = Vector3.Angle(twistAxis, rotatedTwistAxis);
                swing = Quaternion.AngleAxis(swingAngle, swingAxis);
            }
            else
            {
                // more singularity: rotation axis parallel to twist axis
                swing = Quaternion.identity; // no swing
            }

            // always twist 180 degree on singularity
            //twist = Quaternion.AngleAxis(180.0f, twistAxis);
            twist = Quaternion.identity;
            return;
        }

        // formula & proof: 
        // http://www.euclideanspace.com/maths/geometry/rotations/for/decomposition/
        Vector3 p = Vector3.Project(r, twistAxis);
        twist = new Quaternion(p.x, p.y, p.z, q.w);
        twist = twist.normalized;
        swing = q * Quaternion.Inverse(twist);
    }

    public static Vector3 ComponentDivide(this Vector3 a, Vector3 b)
    {
        Vector3 v = new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

        return v;
    }

    public static void WriteVector(this BinaryWriter writer, Vector2 v)
    {
        writer.Write((float)v.x);
        writer.Write((float)v.y);
    }

    public static void WriteVector(this BinaryWriter writer, Vector3 v)
    {
        writer.Write((float)v.x);
        writer.Write((float)v.y);
        writer.Write((float)v.z);
    }

    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        Vector2 v;

        v.x = reader.ReadSingle();
        v.y = reader.ReadSingle();

        return v;
    }

    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        Vector3 v;

        v.x = reader.ReadSingle();
        v.y = reader.ReadSingle();
        v.z = reader.ReadSingle();

        return v;
    }

    public static void SaveTextureCache(Texture2D tex, string path, bool linear)
    {
        var rawTexData = tex.GetRawTextureData();

        using var stream = new FileStream(path, FileMode.CreateNew);
        using var writer = new BinaryWriter(stream);

        writer.Write("TXCACHEV02");
        writer.Write(tex.width);
        writer.Write(tex.height);
        writer.Write(tex.mipmapCount);
        writer.Write((int)tex.format);
        writer.Write(linear);
        writer.Write(rawTexData.Length);
        writer.Write(rawTexData);
    }

    public static Texture2D LoadTextureCache(string path)
    {
        try
        {
            if (!File.Exists(path))
                return null;

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64);
            using var reader = new BinaryReader(stream);

            var header = reader.ReadString();
            if (header != "TXCACHEV02")
                return null;

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();
            int mipCount = reader.ReadInt32();
            TextureFormat format = (TextureFormat)reader.ReadInt32();
            bool linear = reader.ReadBoolean();
            int length = reader.ReadInt32();

            //if (_buffer == null || _buffer.Length < length)
            //{
            //    _buffer = new byte[Math.Max(length, 100 * 1024 * 1024)];
            //}

            //var rawTexData = reader.ReadBytes(length);
            using var buffer = new NativeArray<byte>(length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var bytesRead = stream.Read(buffer.AsSpan());

            //var rawTexData = new byte[length];
            //var bytesRead = await stream.ReadAsync(rawTexData, 0, length);

            if (bytesRead != length)
            {
                Debug.LogError($"Error reading texture cache data for {path}");
                return null;
            }

            var tex = new Texture2D(width, height, format, mipCount > 1 ? true : false, linear);

            //tex.Reinitialize(width, height, format, mipCount > 1 ? true : false);
            //tex.LoadRawTextureData(rawTexData);
            tex.LoadRawTextureData<byte>(buffer);
            //tex.LoadRawTextureData(_buffer, )
            tex.Apply();

            return tex;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error reading texture cache {path} : {ex.Message}");
            return null;
        }

        return null;
    }

    //cross product method - see https://nerdparadise.com/math/pointinatriangle
    public static bool PointInTriangle(Vector2 pt, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector3 pt3 = (Vector3)pt;
        Vector3 a3 = (Vector3)a;
        Vector3 b3 = (Vector3)b;
        Vector3 c3 = (Vector3)c;

        var ab = b3 - a3;
        var bc = c3 - b3;
        var ca = a3 - c3;

        var apt = pt3 - a3;
        var bpt = pt3 - b3;
        var cpt = pt3 - c3;

        var cr1 = Vector3.Cross(apt, ab);
        var cr2 = Vector3.Cross(bpt, bc);
        var cr3 = Vector3.Cross(cpt, ca);

        if ((cr1.z >= 0 && cr2.z >= 0 && cr3.z >= 0) ||
            (cr1.z <= 0 && cr2.z <= 0 && cr3.z <= 0))
        {
            return true;
        }

        return false;

    }

}


/// <summary>
/// Point Within Mesh - From http://answers.unity3d.com/questions/611947/am-i-inside-a-volume-without-colliders.html
/// </summary>
public static class MeshExtension
{


    /// <summary>
    /// simpler version that I think might fail completely on non-convex meshes?
    /// </summary>
    public static bool IsPointInsideConvex(this Mesh aMesh, Vector3 aLocalPoint)
    {
        var verts = aMesh.vertices;
        var tris = aMesh.triangles;
        int triangleCount = tris.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            var V1 = verts[tris[i * 3]];
            var V2 = verts[tris[i * 3 + 1]];
            var V3 = verts[tris[i * 3 + 2]];
            var P = new Plane(V1, V2, V3);
            if (P.GetSide(aLocalPoint))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Decide whether a point is within a mesh, in a good yet simplistic way.
    /// It works best with convex meshes, whereas a concave mesh is simply
    /// treated as a convex mesh and so it will not be totally exact.
    /// (For a torus/donut mesh it will be like it didn't have a hole.)
    /// </summary>
    public static bool PointIsWithin(this Mesh mesh, Vector3 point)
    {
        Vector3 p = point - mesh.bounds.center;

        for (int i = 0; i < mesh.vertices.Length; i += 3)
        {
            Vector3 a = mesh.vertices[i] - mesh.bounds.center;
            Vector3 b = mesh.vertices[i + 1] - mesh.bounds.center;
            Vector3 c = mesh.vertices[i + 2] - mesh.bounds.center;
            if (RayWithinTriangle(p, a, b, c))
                return true;
        }
        return false;
    }
    /// <summary>
    /// Radiate out from the origin through the given point to see whether
    /// the ray would hit the triangle and the point is closer to the origin than the triangle.
    /// The triangle is specified by v0, v1 and v2.
    /// </summary>
    private static bool RayWithinTriangle(Vector3 point, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        Vector3 intersectionPoint;
        if (RayIntersectsTriangle(point, v0, v1, v2, out intersectionPoint))
        {
            float pointDist = point.sqrMagnitude;
            float intersectionDist = intersectionPoint.sqrMagnitude;
            return (pointDist < intersectionDist);
        }
        return false;
    }

    /// <summary>
    /// Radiate out from the origin through the given point to see whether
    /// the ray would hit the triangle.
    /// The triangle is specified by v0, v1 and v2.
    /// </summary>
    private static bool RayIntersectsTriangle(Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 intersectionPoint)
    {
        intersectionPoint = new Vector3();

        Vector3 e1 = v1 - v0;
        Vector3 e2 = v2 - v0;

        Vector3 h = Vector3.Cross(direction, e2);
        float a = Vector3.Dot(e1, h);

        if (a > -0.00001 && a < 0.00001)
            return false;

        float f = 1 / a;
        Vector3 s = Vector3.zero - v0;
        float u = f * Vector3.Dot(s, h);

        if (u < 0.0 || u > 1.0)
            return false;

        Vector3 q = Vector3.Cross(s, e1);
        float v = f * Vector3.Dot(direction, q);

        if (v < 0.0 || u + v > 1.0)
            return false;

        // At this stage we can compute t to find out where
        // the intersection point is on the line.
        float t = f * Vector3.Dot(e2, q);

        if (t > 0.00001) // ray intersection
        {
            intersectionPoint[0] = direction[0] * t;
            intersectionPoint[1] = direction[1] * t;
            intersectionPoint[2] = direction[2] * t;
            return true;
        }

        // At this point there is a line intersection
        // but not a ray intersection.
        return false;
    }

}