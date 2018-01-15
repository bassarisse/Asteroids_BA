using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HyperspaceBehavior : MonoBehaviour {

	public GameObject Ship;
	public float Margin = 1f;
	public float TransitionTime = 1f;

	Camera _camera;
	float _hiddenTime = 0;

	void OnEnable() {
		_camera = Camera.main;
	}

	void Update () {

		if (Ship == null)
			return;

		if (InputExtensions.Pressed.B && _hiddenTime <= 0f) {
			this.Hyperspace ();
		}

	}

	void FixedUpdate() {

		if (_hiddenTime > 0f) {
			_hiddenTime -= Time.fixedDeltaTime;
			if (_hiddenTime <= 0f)
				Ship.SetActive (true);
		}

	}

	void Hyperspace() {
		if (_camera == null)
			return;

		Ship.SetActive (false);
		_hiddenTime = TransitionTime;

		var pos = Ship.transform.position;
		var center = _camera.transform.position;
		var height = _camera.orthographicSize * 2.0f;
		var width = height * _camera.aspect;

		var position = new Vector3(
			center.x + (Random.value - 0.5f) * (width - Margin * 2),
			center.y + (Random.value - 0.5f) * (height - Margin * 2),
			pos.z
		);

		Ship.transform.position = position;

	}

}
