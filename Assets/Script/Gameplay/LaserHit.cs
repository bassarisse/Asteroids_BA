using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LaserHit : MonoBehaviour {

	private static readonly Dictionary<LaserType, Dictionary<GameObject, LaserHit>> Cache = new Dictionary<LaserType, Dictionary<GameObject, LaserHit>>();
	
	public static Dictionary<GameObject, LaserHit> GetCache(LaserType type) {
		if (!Cache.ContainsKey(type))
			Cache.Add(type, new Dictionary<GameObject, LaserHit>());
		return Cache[type];
	}

	public void OnEnable() { GetCache(Type).Add(gameObject, this); }
	public void OnDisable() { GetCache(Type).Remove(gameObject); }

	public LaserType Type = LaserType.Friend;
	public Collider2DEvent OnHit;

	void Awake () {
		if (OnHit == null)
			OnHit = new Collider2DEvent ();
	}

	public void Hit(GameObject gameObject, Collider2D collider) {
		OnHit.Invoke (gameObject, collider);
	}
}
