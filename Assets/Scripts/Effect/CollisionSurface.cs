// ScriptableObjects/CollisionShapeData.cs
using System;
using UnityEngine;

/// <summary>
/// 接触面の形状データを定義するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "CollisionShape", menuName = "Collision/ShapeData")]
public class CollisionShapeData : ScriptableObject
{
    public enum ShapeType
    {
        Circle,
        Square,
        Hexagon,
        Custom
    }

    [SerializeField] private ShapeType shapeType = ShapeType.Circle;
    [SerializeField] private int segments = 32;
    [SerializeField] private float edgeSmoothing = 0.1f;
    [SerializeField] private AnimationCurve customShapeCurve;
    
    public ShapeType Type => shapeType;
    public int Segments => segments;
    public float EdgeSmoothing => edgeSmoothing;
    public AnimationCurve CustomCurve => customShapeCurve;
}

// Domain/ShapeMeshGenerator.cs
/// <summary>
/// 各種形状のメッシュを生成するユーティリティクラス
/// </summary>
public static class ShapeMeshGenerator
{
    public static Mesh CreateShapeMesh(CollisionShapeData shapeData, float radius)
    {
        return shapeData.Type switch
        {
            CollisionShapeData.ShapeType.Circle => CreateCircleMesh(radius, shapeData.Segments),
            CollisionShapeData.ShapeType.Square => CreateSquareMesh(radius, shapeData.EdgeSmoothing),
            CollisionShapeData.ShapeType.Hexagon => CreateHexagonMesh(radius, shapeData.EdgeSmoothing),
            CollisionShapeData.ShapeType.Custom => CreateCustomShapeMesh(radius, shapeData.CustomCurve, shapeData.Segments),
            _ => CreateCircleMesh(radius, shapeData.Segments)
        };
    }

    private static Mesh CreateCircleMesh(float radius, int segments)
    {
        throw new NotImplementedException();
    }

    private static Mesh CreateSquareMesh(float radius, float smoothing)
    {
        var mesh = new Mesh();
        var cornerCount = 4;
        var verticesPerCorner = 5;
        var vertices = new Vector3[cornerCount * verticesPerCorner + 1];
        var triangles = new int[cornerCount * verticesPerCorner * 3];

        vertices[0] = Vector3.zero; // Center
        var currentVertex = 1;
        var currentTriangle = 0;

        for (int i = 0; i < cornerCount; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            float nextAngle = (i + 1) * 90f * Mathf.Deg2Rad;
            
            // 各コーナーの頂点を生成
            for (int j = 0; j < verticesPerCorner; j++)
            {
                float t = j / (float)(verticesPerCorner - 1);
                float smoothedAngle = Mathf.Lerp(angle, nextAngle, t);
                vertices[currentVertex + j] = new Vector3(
                    Mathf.Cos(smoothedAngle) * radius,
                    Mathf.Sin(smoothedAngle) * radius,
                    0
                );
            }

            // 三角形を生成
            for (int j = 0; j < verticesPerCorner - 1; j++)
            {
                triangles[currentTriangle++] = 0;
                triangles[currentTriangle++] = currentVertex + j;
                triangles[currentTriangle++] = currentVertex + j + 1;
            }

            currentVertex += verticesPerCorner;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Mesh CreateHexagonMesh(float radius, float smoothing)
    {
        var mesh = new Mesh();
        var cornerCount = 6;
        var verticesPerCorner = 4;
        var vertices = new Vector3[cornerCount * verticesPerCorner + 1];
        var triangles = new int[cornerCount * verticesPerCorner * 3];

        vertices[0] = Vector3.zero;
        var currentVertex = 1;
        var currentTriangle = 0;

        for (int i = 0; i < cornerCount; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            float nextAngle = (i + 1) * 60f * Mathf.Deg2Rad;

            for (int j = 0; j < verticesPerCorner; j++)
            {
                float t = j / (float)(verticesPerCorner - 1);
                float smoothedAngle = Mathf.Lerp(angle, nextAngle, t);
                vertices[currentVertex + j] = new Vector3(
                    Mathf.Cos(smoothedAngle) * radius,
                    Mathf.Sin(smoothedAngle) * radius,
                    0
                );
            }

            for (int j = 0; j < verticesPerCorner - 1; j++)
            {
                triangles[currentTriangle++] = 0;
                triangles[currentTriangle++] = currentVertex + j;
                triangles[currentTriangle++] = currentVertex + j + 1;
            }

            currentVertex += verticesPerCorner;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Mesh CreateCustomShapeMesh(float radius, AnimationCurve curve, int segments)
    {
        var mesh = new Mesh();
        var vertices = new Vector3[segments + 1];
        var triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            float evaluatedRadius = radius * curve.Evaluate(i / (float)segments);
            
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angle) * evaluatedRadius,
                Mathf.Sin(angle) * evaluatedRadius,
                0
            );
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}