using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceTeleport : MonoBehaviour {

	public float AppearOffset = 1f;

	public void Teleport() {

		var camera = Camera.main;
		var area = camera.rect;
		var pos = this.transform.position;

		var posMin = camera.ViewportToWorldPoint (new Vector3 (area.xMin, area.yMin, 0));
		var posMax = camera.ViewportToWorldPoint (new Vector3 (area.xMax, area.yMax, 0));
		var xMin = posMin.x;
		var xMax = posMax.x;
		var yMin = posMin.y;
		var yMax = posMax.y;

		if (pos.x < xMin) {
			this.transform.position = new Vector3 (xMax + AppearOffset, pos.y, pos.z);
		} else if (pos.x > xMax) {
			this.transform.position = new Vector3 (xMin - AppearOffset, pos.y, pos.z);
		}

		pos = this.transform.position;

		if (pos.y < yMin) {
			this.transform.position = new Vector3 (pos.x, yMax + AppearOffset, pos.z);
		} else if (pos.y > yMax) {
			this.transform.position = new Vector3 (pos.x, yMin - AppearOffset, pos.z);
		}

	}
}
