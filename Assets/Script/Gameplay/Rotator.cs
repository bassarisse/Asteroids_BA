using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

	public float XRotation = 0f;
	public float YRotation = 0f;
	public float ZRotation = 0f;

	void Update () {

		transform.Rotate (XRotation * Time.deltaTime, YRotation * Time.deltaTime, ZRotation * Time.deltaTime);
		
	}
}
