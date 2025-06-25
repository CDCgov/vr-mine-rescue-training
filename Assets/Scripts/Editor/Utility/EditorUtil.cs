using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class EditorUtil
{
    private static Vector3 position;
    private static Quaternion rotation;
    private static Vector3 scale;

    private static Dictionary<string, string[]> _assetSearchCache = new Dictionary<string, string[]>();

    [MenuItem("CONTEXT/Transform/Copy Transform Global",false,151)]
    static void CopyTransformGlobal () {
        position = Selection.activeTransform.position;
        rotation = Selection.activeTransform.rotation;
        //scale = Selection.activeTransform.lossyScale;
    }

    [MenuItem("CONTEXT/Transform/Paste Transform Global",false,151)]
    static void PasteTransformGlobal () {
        Selection.activeTransform.position = position;
        Selection.activeTransform.rotation = rotation;
        //scale = Selection.activeTransform.lossyScale;
    }

    [MenuItem("Assets/Copy asset path to clipboard")]
    public static void CopyAssetPath()
    {
        var selected = Selection.activeObject;

        string path = AssetDatabase.GetAssetPath(selected);
        EditorGUIUtility.systemCopyBuffer = path;
    }

    [MenuItem("Assets/Create editor script")]
    public static void CreateEditorScript()
    {
        var selected = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(selected);

        string extension = path.Substring(path.Length - 3);
        if (string.Compare(extension, ".cs", true) != 0)
            return; 

        //Debug.Log(path);
        string filename = Path.GetFileNameWithoutExtension(path);
        string folder = Path.GetDirectoryName(path);
        folder = Path.Combine(folder, "Editor");
        string editorFileName = Path.Combine(folder, string.Format("{0}Editor.cs", filename));
        //Debug.Log(filename);
        //Debug.Log(folder);

        string templatePath = Path.Combine(Application.dataPath, "Editor/CustomEditorTemplate.txt");
        string templateText = File.ReadAllText(templatePath);
        templateText = templateText.Replace("{CLASSNAME}", string.Format("{0}Editor", filename));
        templateText = templateText.Replace("{OBJECTCLASS}", string.Format("{0}", filename));

        Directory.CreateDirectory(Path.GetDirectoryName(editorFileName));
        File.WriteAllText(editorFileName, templateText);

        
    }

    [MenuItem("Test/Subdivide Mesh")]
    public static void SubdivideMesh()
    {		
        var selected = Selection.activeObject as GameObject;

        if (selected == null)
            return;

        MeshFilter mf = selected.GetComponent<MeshFilter>();
        if (mf == null)
            return;

        Mesh m = mf.sharedMesh;
        if (m == null || m.vertices == null || m.normals == null || m.triangles == null)
            return;


        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        

        for (int i = 0; i < m.triangles.Length - 2; i += 3)
        {
            int t1 = m.triangles[i];
            int t2 = m.triangles[i+1];
            int t3 = m.triangles[i+2];

            Vector3 v1 = m.vertices[t1];
            Vector3 v2 = m.vertices[t2];
            Vector3 v3 = m.vertices[t3];

            Vector3 n1 = m.normals[t1];
            Vector3 n2 = m.normals[t2];
            Vector3 n3 = m.normals[t3];

            Vector3 vMid = v1 * 0.3333f + v2 * 0.3333f + v3 * 0.3333f;
            Vector3 nMid = n1 * 0.3333f + n2 * 0.3333f + n3 * 0.3333f;
            nMid.Normalize();

            int vBaseIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(vMid);

            normals.Add(n1);
            normals.Add(n2);
            normals.Add(n3);
            normals.Add(nMid);

            triangles.Add(vBaseIndex + 0);
            triangles.Add(vBaseIndex + 1);
            triangles.Add(vBaseIndex + 3);

            triangles.Add(vBaseIndex + 1);
            triangles.Add(vBaseIndex + 2);
            triangles.Add(vBaseIndex + 3);

            triangles.Add(vBaseIndex + 0);
            triangles.Add(vBaseIndex + 3);
            triangles.Add(vBaseIndex + 2);
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = triangles.ToArray();

        mf.mesh = newMesh;

    }

    [MenuItem("Test/Editor Screenshot _F12")]
    public static void EditorScreenshot()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            //Debug.Log("Found scene view");
            Camera cam = sceneView.camera;			

            if (cam != null)
            {
                //Debug.Log("Found scene camera");

                if (cam.targetTexture == null)
                {
                    Debug.Log("No render texture :( ");
                    return;
                }

                /*RenderTexture rtOrig = cam.targetTexture;
                RenderTexture rtScreenshot = new RenderTexture(640, 480, 24);
                cam.targetTexture = rtScreenshot;
                cam.Render();
                cam.targetTexture = rtOrig; */

                RenderTexture rtScreenshot = cam.targetTexture;

                Texture2D tex = new Texture2D(rtScreenshot.width, rtScreenshot.height, TextureFormat.RGB24, false);
                var oldRenderTexture = RenderTexture.active;
                RenderTexture.active = rtScreenshot;
                tex.ReadPixels(new Rect(0, 0, rtScreenshot.width, rtScreenshot.height), 0, 0);
                RenderTexture.active = oldRenderTexture;

                Directory.CreateDirectory("Screenshots");
                string filename = string.Format("Screenshots/{0}.png", System.DateTime.Now.ToString("yyyy-dd-M_HH-mm-ss"));

                byte[] pngData = tex.EncodeToPNG();
                File.WriteAllBytes(filename, pngData);
                Debug.Log("Screenshot saved to " + filename);

            }
        }

        /*
        string path = Directory.GetCurrentDirectory();
        path = Path.Combine(path, "Screenshots");
        Directory.CreateDirectory("Screenshots");

        string filename = string.Format("Screenshot_{0}.png", System.DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));
        filename = "test2.png";
        filename = Path.Combine(path, filename);
        filename = "Screenshots/test2.png"; */

        /*
        Directory.CreateDirectory("Screenshots");
        string filename = string.Format("Screenshots/{0}.png", System.DateTime.Now.ToString("yyyy-dd-M_HH-mm-ss"));
        ScreenCapture.CaptureScreenshot(filename); 


        Debug.Log("Screenshot saved to " + filename); */
    }


    public static void DrawDropShadowHandlesLabel(Vector3 wpos, string label, GUIStyle style)
    {
        Vector2 guiPos = HandleUtility.WorldToGUIPoint(wpos);
        Handles.BeginGUI();
        //GUI.Box(new Rect(guiPos.x, guiPos.y, 100, 30), "");
        //GUILayout.BeginArea(new Rect(guiPos.x, guiPos.y, 100, 30), _bgTex);
        //GUILayout.Label("Hello Box World", _labelStyle);
        EditorGUI.DropShadowLabel(new Rect(guiPos.x, guiPos.y - 25, 200, 50), label, style);
        //GUILayout.EndArea();
        Handles.EndGUI();
    }

    public static void ClearAssetSearchCache()
    {
        _assetSearchCache = new Dictionary<string, string[]>();
    }

    public static string[] FindAllAssets(string searchFilter)
    {
        string[] assetPaths = null;

        if (_assetSearchCache.TryGetValue(searchFilter, out assetPaths))
        {
            return assetPaths;
        }

        var guids = AssetDatabase.FindAssets(searchFilter);
        if (guids == null | guids.Length <= 0)
            return null;

        assetPaths = new string[guids.Length];

        for (int i = 0; i < assetPaths.Length; i++)
        {
            assetPaths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
        }

        _assetSearchCache[searchFilter] = assetPaths;

        return assetPaths;
    }

    public static string[] GetAssetDisplayNames(string[] assetPaths)
    {
        if (assetPaths == null || assetPaths.Length <= 0)
            return null;

        string[] names = new string[assetPaths.Length];

        for (int i = 0; i < assetPaths.Length; i++)
        {
            string path = assetPaths[i];
            int lastSlash = path.LastIndexOf('/');
            
            if (lastSlash > 0)
                path = path.Substring(lastSlash + 1);

            int lastPeriod = path.LastIndexOf('.');
            if (lastPeriod > 0)
                path = path.Substring(0, lastPeriod);
            
            names[i] = path;
        }

        return names;
    }

    public static T[] LoadAllAssets<T>(string searchFilter)
    {
        var assetPaths = FindAllAssets(searchFilter);
        if (assetPaths == null || assetPaths.Length <= 0)
            return null;

        T[] objs = new T[assetPaths.Length];

        return objs;
    }

    public static bool ObjectSelector<T>(string label, string searchString, ref T obj, Object target, string UndoMessage) where T : UnityEngine.Object
    {
        bool changed = false;

        var paths = EditorUtil.FindAllAssets(searchString);
        if (paths == null)
            return false;

        var names = EditorUtil.GetAssetDisplayNames(paths);
        if (names == null)
            return false;

        int selectedIndex = -1;
        string selectedPath = null;

        if (obj != null)
        {
            selectedPath = AssetDatabase.GetAssetPath(obj);

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] == selectedPath)
                {
                    selectedIndex = i;
                    break;
                }
            }
        }

        int newIndex = EditorGUILayout.Popup(label, selectedIndex, names);

        if ((newIndex != selectedIndex || selectedIndex < 0) && newIndex >= 0)
        {
            UnityEditor.Undo.RecordObject(target, UndoMessage);

            obj = AssetDatabase.LoadAssetAtPath<T>(paths[newIndex]);
            changed = true;
        }

        return changed;
    }

    public static bool MaterialSelector(string label, string searchString, ref Material mat)
    {
        bool matChanged = false;

        var matPaths = EditorUtil.FindAllAssets(searchString);
        if (matPaths == null)
            return false;

        var matNames = EditorUtil.GetAssetDisplayNames(matPaths);
        if (matNames == null)
            return false;

        int selectedIndex = -1;
        string selectedPath = null;

        if (mat != null)
        {
            selectedPath = AssetDatabase.GetAssetPath(mat);
            
            for (int i = 0; i < matPaths.Length; i++)
            {
                if (matPaths[i] == selectedPath)
                {					
                    selectedIndex = i;
                    break;
                }
            }
        }

        int newIndex = EditorGUILayout.Popup(label, selectedIndex, matNames);

        if ((newIndex != selectedIndex || selectedIndex < 0) && newIndex >= 0)
        {
            mat = AssetDatabase.LoadAssetAtPath<Material>(matPaths[newIndex]);
            matChanged = true;
        }		

        if (mat != null)
        {
            //DrawMaterialPreview(GUILayoutUtility.GetRect(200, 300), mat);
        }
        /*
        if (mat != null)
        {
            Editor matEdit = Editor.CreateEditor(mat);
            matEdit.OnPreviewGUI(GUILayoutUtility.GetRect(200, 300), EditorStyles.whiteLabel);
        }
        */

        return matChanged;
    }


    private static PreviewRenderUtility _previewRenderer;
    private static Mesh _boxMesh;

    public static void DrawMaterialPreview(Rect r, Material mat)
    {
        if (_previewRenderer == null)
        {
            _previewRenderer = new PreviewRenderUtility();

        }

        if (_boxMesh == null)
        {
            _boxMesh= Resources.Load("SquareTest", typeof(Mesh)) as Mesh;
        }

        _previewRenderer.camera.transform.position = (Vector3)(-Vector3.forward * 8f);
        _previewRenderer.camera.transform.rotation = Quaternion.identity;
        _previewRenderer.camera.farClipPlane = 30;

        _previewRenderer.lights[0].intensity = 0.5f;
        _previewRenderer.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);
        _previewRenderer.lights[1].intensity = 0.5f;

        _previewRenderer.BeginPreview(r, GUIStyle.none);
        _previewRenderer.DrawMesh(_boxMesh, -Vector3.up * 0.5f, Quaternion.Euler(-30f, 0f, 0f) * Quaternion.Euler(0f, 60, 0f), mat, 0);

        bool fog = RenderSettings.fog;
        Unsupported.SetRenderSettingsUseFogNoDirty(false);
        _previewRenderer.camera.Render();
        Unsupported.SetRenderSettingsUseFogNoDirty(fog);
        Texture texture = _previewRenderer.EndPreview();

        GUI.DrawTexture(r, texture);
    }


    /// <summary>
    /// Draw grid lines with the specified spacing in feet
    /// gridLines vector is a buffer for the lines that is allocated and maintained by the function
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="gridLineCount"></param>
    /// <param name="gridSpacing"></param>
    /// <param name="gridColor"></param>
    /// <param name="gridLines"></param>
    public static void DrawGridLines(Vector3 origin, int gridLineCount, float gridSpacing, Color gridColor, ref Vector3[] gridLines)
    {
        int numLinePairs = gridLineCount * 2;
        if (gridLines == null || gridLines.Length != numLinePairs * 2)
        {
            gridLines = new Vector3[numLinePairs * 2];
        }

        int i = 0;

        //Vector3 origin = fieldSystem.transform.position;
        //origin.x += fieldSystem.GridShift.x;
        //origin.z += fieldSystem.GridShift.y;

        float spacing = gridSpacing * 0.3048f;
        float size = (float)gridLineCount * spacing;

        float startX = origin.x - (size / 2);
        float endX = origin.x + (size / 2);
        float startZ = origin.z - (size / 2);
        float endZ = origin.z + (size / 2);

        for (int x = 0; x < gridLineCount; x++)
        {
            Vector3 p1 = new Vector3(startX + x * spacing + spacing / 2, origin.y, startZ);
            Vector3 p2 = new Vector3(startX + x * spacing + spacing / 2, origin.y, endZ);

            gridLines[i * 2] = p1;
            gridLines[(i * 2) + 1] = p2;

            i++;
        }

        for (int z = 0; z < gridLineCount; z++)
        {
            Vector3 p1 = new Vector3(startX, origin.y, startZ + z * spacing + spacing / 2);
            Vector3 p2 = new Vector3(endX, origin.y, startZ + z * spacing + spacing / 2);

            gridLines[i * 2] = p1;
            gridLines[(i * 2) + 1] = p2;

            i++;
        }

        Handles.color = gridColor;
        Handles.DrawLines(gridLines);
    }
}
