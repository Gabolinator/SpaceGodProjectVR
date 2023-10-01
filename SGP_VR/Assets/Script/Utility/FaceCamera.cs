using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
	[SerializeField] protected Camera currentCamera;
	public bool ignoreTilt = false;

	private void Update()
	{
		if (!Camera.main)
			return;


		// Get the direction to the camera from this object
		var cameraDirection = transform.position - Camera.main.transform.position;

		currentCamera = Camera.main;

		// Rotate the object to face the camera
		transform.rotation = Quaternion.LookRotation(cameraDirection);

		if (ignoreTilt) transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
	}
}

