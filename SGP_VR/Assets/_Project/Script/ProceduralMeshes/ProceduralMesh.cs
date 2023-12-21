using ProceduralMeshes;
using ProceduralMeshes.Generators;
using ProceduralMeshes.Streams;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour 
{
    Mesh mesh;

    [SerializeField, Range(1, 50)]
    int resolution = 1;

    [SerializeField, Range(1, 360)]
    int angleLat = 1;

    [SerializeField, Range(1, 360)]
    int angleLong = 1;
    
    [SerializeField]
    MeshType meshType;

    public enum MaterialMode { Flat, Ripple, LatLonMap, CubeMap }

    [SerializeField]
    MaterialMode material;

    [SerializeField]
    Material[] materials;

    [System.Flags]
    public enum GizmoMode { Nothing = 0, Vertices = 1, Normals = 0b10, Tangents = 0b100, Triangles = 0b1000 }

    [SerializeField]
    GizmoMode gizmos;

    [System.NonSerialized]
    Vector3[] vertices, normals;

    [System.NonSerialized]
    Vector4[] tangents;

    [System.NonSerialized]
    int[] triangles;

    
    RandomShapeGenerator shapeGenerator = new RandomShapeGenerator();
    
    
    [System.Flags]
    public enum MeshOptimizationMode
    {
        Nothing = 0, ReorderIndices = 1, ReorderVertices = 0b10
    }

    [SerializeField]
    MeshOptimizationMode meshOptimization;

    static MeshJobScheduleDelegate[] jobs =
   {
        MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
        MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
        MeshJob<SharedTriangleGrid, SingleStream>.ScheduleParallel,
        MeshJob<PointyHexagonGrid, SingleStream>.ScheduleParallel,
        MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel,
        MeshJob<UVSphere, SingleStream>.ScheduleParallel,
        //MeshJob<CubeSphere, SingleStream>.ScheduleParallel,
        MeshJob<SharedCubeSphere, PositionStream>.ScheduleParallel,
        MeshJob<Octasphere, SingleStream>.ScheduleParallel,
        MeshJob<SphereFragment_UV, SingleStream>.ScheduleParallel
    };

    public enum MeshType
    {
        SquareGrid,
        SharedSquareGrid,
        SharedTriangleGrid,
        PointyHexagonGrid,
        FlatHexagonGrid,
        UVSphere,
        //CubeSphere,
        SharedCubeSphere,
        
        Octasphere,
        SphereFragment, 
        RandomFragmentTest
    };

    void GenerateMesh() 
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        jobs[(int)meshType](mesh, meshData, resolution, angleLat,angleLong ,default).Complete();

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);

        if (meshOptimization == MeshOptimizationMode.ReorderIndices)
        {
            mesh.OptimizeIndexBuffers();
        }
        else if (meshOptimization == MeshOptimizationMode.ReorderVertices)
        {
            mesh.OptimizeReorderVertexBuffer();
        }
        else if (meshOptimization != MeshOptimizationMode.Nothing)
        {
            mesh.Optimize();
        }
    }

    void GenerateMesh(int minTriangles, int maxTriangles, float size)
    {
        Mesh randomShapeMesh = shapeGenerator.GenerateRandomShape(minTriangles, maxTriangles, size); // Vary min and max triangles and size

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = randomShapeMesh;
       
    }

    void OnDrawGizmos()
    {
        if (gizmos == GizmoMode.Nothing || mesh == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Transform t = transform;

        bool drawVertices = (gizmos & GizmoMode.Vertices) != 0;
        bool drawNormals = (gizmos & GizmoMode.Normals) != 0;
        bool drawTangents = (gizmos & GizmoMode.Tangents) != 0;
        bool drawTriangles = (gizmos & GizmoMode.Triangles) != 0;


        if (vertices == null)
        {
            vertices = mesh.vertices;
        }
        if (drawNormals && normals == null)
        {
            drawNormals = mesh.HasVertexAttribute(VertexAttribute.Normal);
            if (drawNormals)
            {
                normals = mesh.normals;
            }
        }
        if (drawTangents && tangents == null)
        {
            drawTangents = mesh.HasVertexAttribute(VertexAttribute.Tangent);
            if (drawTangents)
            {
                tangents = mesh.tangents;
            }
        }
        if (drawTriangles && triangles == null)
        {
            triangles = mesh.triangles;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 position = t.TransformPoint(vertices[i]);
            if (drawVertices)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(position, 0.02f);
            }
            if (drawNormals)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(position, t.TransformDirection(normals[i]) * 0.2f);
            }
            if (drawTangents)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(position, t.TransformDirection(tangents[i]) * 0.2f);
            }

        }
        if (drawTriangles)
        {
           
            float colorStep = 1f / (triangles.Length - 3);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                float c = i * colorStep;
                Gizmos.color = new Color(c, 0f, c);
                Gizmos.DrawSphere(
                    t.TransformPoint((
                        vertices[triangles[i]] +
                        vertices[triangles[i + 1]] +
                        vertices[triangles[i + 2]]
                    ) * (1f / 3f)),
                    0.02f
                );
            }
        }
    }

    void Awake()
    {
        mesh = new Mesh { name = "Procedural Mesh" };

        GetComponent<MeshFilter>().mesh = mesh;
    }


    void OnValidate() => enabled = true;

    void Update()
    {
        if (meshType == MeshType.RandomFragmentTest) GenerateMesh(5, 10 , 1);
    
        
        else GenerateMesh();
    

    enabled = false;

        vertices = null;
        normals = null;
        tangents = null;
        triangles = null;

        GetComponent<MeshRenderer>().material = materials[(int)material];

        UpdateOtherScripts();
    }

    private void UpdateOtherScripts()
    {
        var meshDeformer =GetComponent<MeshDeformer>();
        if (meshDeformer)
        {
            meshDeformer.Setup();
        }
        var collider =GetComponent<MeshCollider>();
        if (collider)
        {
            collider.sharedMesh = mesh;
        }
        
    }
}

public class RandomShapeGenerator
{
    public Mesh GenerateRandomShape(int minTriangles, int maxTriangles, float size)
    {
        Mesh mesh = new Mesh();

        int triangleCount = Random.Range(minTriangles, maxTriangles); // Randomize number of triangles
        int vertexCount = triangleCount * 3;

        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount * 3];
        Vector2[] uv = new Vector2[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            // Generate random vertices within a cube area
            vertices[i] = new Vector3(
                Random.Range(-size, size),
                Random.Range(-size, size),
                Random.Range(-size, size)
            );
            uv[i] = new Vector2(Random.value, Random.value);
        }

        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
    
    

