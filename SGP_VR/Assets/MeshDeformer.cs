using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour {

	public float springForce = 20f;
	public float damping = 5f;

	Mesh deformingMesh;
	Vector3[] originalVertices, displacedVertices;
	Vector3[] vertexVelocities;

	float uniformScale = 1f;
	public bool coroutineActive = false; 

	void Start ()
	{
		Setup();
	}



	void UpdateVertex (int i, float time, float springForce, float damping ) {
		Vector3 velocity = vertexVelocities[i];
		Vector3 displacement = displacedVertices[i] - originalVertices[i];
		displacement *= uniformScale;
		velocity -= displacement * (springForce * time);
		velocity *= 1f - damping * time;
		vertexVelocities[i] = velocity;
		displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
	}

	// private void Update()
	// {
	// 	uniformScale = transform.localScale.x;
	// 	// for (int i = 0; i < displacedVertices.Length; i++) {
	// 	// 	UpdateVertex(i);
	// 	// }
	// 	deformingMesh.vertices = displacedVertices;
	// 	deformingMesh.RecalculateNormals();
	// }

	public IEnumerator SpringBackToOriginalPositionCoroutine(float overTime, float freq)
	{

		while (coroutineActive)
		{
			yield return new WaitForSeconds(freq/2);
		}
		
		float t = 0;

		while (t<overTime)
		{
			coroutineActive = true;
			SpringBackToOriginalPositionInternal(Time.deltaTime, springForce, damping);
			t += freq;
			yield return new WaitForSeconds(freq);
		}

		coroutineActive = false;
	}

	void SpringBackToOriginalPosition()
	{
		uniformScale = transform.localScale.x;
		for (int i = 0; i < displacedVertices.Length; i++) {
			UpdateVertex(i);
		}
		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();
	}
	void SpringBackToOriginalPositionInternal(float time, float springForce, float damping)
	{
		
		for (int i = 0; i < displacedVertices.Length; i++) {
			UpdateVertex(i, time, springForce, damping);
		}
		
		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();
	}

	void UpdateVertex (int i)
	{
		UpdateVertex(i, Time.deltaTime, springForce, damping);
	}

	public void AddDeformingForce (Vector3 point, float force) {
		if(coroutineActive) return;
		
		point = transform.InverseTransformPoint(point);
		for (int i = 0; i < displacedVertices.Length; i++) {
			AddForceToVertex(i, point, force, Time.deltaTime);
		}
	}

	void AddForceToVertex (int i, Vector3 point, float force, float time) {
		Vector3 pointToVertex = displacedVertices[i] - point;
		pointToVertex *= uniformScale;
		float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
		float velocity = attenuatedForce * time;
		vertexVelocities[i] += pointToVertex.normalized * velocity;
	}
	
	void AddDeformingForceInternal (float time, Vector3 point, float force) {

		point = transform.InverseTransformPoint(point);
		for (int i = 0; i < displacedVertices.Length; i++) {
			AddForceToVertex(i, point, force, time);
			UpdateVertex(i);
			vertexVelocities[i] = Vector3.zero;
		}
		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();
		
	}
	
	public IEnumerator AddDeformingForceCoroutine(float overTime, float freq, Vector3 point, float force)
	{
		if (coroutineActive) yield return null;
		float t = 0;

		coroutineActive = true;
		while (t<overTime)
		{
			
			AddDeformingForceInternal(Time.deltaTime, point, force);
			t += freq;
			yield return new WaitForSeconds(freq);
		}

		coroutineActive = false;
	}

	public void Setup()
	{
		deformingMesh = GetComponent<MeshFilter>().mesh;
		originalVertices = deformingMesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices[i] = originalVertices[i];
		}
		vertexVelocities = new Vector3[originalVertices.Length];
	}

	Mesh[] CreateFracturedMeshes(Mesh originalMesh)
	{
		// Split the original mesh into two separate meshes
		// For simplicity, we are just splitting along the X-axis
		Vector3[] vertices = originalMesh.vertices;
		int halfVerticesCount = vertices.Length / 2;

		// Create two new meshes
		Mesh mesh1 = new Mesh();
		Mesh mesh2 = new Mesh();

		// Assign vertices to each mesh
		mesh1.vertices = vertices.Take(halfVerticesCount).ToArray();
		mesh2.vertices = vertices.Skip(halfVerticesCount).ToArray();

		// Copy other mesh data
		mesh1.triangles = originalMesh.triangles.Take(originalMesh.triangles.Length/2).ToArray();
		mesh2.triangles = originalMesh.triangles.Skip(originalMesh.triangles.Length/2).ToArray();

		// Create Normals and UVs
		mesh1.RecalculateNormals();
		mesh1.RecalculateBounds();
		mesh2.RecalculateNormals();
		mesh2.RecalculateBounds();

		return new Mesh[] { mesh1, mesh2 };
	}

	public void CreateFracturedObjects()
	{
		var meshes = CreateFracturedMeshes(deformingMesh);
		CreateFracturedObjects(meshes);
	}

	void CreateFracturedObjects(Mesh[] fracturedMeshes)
	{
		// Create game objects with MeshFilter and MeshRenderer components for the fractured parts
		for (int i = 0; i < fracturedMeshes.Length; i++)
		{
			GameObject fracturedObject = new GameObject("Fractured Part " + (i + 1));
			fracturedObject.transform.position = transform.position;
			fracturedObject.transform.rotation = transform.rotation;

			MeshFilter meshFilter = fracturedObject.AddComponent<MeshFilter>();
			meshFilter.mesh = fracturedMeshes[i];

			MeshRenderer meshRenderer = fracturedObject.AddComponent<MeshRenderer>();
			//meshRenderer.material = fracturedMaterial; // Assign the material for the fractured parts
		}
	}

}