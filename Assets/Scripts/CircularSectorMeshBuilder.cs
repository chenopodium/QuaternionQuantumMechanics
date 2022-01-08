using UnityEngine;
using System.Collections.Generic;
public class CircularSectorMeshBuilder : MeshBuilder
{
    [Header("Mesh Options")]
    [Min(0)]
    [SerializeField]
    private int trianglesPerRad = 5;
    [SerializeField]
    [Range(0f, 360f)]
    private float angleInDegrees = 270f;
    override protected List<Vector3> CalculateVertices() {
        var triangleCount = GetTriangleCount();
        float sectorAngle = Mathf.Deg2Rad * angleInDegrees;
        var vertices = new List<Vector3>();
        vertices.Add(Vector2.zero);
        for (int i = 0; i < triangleCount + 1; i++) {
            float theta = i / (float)triangleCount * sectorAngle;
            var vertex = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0);
            vertices.Add(vertex);
        }
        return vertices;
    }
    override protected List<int> CalculateTriangles() {
        var triangleCount = GetTriangleCount();
        var triangles = new List<int>();
        for (int i = 0; i < triangleCount; i++) {
            triangles.Add(0);
            triangles.Add(i + 2);
            triangles.Add(i + 1);
        }
        return triangles;
    }
    private int GetTriangleCount() {
        return Mathf.CeilToInt(2 * Mathf.PI * trianglesPerRad);
    }
}