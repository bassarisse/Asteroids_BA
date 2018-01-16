using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VoidHit : MonoBehaviour {

	public static readonly Dictionary<GameObject, VoidHit> Cache = new Dictionary<GameObject, VoidHit>();
	public void OnEnable() { Cache.Add(gameObject, this); }
	public void OnDisable() { Cache.Remove(gameObject); }

	public UnityEvent OnHit;

	void Awake () {
		if (OnHit == null)
			OnHit = new UnityEvent ();
	}

	public void Hit() {
		OnHit.Invoke ();
	}
}
