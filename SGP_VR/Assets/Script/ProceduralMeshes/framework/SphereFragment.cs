using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{

    public struct SphereFragment : IMeshGenerator
    {
        public float Angle { get; set; }

        public int Sides => 3;

        struct Side
        {
            public int id;
            public float3 uvOrigin, uVector, vVector;
            public float3 normal;
            public float4 tangent;
        }

        static Side GetSide(int id) => id switch
        { 
            0 => new Side 
            {
            id = id,
            uvOrigin = -1f,
            uVector = 2f * right(),
            vVector = 2f * up(),
            normal = back(),
            tangent = float4(1f, 0f, 0f, -1f)
            },

            1 => new Side
            {
                id = id,
                uvOrigin = float3(1f, -1f, -1f),
                uVector = 2f * forward(),
                vVector = 2f * up(),
                normal = right(),
                tangent = float4(0f, 0f, 1f, -1f)
            },

            _ => new Side
            {
                id = id,
                uvOrigin = float3(-1f, -1f, 1f),
                uVector = 2f * back(),
                vVector = 2f * up(),
                normal = left(),
                tangent = float4(0f, 1f, 0f, -1f)
            },
        };

        public int Resolution { get; set; }

        public int VertexCount => Sides*4 * Resolution * Resolution;

        public int IndexCount => Sides * 6 * Resolution * Resolution;

        public int JobLength => Sides * Resolution;

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f));

        public void Execute<S>(int i, S streams) where S : struct, IMeshStreams
        {
          
            int u = i / Sides;
            Side side = GetSide(i - Sides * u);
            Debug.Log("Side : " + side.id);

            int vi = 4 * Resolution * (Resolution * side.id + u);
            int ti = 2 * Resolution * (Resolution * side.id + u);

            float3 uA = side.uvOrigin + side.uVector * u / Resolution;
            float3 uB = side.uvOrigin + side.uVector * (u + 1) / Resolution;

            for (int v = 0; v < Resolution; v++, vi += 4, ti += 2)
            {

                    float3 pA = uA + side.vVector * v / Resolution;
                    float3 pB = uB + side.vVector * v / Resolution;
                    float3 pC = uA + side.vVector * (v + 1) / Resolution;
                    float3 pD = uB + side.vVector * (v + 1) / Resolution;

                if (side.id == 1)
                {


                    pB -= vi / Resolution * new float3(cos(PI / 4), 0, 0);

                    pD -= vi / Resolution * new float3(cos(PI / 4), 0, 0);
                }



                else if (side.id == 2)
                {

                    pA += new float3(cos(PI / 4), 0, 0);

                    pC += new float3(cos(PI / 4), 0, 0);
                }


                    var vertex = new Vertex();

                    vertex.normal = side.normal;
                    vertex.tangent = side.tangent;

                    vertex.position = pA;
                    streams.SetVertex(vi + 0, vertex);

                    vertex.position = pB;
                    vertex.texCoord0 = float2(1f, 0f);
                    streams.SetVertex(vi + 1, vertex);

                    vertex.position = pC;
                    vertex.texCoord0 = float2(0f, 1f);
                    streams.SetVertex(vi + 2, vertex);

                    vertex.position = pD;
                    vertex.texCoord0 = 1f;
                    streams.SetVertex(vi + 3, vertex);

                    streams.SetTriangle(ti + 0, vi + int3(0, 2, 1));
                    streams.SetTriangle(ti + 1, vi + int3(1, 2, 3));
                
            }
        }
    }
}

