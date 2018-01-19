using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waiter : MonoBehaviour {

	protected WaiterModule _waiter = new WaiterModule();

	void FixedUpdate() {
		_waiter.FixedUpdate ();
	}

	public Coroutine Wait(float time) {
		return StartCoroutine (_waiter.CreateWait(time));
	}

}

public class WaiterModule {

	const float DISABLE_TIME = -10f;

	List<float> _timers = new List<float>();

	public void FixedUpdate() {
		var t = Time.deltaTime;
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
		yield return new WaitUntil (() => _timers [i] <= 0f); // this could probably be more optimized by caching the coroutine
		_timers [i] = DISABLE_TIME;
	}

}
