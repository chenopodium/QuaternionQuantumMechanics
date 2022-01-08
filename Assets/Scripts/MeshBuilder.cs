using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshBuilder : MonoBehaviour
{
    [Header("Debug Options")]
   
    [SerializeField]
    protected bool printDebugInfo = false;

    //Use property instead to ensure initialization
    private MeshFilter meshFilter;
    private Mesh mesh;

    protected List<int> triangles;
    protected List<Color> colors;
    protected List<Vector3> vertices;
    public void Start() {
        
        UpdateMesh();
    }
    protected MeshFilter MeshFilter {
      
        get {
            if (meshFilter == null) {
                meshFilter = GetComponent<MeshFilter>();
                //cannot be null since MeshFilter is required. 
            }

            return meshFilter;
        }
    }
    protected void p(string s) {
        if (!printDebugInfo) return;
        Debug.Log("MeshBuilder: " + s);
    }
    public void UpdateMesh() {
        p("UpdateMesh");
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
        DestroyOldMesh();
        Preprocess();

        mesh = new Mesh();

        CalculateVertices();

        DebugLog("vertices", vertices.Count);
        mesh.SetVertices(vertices);

        triangles = CalculateTriangles();

        DebugLog("triangles", triangles.Count);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
        var uvs = CalculateUvs(vertices);

        if (uvs == null) {
            //can be null if subclass does not support texturing
            DebugLog("uvs", null);
        }
        else {
            DebugLog("uvs", uvs.Count);
            mesh.SetUVs(0, uvs);
        }

        var normals = CalculateNormals();

        if (normals == null) {
            //can be null if subclass does override default normals
            DebugLog("normals", null);
            mesh.RecalculateNormals();
        }
        else {
            DebugLog("normals", normals.Count);
            mesh.SetNormals(normals);
        }

        mesh.RecalculateBounds();
        meshFilter.sharedMesh = mesh;
    }

    protected LineRenderer addLine(Color c, Vector3 a, Vector3 b) {
        LineRenderer lr = new GameObject("Line").AddComponent<LineRenderer>();


        //  Debug.DrawLine(a,b, Color.yellow,20f);
        Material m = new Material(Shader.Find("Sprites/Default"));
        // Material m = new Material(Shader.Find("Unlit/Texture"));
        lr.material = m;

        lr.startWidth = 0.001f;
        lr.endWidth = 0.001f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetColors(c, c);

        //For drawing line in the world space, provide the x,y,z values
        lr.SetPosition(0, a); //x,y and z position of the starting point of the line
        lr.SetPosition(1, b);
        //lr.transform.parent = sphere.transform;
        return lr;
    }
    virtual protected List<Vector3> CalculateVertices() {
        return new List<Vector3>
        {
         new Vector3(-1, -1, 0),
         new Vector3(1, -1, 0),
         new Vector3(1, 1, 0),
         new Vector3(-1, 1, 0),
      };
    }

    virtual protected List<Vector2> CalculateUvs(List<Vector3> vertices) {
        return GetStandardUvs(vertices, true, true);
    }

    virtual protected void Preprocess() { }

    virtual protected List<int> CalculateTriangles() {
        return new List<int>
        {
         0, 3, 1,
         1, 3, 2
      };
    }

    virtual protected List<Vector3> CalculateNormals() {
        return null;
    }

    [ContextMenu("Update Mesh")]
    protected void UpdateMeshTest() {
        UpdateMesh();
    }

    private void DestroyOldMesh() {
        if (MeshFilter.sharedMesh != null) {
            if (Application.isPlaying) {
                Destroy(MeshFilter.sharedMesh); //prevents memory leak
            }
            else {
                DestroyImmediate(MeshFilter.sharedMesh);
            }
        }
    }

    //Assumes a set of vertices in the XY plane
    protected List<Vector2> GetStandardUvs(
       List<Vector3> vertices,
       bool preserveAspectRatio,
       bool mapOriginToCenter
    ) {
        var boundingBox = GetBoundingBoxXY(vertices, mapOriginToCenter);
        var map = GetStandardUvMap(boundingBox, preserveAspectRatio, mapOriginToCenter);

        return vertices.Select(map).ToList();
    }

    private Rect GetBoundingBoxXY(List<Vector3> vertices, bool mapOriginToCenter) {
        if(vertices.Count<1) {
            p("Got no vertices!");
            return Rect.zero;
        }
        var anchor = vertices[0];
        var extent = vertices[0];

        foreach (var vertex in vertices.Skip(1)) {
            if (vertex.x < anchor.x) {
                anchor.x = vertex.x;
            }

            else if (vertex.x > extent.x) {
                extent.x = vertex.x;
            }

            if (vertex.y < anchor.y) {
                anchor.y = vertex.y;
            }

            else if (vertex.y > extent.y) {
                extent.y = vertex.y;
            }
        }

        if (mapOriginToCenter) {
            anchor.x = Mathf.Min(anchor.x, -extent.x);
            anchor.y = Mathf.Min(anchor.y, -extent.y);

            extent.x = Mathf.Max(extent.x, -anchor.x);
            extent.y = Mathf.Max(extent.y, -anchor.y);
        }

        var size = extent - anchor;

        return new Rect(anchor, size);

    }

    private Func<Vector3, Vector2> GetStandardUvMap(
       Rect boundingBox,
       bool preserveAspectRatio,
       bool mapOriginToCenter) {
        Vector2 anchor = boundingBox.position;
        Vector2 size = boundingBox.size;

        if (preserveAspectRatio) {
            if (size.x < size.y) {
                size = new Vector3(size.y, size.y, 0);
            }
            else {
                size = new Vector3(size.x, size.x, 0);
            }
        }

        if (mapOriginToCenter) {
            return v => new Vector2(v.x / size.x + 0.5f, v.y / size.y + 0.5f);
        }
        else {
            return v => new Vector2((v.x - anchor.x) / size.x, (v.y - anchor.y) / size.y);
        }
    }

    private void DebugLog(string label, object message) {
        if (!printDebugInfo) return;

        if (message == null) {
            DebugLog(label, "null");
        }
        else {
            Debug.Log(label + ": " + message, this);
        }
    }

    private void GetVerticesFromMesh() {
        if (MeshFilter.sharedMesh != null) {
            vertices = MeshFilter.sharedMesh.vertices.ToList();

            DebugLog("vertices", "refreshed from mesh");
        }
    }

    private void OnValidate() {
        if (vertices == null) {
            GetVerticesFromMesh();
        }
    }
}