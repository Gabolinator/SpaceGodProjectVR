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

	// public void AddDeformingForce (Vector3 point, float force) {
	// 	if(coroutineActive) return;
	// 	
	// 	point = transform.InverseTransformPoint(point);
	// 	for (int i = 0; i < displacedVertices.Length; i++) {
	// 		AddForceToVertex(i, point, force, Time.deltaTime);
	// 	}
	// }

	void AddForceToVertex (int i, Vector3 point, float force, float time) {
		Vector3 pointToVertex = displacedVertices[i] - point;
		pointToVertex *= uniformScale;
		float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
		float velocity = attenuatedForce * time;
		vertexVelocities[i] += pointToVertex.normalized * velocity;
	}
	
	public void  AddDeformingForceInternal (float time, Vector3 point, float force) {

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

	public void AddDeformingForce(Vector3 point, float force)
	{
		float overtime = 2;
		float freq = .1f;
		StartCoroutine(AddDeformingForceCoroutine(overtime, freq, point, force));
	}



}