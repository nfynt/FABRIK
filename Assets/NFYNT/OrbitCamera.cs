﻿using UnityEngine;
using System.Collections;

[AddComponentMenu("Nfynt/OrbitCamera")]
public class OrbitCamera : MonoBehaviour
{
	public TaskOneController taskController;
	public Transform target;
	public float distance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;

	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;

	public float distanceMin = .5f;
	public float distanceMax = 15f;

	float x = 0.0f;
	float y = 0.0f;

	
	void Start()
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
	}

	void LateUpdate()
	{
		if (Input.GetMouseButton(1) && target && taskController.currMode != ViewMode.GraphPlot)
		{
			x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

			y = ClampAngle(y, yMinLimit, yMaxLimit);

			Quaternion rotation = Quaternion.Euler(y, x, 0);
			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
			Vector3 position = transform.rotation * negDistance + target.position;

			transform.position = position;
			transform.rotation = rotation;
		}

		if(target && Input.GetAxis("Mouse ScrollWheel") != 0 && taskController.currMode!=ViewMode.GraphPlot)
		{
			distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
			Vector3 position = transform.rotation * negDistance + target.position;

			transform.position = position;
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}
}