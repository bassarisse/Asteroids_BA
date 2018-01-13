using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VisibilityNotifier : MonoBehaviour {

	public UnityEvent OnAppear;
	public UnityEvent OnDisappear;

	void Awake () {

		if (OnAppear == null)
			OnAppear = new UnityEvent ();

		if (OnDisappear == null)
			OnDisappear = new UnityEvent ();

	}

	void OnBecameVisible() {
		OnAppear.Invoke ();
	}

	void OnBecameInvisible() {
		OnDisappear.Invoke ();
	}

}
