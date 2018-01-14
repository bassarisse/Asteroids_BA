using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceTeleport : MonoBehaviour {

	public float AppearOffset = 1f;

	public void Teleport() {

		var camera = Camera.main;
		if (camera == null)
			return;
		
		var area = camera.rect;
		var pos = this.transform.position;

		var posMin = camera.ViewportToWorldPoint (new Vector3 (area.xMin, area.yMin, 0));
		var posMax = camera.ViewportToWorldPoint (new Vector3 (area.xMax, area.yMax, 0));
		var xMin = posMin.x;
		var xMax = posMax.x;
		var yMin = posMin.y;
		var yMax = posMax.y;

		var x = pos.x;
		var y = pos.y;

		if (pos.x < xMin) {
			x = xMax + AppearOffset;
		} else if (pos.x > xMax) {
			x = xMin - AppearOffset;
		}

		if (pos.y < yMin) {
			y = yMax + AppearOffset;
		} else if (pos.y > yMax) {
			y = yMin - AppearOffset;
		}

		this.transform.position = new Vector3 (x, y, pos.z);

	}
}
