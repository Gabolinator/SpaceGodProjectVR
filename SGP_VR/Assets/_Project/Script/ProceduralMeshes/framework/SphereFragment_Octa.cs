using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;

namespace ProceduralMeshes.Generators {

	public struct SphereFragment_Octa : IMeshGenerator {

		struct Rhombus {
			public int id;
			public float3 leftCorner, rightCorner;
		}

		public int AngleLat { get; set; }
		public int AngleLong { get; set; }

		public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

		public int VertexCount => AngleLat * Resolution * Resolution + 2 * Resolution + 7 + (AngleLat != 4 ? 1: 0);

		public int IndexCount => 0;//6 * NumOfSides * Resolution * Resolution + (NumOfSides != 4 ? 12: 0);

		public int JobLength => AngleLat* Resolution + 1;

		public int Resolution { get; set; }

		public void Execute<S> (int i, S streams) where S : struct, IMeshStreams {
			
			Debug.Log("i:" + i);
			
			if (i == 0) {
				ExecutePolesAndSeam(streams);
			}
			else {
				ExecuteRegular(i - 1, streams);
			}

			//if ( NumOfSides !=4) ExecuteBackFaces(i, streams);
		}

		public void ExecuteRegular<S> (int i, S streams) where S : struct, IMeshStreams {
			int u = i / AngleLat;
			
			Debug.Log("u:" + u);
			Rhombus rhombus = GetRhombus(i - 4 * u);
			int vi = Resolution * (Resolution * rhombus.id + u + 2) + 7; // vertex index 
			
			int ti = 2 * Resolution * (Resolution * rhombus.id + u); //triangle index 
			Debug.Log("ti:" + ti);
			bool firstColumn = u == 0;

			int4 quad = int4(
				vi,
				firstColumn ? rhombus.id : vi - Resolution,
				firstColumn ?
					rhombus.id == 0 ? 8 : vi - Resolution * (Resolution + u) :
					vi - Resolution + 1,
				vi + 1
			);

			u += 1;
			
			
			float3 columnBottomDir = rhombus.rightCorner - down();
			float3 columnBottomStart = down() + columnBottomDir * u / Resolution;
			float3 columnBottomEnd =
				rhombus.leftCorner + columnBottomDir * u / Resolution;

			float3 columnTopDir = up() - rhombus.leftCorner;
			float3 columnTopStart =
				rhombus.rightCorner + columnTopDir * ((float)u / Resolution - 1f);
			float3 columnTopEnd = rhombus.leftCorner + columnTopDir * u / Resolution;

			var vertex = new Vertex();
			vertex.normal = vertex.position = normalize(columnBottomStart);
			vertex.tangent.xz = GetTangentXZ(vertex.position);
			vertex.tangent.w = -1f;
			vertex.texCoord0 = GetTextCoord(vertex.position);
			streams.SetVertex(vi, vertex);
		
			vi += 1;

			for (int v = 1; v < Resolution; v++, vi++, ti += 2) {
				if (v <= Resolution - u) {
					vertex.position =
						lerp(columnBottomStart, columnBottomEnd, (float)v / Resolution);
				}
				else {
					vertex.position =
						lerp(columnTopStart, columnTopEnd, (float)v / Resolution);
				}
				//Debug.Log("ti:" + ti);
				vertex.normal = vertex.position = normalize(vertex.position);
				vertex.tangent.xz = GetTangentXZ(vertex.position);
				vertex.texCoord0 = GetTextCoord(vertex.position);
				streams.SetVertex(vi, vertex);
				//streams.SetTriangle(ti + 0, quad.xyz);
				//streams.SetTriangle(ti + 1, quad.xzw);
				Debug.Log("ti:" + ti);
				quad.y = quad.z;
				quad += int4(1, 0, firstColumn && rhombus.id != 0 ? Resolution : 1, 1);
			}

			quad.z = Resolution * Resolution * rhombus.id + Resolution + u + 6;
			quad.w = u < Resolution ? quad.z + 1 : rhombus.id + 4;

			Debug.Log("ti:" + ti);
			//streams.SetTriangle(ti + 0, quad.xyz);
			//streams.SetTriangle(ti + 1, quad.xzw);
			
		}

		public void ExecutePolesAndSeam<S> (S streams) where S : struct, IMeshStreams {
			var vertex = new Vertex();
			vertex.tangent = float4(sqrt(0.5f), 0f, sqrt(0.5f), -1f);
			vertex.texCoord0.x = 0.125f;

			for (int i = 0; i < 4; i++) {
				vertex.position = vertex.normal = down();
				vertex.texCoord0.y = 0f;
				
				streams.SetVertex(i, vertex);
			
				//vertex.position = vertex.normal = up();
				//vertex.texCoord0.y = 1f;
				//streams.SetVertex(i + NumOfSides, vertex);
				//vertex.tangent.xz = float2(-vertex.tangent.z, vertex.tangent.x);
				//vertex.texCoord0.x += 0.25f;
				
			}

			//vertex.tangent.xz = float2(1f, 0f);
			//vertex.texCoord0.x = 0f;

			for (int v = 1; v < 2 * Resolution; v++) {
				if (v < Resolution) {
					vertex.position = lerp(down(), back(), (float)v / Resolution);
				}
				else {
					vertex.position =
						lerp(back(), up(), (float)(v - Resolution) / Resolution);
				}
				vertex.normal = vertex.position = normalize(vertex.position);
				vertex.texCoord0.y = GetTextCoord(vertex.position).y;
				streams.SetVertex(v + 7, vertex);
				

				if (AngleLat != 4)
				{
					vertex.position = float3.zero;
					//vertex.texCoord0.y = GetTextCoord(vertex.position).y;
					//vertex.normal = 
					streams.SetVertex(VertexCount - 1, vertex);
				}
			}
		}


		public void ExecuteBackFaces<S>(int i , S streams) where S : struct, IMeshStreams
		{
			
			//To close de shape the 4 triangle in the "back"
			float3 position = float3.zero;
			var indexCenter = streams.GetVertexIndex(position);

			position = float3(1, 0, 0);
			var indexLeft = streams.GetVertexIndex(position);

			position = float3(-1, 0, 0);
			var indexRight = streams.GetVertexIndex(position);

			position = float3(0, 1, 0);
			var indexUp = streams.GetVertexIndex(position);

			position = float3(0, -1, 0);
			var indexDown = streams.GetVertexIndex(position);

			position = float3(0, 0, -1);
			var indexForward = streams.GetVertexIndex(position);

			position = float3(0, 0, 1);
			var indexBack = streams.GetVertexIndex(position);


			if (i == AngleLat)
			{
				if (AngleLat == 1)
				{

					streams.SetTriangle(2, int3(indexUp, indexCenter, indexLeft));
					streams.SetTriangle(3, int3(indexLeft, indexCenter, indexDown));
					streams.SetTriangle(4, int3(indexForward, indexCenter, indexUp));
					streams.SetTriangle(5, int3(indexDown, indexCenter, indexForward));
				}

				else if (AngleLat == 2)
				{


					streams.SetTriangle(4, int3(indexUp, indexCenter, indexBack));
					streams.SetTriangle(5, int3(indexBack, indexCenter, indexDown));
					streams.SetTriangle(6, int3(indexDown, indexCenter, indexForward));
					streams.SetTriangle(7, int3(indexForward, indexCenter, indexUp));
				}

				else if (AngleLat == 3)
				{
					streams.SetTriangle(6, int3(indexDown, indexCenter, indexForward));
					streams.SetTriangle(7, int3(indexForward, indexCenter, indexUp));
					streams.SetTriangle(8, int3(indexUp, indexCenter, indexRight));
					streams.SetTriangle(9, int3(indexDown, indexRight,indexCenter));
					
				}
			}
		}

		static Rhombus GetRhombus (int id) => id switch {
			0 => new Rhombus {
				id = id,
				leftCorner = back(),
				rightCorner = right()
			},
			1 => new Rhombus {
				id = id,
				leftCorner = right(),
				rightCorner = forward()
			},
			2 => new Rhombus {
				id = id,
				leftCorner = forward(),
				rightCorner = left()
			},
			_ => new Rhombus {
				id = id,
				leftCorner = left(),
				rightCorner = back()
			}
		};

		static float2 GetTangentXZ (float3 p) => normalize(float2(-p.z, p.x));

		static float2 GetTextCoord (float3 p) {
			var texCoord = float2(
				atan2(p.x, p.z) / (-2f * PI) + 0.5f,
				asin(p.y) / PI + 0.5f
			);
			if (texCoord.x < 1e-6f) {
				texCoord.x = 1f;
			}
			return texCoord;
		}
	}
}