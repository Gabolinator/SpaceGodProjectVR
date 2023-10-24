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

   

    [SerializeField]
    MeshType meshType;

    [System.Flags]
    public enum GizmoMode { Nothing = 0, Vertices = 1, Normals = 0b10, Tangents = 0b100 }

    [SerializeField]
    GizmoMode gizmos;

    Vector3[] vertices, normals;
    Vector4[] tangents;

    static MeshJobScheduleDelegate[] jobs =
   {
        MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
        MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
        MeshJob<SharedTriangleGrid, SingleStream>.ScheduleParallel,
        MeshJob<PointyHexagonGrid, SingleStream>.ScheduleParallel,
        MeshJob<FlatHexagonGrid, SingleStream>.ScheduleParallel,
        MeshJob<UVSphere, SingleStream>.ScheduleParallel
    };

    public enum MeshType
    {
        SquareGrid,
        SharedSquareGrid,
        SharedTriangleGrid,
        PointyHexagonGrid,
        FlatHexagonGrid,
        UVSphere
    };

    void GenerateMesh() 
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        jobs[(int)meshType](mesh, meshData, resolution, default).Complete();

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
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


        if (vertices == null)
        {
            vertices = mesh.vertices;
        }
        if (drawNormals && normals == null)
        {
            normals = mesh.normals;
        }
        if (drawTangents && tangents == null)
        {
            tangents = mesh.tangents;
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
    }

    void Awake()
    {
        mesh = new Mesh { name = "Procedural Mesh" };

        GetComponent<MeshFilter>().mesh = mesh;
    }


    void OnValidate() => enabled = true;

    void Update()
    {
        GenerateMesh();
        enabled = false;

        vertices = null;
        normals = null;
        tangents = null;
    }

}
