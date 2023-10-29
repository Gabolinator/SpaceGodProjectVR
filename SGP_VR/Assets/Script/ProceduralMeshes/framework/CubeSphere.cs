using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{

    public struct CubeSphere : IMeshGenerator
    {

        struct Side
        {
            public float3 uvOrigin, uVector, vVector;
            public float3 normal;
            public float4 tangent;
        }



        public int Resolution { get; set; }

        public int VertexCount => 4 * Resolution * Resolution;

        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution;

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f));

        public void Execute<S>(int u, S streams) where S : struct, IMeshStreams
        {
            var side = new Side
            {
                uvOrigin = -1f,
                uVector = 2f * right(),
                vVector = 2f * up(),
                normal = back(),
                tangent = float4(1f, 0f, 0f, -1f)
            };

            int vi = 4 * Resolution * u, ti = 2 * Resolution * u;

            float3 uA = side.uvOrigin + side.uVector * u / Resolution;
            float3 uB = side.uvOrigin + side.uVector * (u + 1) / Resolution;

            for (int v = 0; v < Resolution; v++, vi += 4, ti += 2)
            {

                float3 pA = uA + side.vVector * v / Resolution;
                float3 pB = uB + side.vVector * v / Resolution;
                float3 pC = uA + side.vVector * (v + 1) / Resolution;
                float3 pD = uB + side.vVector * (v + 1) / Resolution;

                //var xCoordinates = float2(v, v + 1f) / Resolution - 0.5f;
                //var zCoordinates = float2(u, u + 1f) / Resolution - 0.5f;

                var vertex = new Vertex();
                //vertex.normal.y = 1f;
                //vertex.tangent.xw = float2€(1f, -1f);
                vertex.normal = side.normal;
                vertex.tangent = side.tangent;

                //vertex.position.x = xCoordinates.x;
                //vertex.position.z = zCoordinates.x;
                vertex.position = pA;
                streams.SetVertex(vi + 0, vertex);

                //vertex.position.x = xCoordinates.y;
                vertex.position = pB;
                vertex.texCoord0 = float2(1f, 0f);
                streams.SetVertex(vi + 1, vertex);

                //vertex.position.x = xCoordinates.x;
                //vertex.position.z = zCoordinates.y;
                vertex.position = pC;
                vertex.texCoord0 = float2(0f, 1f);
                streams.SetVertex(vi + 2, vertex);

                //vertex.position.x = xCoordinates.y;
                vertex.position = pD;
                vertex.texCoord0 = 1f;
                streams.SetVertex(vi + 3, vertex);


                streams.SetTriangle(ti + 0, vi + int3(0, 2, 1));
                streams.SetTriangle(ti + 1, vi + int3(1, 2, 3));
            }
        }
    }
}