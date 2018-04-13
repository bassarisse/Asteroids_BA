using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityExtensions {
	
	static public Rect CreateRectFromCamera(Camera camera, float multiplier = 1.0f) {

			var center = camera.transform.position;
			var height = camera.orthographicSize * 2f * multiplier;
			var width = height * camera.aspect;

			return new Rect(center.x - width / 2f, center.y - height / 2f, width, height);
	}

	static public float ClampAngle(float angle) {
		var newAngle = angle;

		while (newAngle > 180)
			newAngle -= 360f;
		while (newAngle < -180)
			newAngle += 360f;

		return newAngle;
	}
	
}
