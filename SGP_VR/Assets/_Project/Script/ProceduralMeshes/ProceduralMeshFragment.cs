using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMeshFragment : MonoBehaviour
{
    private void OnEnable()
    {
        bool renderBack = false;

        var mesh = new Mesh { name = "new Procedural Mesh" };

        var meshFilter = GetComponent<MeshFilter>();

        /*generate mesh component and assign them to the mesh filter*/
        var vertices = GenerateVertices();
        var triangles = GenerateTriangles(vertices, renderBack); //new int[] {0,2,1, 1,2,3};
        var normals = GenerateNormals(vertices, renderBack);
        var uv = GenerateUVCoordinates(vertices);
        var tangents = GenerateTangents(vertices);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.tangents = tangents;

        meshFilter.mesh = mesh;
    }

    private Vector3[] GenerateVertices()
    {
        return new Vector3[] { Vector3.zero, Vector3.right, Vector3.up,  new Vector3(1f, 1f) };
    }

    private int[] GenerateTriangles(Vector3[] vertices, bool renderBack = false)
    {
        
        var numTriangle = vertices.Length -2;//if shared vertices
        var indices = new int[numTriangle * 3];

        /**/
        if (!renderBack)
        {
            //for (int k = 0; k < numTriangle; k++)
            //{
            //    if (k == 0)
            //    {
            //        for (int i = 0; i < numTriangle; i++)
            //        {
            //            indices[k + i] = i;
            //        }
            //    }
            //    else
            //    {
            //        int j = ;
            //        for (int i = numTriangle-1; i > 0; i--)
            //        {
            //            indices[k + i] = i;
            //            j++;
            //        }

            //    }


            //}
        }

        else
        {
            int j = 0; // index 
            for (int k = 1; k <= numTriangle; k++)
            {
                

                if (k%2 != 0) // all odd number triangle
                {
                    indices[0] = 0;
                   
                    for (int i = 3; i > 0; i--)
                    {
                        indices[j] = i;
                        j++;
                    }
                }
                else
                {
                    for (int i = 1; i < numTriangle; i++)
                    {
                        indices[k + i] = i;
                    }

                }
            }
        }

        Debug.Log("indices : [ ");
        foreach (int i in indices)
        { Debug.Log(indices[i] + " , "); }
        Debug.Log(" ]");

        return indices;

    }

    private Vector3[] GenerateNormals(Vector3[] vertices, bool renderBack = false)
    {
        var normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = renderBack ? Vector3.back : Vector3.forward;
        }
        return normals;
    }

    private Vector2[] GenerateUVCoordinates(Vector3[] vertices)
    {

        return new Vector2[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
    }

    private Vector4[] GenerateTangents(Vector3[] vertices)
    {
        var tangents = new Vector4[vertices.Length];
        for (int i = 0; i < tangents.Length; i++)
        {
            tangents[i] = new Vector4(1f, 0f, 0f, -1f);
        }
        return tangents;
    }
}
