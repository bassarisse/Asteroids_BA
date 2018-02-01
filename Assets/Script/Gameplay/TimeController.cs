using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour {

	float _originalTimeScale;
	float _currentTimeScale;

	float _maxSlowMotionTime;
	float _currentSlowMotionTime;

	void Start () {
		_originalTimeScale = Time.timeScale;
	}
	
	void FixedUpdate () {
		
		if (_currentSlowMotionTime > 0f) {
			_currentSlowMotionTime -= Time.deltaTime;
			Time.timeScale = _originalTimeScale * 0.1f + _originalTimeScale * (1f - _currentSlowMotionTime / _maxSlowMotionTime) * 0.9f;
		} else {
			Time.timeScale = _originalTimeScale;
		}


	}

	public void SlowMotion(float time) {

		_maxSlowMotionTime = time;
		_currentSlowMotionTime = time;
		
	}

	void OnDestroy() {
		Time.timeScale = _originalTimeScale;
	}

}
