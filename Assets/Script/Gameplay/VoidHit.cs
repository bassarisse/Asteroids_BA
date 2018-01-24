using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VoidHit : MonoBehaviour {

	public static readonly Dictionary<GameObject, VoidHit> Cache = new Dictionary<GameObject, VoidHit>();

	public static void HitAll() {

		var values = VoidHit.Cache.Values;
		var l = values.Count;
		var list = new VoidHit[l];

		using (var enumerator = values.GetEnumerator())
		{
			var i = 0;
			while (enumerator.MoveNext())
			{
				list [i] = enumerator.Current;
				i++;
			}
		}

		for (var i = 0; i < l; i++) {
			list [i].Hit ();
		}
			
	}

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
