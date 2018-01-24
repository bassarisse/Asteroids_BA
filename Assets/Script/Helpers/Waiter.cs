using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waiter : MonoBehaviour {

	protected WaiterModule _waiter = new WaiterModule();
	protected bool _paused = false;

	void FixedUpdate() {
		if (!_paused)
			_waiter.Update (Time.deltaTime);
	}

	public void PauseWaiter() {
		_paused = true;
	}

	public void ResumeWaiter() {
		_paused = false;
	}

	public Coroutine Wait(float time) {
		return StartCoroutine (_waiter.CreateWait(time));
	}

	void OnDestroy() {
		StopAllCoroutines ();
	}

}

public class WaiterModule {

	const float DISABLE_TIME = -10f;

	WaitForFixedUpdate _waitInstruction = new WaitForFixedUpdate();

	List<float> _timers = new List<float>();

	public void Update(float t) {
		var l = _timers.Count;
		for (int i = 0; i < l; i++) {
			var time = _timers [i];
			if (time > 0f) {
				_timers [i] = time - t;
			}
		}
	}

	int AddTimer(float newTime) {
		var i = 0;
		var l = _timers.Count;
		for (; i < l; i++) {
			var time = _timers [i];
			if (time == DISABLE_TIME) {
				_timers [i] = newTime;
				return i;
			}
		}
		_timers.Add (newTime);
		return i;
	}

	public IEnumerator CreateWait(float time) {
		var i = AddTimer (time);
		while (_timers [i] > 0f) {
			yield return _waitInstruction;
		}
		_timers [i] = DISABLE_TIME;
	}

}
