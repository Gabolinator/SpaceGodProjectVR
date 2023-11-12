using UnityEngine;
using UnityEngine.InputSystem;

public class MeshDeformerInput : MonoBehaviour {
	
	public float force = 10f;
	public float forceOffset = 0.1f;
	public MeshDeformer meshDeformer;
	
	void Update () {
		if (Mouse.current.leftButton.isPressed) {
			HandleInput();
		}
		// else
		// {
		// 	HandleReleaseInput();
		// }
	}

	void HandleInput () {
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		Ray inputRay = Camera.main.ScreenPointToRay(mousePosition);
		
		RaycastHit hit;
		
		if (Physics.Raycast(inputRay, out hit)) {
			meshDeformer = hit.collider.GetComponent<MeshDeformer>();
			if (meshDeformer) {
				Vector3 point = hit.point;
				point += hit.normal * forceOffset;
				StartCoroutine(meshDeformer.AddDeformingForceCoroutine(.5f, .05f, point, force));
				
			}
		}
	}

	void HandleReleaseInput()
	{
		if(!meshDeformer) return;
	
		StartCoroutine(meshDeformer.SpringBackToOriginalPositionCoroutine(3, .1f));
		meshDeformer = null;
	}
}