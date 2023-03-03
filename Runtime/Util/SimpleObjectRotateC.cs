using System;
using UnityEngine;

public class SimpleObjectRotateC : MonoBehaviour {

	public float rotationSpeed = 100f;
	public Vector3 direction = new(0, 0, 1);

	Transform _transform;

	void Awake()
	{
		_transform = transform;
	}

	// Update is called once per frame
	void FixedUpdate () {
		_transform.Rotate(direction * (rotationSpeed * Time.deltaTime));
	}
}
