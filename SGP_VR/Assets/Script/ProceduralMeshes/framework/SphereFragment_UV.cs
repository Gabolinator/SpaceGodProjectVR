using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators {

    public struct SphereFragment_UV : IMeshGenerator
    {

        public int Resolution { get; set; }
        public int AngleLat { get; set; }
        
        public int AngleLong { get; set; }
        int ResolutionV => 2 * Resolution; //resolution longitude
        int ResolutionU => 4 * Resolution; //resolution latitude
        
        private int NumLong //number of long to show 
        {

            get
            {
                return ResolutionV; //not working 
                
                // float fraction = (float)AngleLong / 360 * (float)ResolutionV;
                // int num = (int)fraction == 0
                //     ? 1
                //     : (int)fraction;
                // Debug.Log("Num long : " + (int)fraction +" / " + num);
                //
                //
                // return (int)fraction == 0
                //     ? 1
                //     : (int)fraction;
            }
        }
        private int NumLat//number of latitute to show 
        {

            get
            {   
                float fraction = (float)AngleLat / 360 * (float)ResolutionU;
                int num = (int)fraction == 0
                    ? 1
                    : (int)fraction;
                Debug.Log("Num lat : " + (int)fraction +" / " + num);
                return (int)fraction == 0
                    ? 1
                    : (int)fraction; 
            }
        }

        private int NumPoles => AngleLong == 360 ? 1 : 0;

        public int VertexCount => (NumLat + 1) * (ResolutionV+  NumPoles) - 2;

        public int IndexCount => 6 * NumLat * (ResolutionV- 1);

        public int JobLength => NumLat + 1;

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

        public void Execute<S>(int u, S streams) where S : struct, IMeshStreams
        {
            if (u == 0)
            {
                ExecuteSeam(streams);
            }
            else
            {
                ExecuteRegular(u, streams);
            }
        }

        public void ExecuteRegular<S>(int u, S streams) where S : struct, IMeshStreams
        {
            int vi = (ResolutionV + 1) * u - 2, 
                ti = 2 * (ResolutionV - 1) * (u - 1);

            /*Poles*/
            
            var vertex = new Vertex();
            
            vertex.position.y = vertex.normal.y = -1f;
            sincos(2f * PI * (u - 0.5f) / ResolutionU,out vertex.tangent.z, out vertex.tangent.x);
            vertex.tangent.w = -1f;
            vertex.texCoord0.x = (u - 0.5f) / ResolutionU;
            streams.SetVertex(vi, vertex);


                vertex.position.y = vertex.normal.y = 1f;
                vertex.texCoord0.y = 1f;
                streams.SetVertex(vi + ResolutionV, vertex);
            
            vi += 1;

            float2 circle;
            sincos(2f * PI * u / ResolutionU, out circle.x, out circle.y);
            vertex.tangent.xz = circle.yx;
            circle.y = -circle.y;
            vertex.texCoord0.x = (float)u / ResolutionU;

            int shiftLeft = (u == 1 ? 0 : -1) - ResolutionV;

            streams.SetTriangle(ti, vi + int3(-1, shiftLeft, 0));
            ti += 1;


            for (int v = 1; v < NumLong; v++, vi++)
            {
                sincos(PI + PI * v / ResolutionV,out float circleRadius, out vertex.position.y);
                vertex.position.xz = circle * -circleRadius;
                vertex.normal = vertex.position;
                vertex.texCoord0.y = (float)v / ResolutionV;
                streams.SetVertex(vi, vertex);

                if (v > 1)
                {
                    streams.SetTriangle(ti + 0, vi + int3(shiftLeft - 1, shiftLeft, -1));
                    streams.SetTriangle(ti + 1, vi + int3(-1, shiftLeft, 0));
                    ti += 2;
                }
            }

            streams.SetTriangle(ti, vi + int3(shiftLeft - 1, 0, -1));
        }


        public void ExecuteSeam<S>(S streams) where S : struct, IMeshStreams
        {
           
            var vertex = new Vertex();
            
            vertex.tangent.x = 1f;
            vertex.tangent.w = -1f;


            for (int v = 1; v < NumLong; v++)
            {
                sincos(PI + PI * v / ResolutionV,out vertex.position.z, out vertex.position.y);
                vertex.normal = vertex.position;
                vertex.texCoord0.y = (float)v / ResolutionV;
                streams.SetVertex(v-1, vertex);
            }
        }
    }
}