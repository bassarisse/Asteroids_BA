using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShipHit : MonoBehaviour {

	public static readonly Dictionary<GameObject, ShipHit> Cache = new Dictionary<GameObject, ShipHit>();
	public void OnEnable() { Cache.Add(gameObject, this); }
	public void OnDisable() { Cache.Remove(gameObject); }

	public Collider2DEvent OnHit;

	void Awake () {
		if (OnHit == null)
			OnHit = new Collider2DEvent ();
	}

	public void Hit(GameObject gameObject, Collider2D collider) {
		OnHit.Invoke (gameObject, collider);
	}
}
