using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseTrigger : MonoBehaviour {

	public bool ReactWhilePlaying = true;
	public bool ReactWhilePaused = true;

	bool _paused = false;
	bool _scheduledStateChange = false;

	void Update() {

		if (InputExtensions.Pressed.Start) {
			if (_paused && ReactWhilePaused || !_paused && ReactWhilePlaying)
				PauseOrResume ();
		}

	}

	public void PauseOrResume() {
		if (_scheduledStateChange)
			return;
		_scheduledStateChange = true;
		StartCoroutine (SchedulePauseOrResume());
	}

	IEnumerator SchedulePauseOrResume() {
		yield return new WaitForEndOfFrame ();
		ExecutePauseOrResume ();
		_scheduledStateChange = false;
	}

	void ExecutePauseOrResume() {
		_paused = !_paused;

		using (var enumerator = PauseEffect.Cache.Values.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				var item = enumerator.Current;
				if (_paused)
					item.Pause ();
				else
					item.Resume ();
			}
		}

	}

}
