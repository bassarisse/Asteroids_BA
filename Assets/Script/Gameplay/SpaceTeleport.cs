using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpaceTeleport : MonoBehaviour {

	public Renderer TargetRenderer;
	public float AppearOffset = 1f;
	public float InitialToleranceTime = 3f;
	public float ToleranceTime = 1f;
	public UnityEvent OnTeleport;

	Camera _camera;
	bool _isVisible;
	float _teleportTime;

	void Awake() {
		if (OnTeleport == null)
			OnTeleport = new UnityEvent ();
	}

	void OnEnable() {
		_camera = Camera.main;
		_isVisible = true;
		_teleportTime = InitialToleranceTime;
	}

	void FixedUpdate() {
		if (TargetRenderer == null)
			return;
		
		if (_teleportTime > 0f)
			_teleportTime -= Time.fixedDeltaTime;
		
		if (_isVisible && !TargetRenderer.isVisible && _teleportTime <= 0f) {
			Teleport ();
			_isVisible = false;
			_teleportTime = ToleranceTime;
		}

		if (TargetRenderer.isVisible) {
			_isVisible = true;
			_teleportTime = 0f;
		}

	}

	public void Teleport() {

		if (_camera == null)
			return;
		
		var area = _camera.rect;
		var pos = this.transform.position;

		var posMin = _camera.ViewportToWorldPoint (new Vector3 (area.xMin, area.yMin, 0));
		var posMax = _camera.ViewportToWorldPoint (new Vector3 (area.xMax, area.yMax, 0));
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

		OnTeleport.Invoke ();

	}
}
