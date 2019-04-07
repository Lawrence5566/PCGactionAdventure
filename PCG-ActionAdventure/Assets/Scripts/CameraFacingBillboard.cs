using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{
	public Camera referenceCamera;

	void  Awake ()
	{
		// if no camera referenced, grab the main camera
		referenceCamera = GetComponentInParent<Camera>();
		if (!referenceCamera)
			referenceCamera = Camera.main; 
	}

	void Update()
	{
		if (referenceCamera) {
			transform.LookAt (transform.position + referenceCamera.transform.rotation * Vector3.forward,
				referenceCamera.transform.rotation * Vector3.up);
		}
	}


}