using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameObjectPool : System.Object {
	
	public GameObject Prefab;
	public int InitialSize = 0;
	public int AutoExpandQuantity = 0;
	public GameObjectEvent OnCreate = new GameObjectEvent();

	List<GameObject> _pool = new List<GameObject> ();

	public void Fill() {
		while (_pool.Count < InitialSize) {
			AddNewObject ();
		}
	}

	GameObject CreateObject() {

		if (Prefab == null)
			return null;

		Prefab.SetActive(false);

		var newObject = GameObject.Instantiate(Prefab);

		OnCreate.Invoke (newObject);

		return newObject;
	}

	void AddNewObject() {
		_pool.Add (CreateObject ());
	}

	public GameObject GetObject() {

		for (var i = 0; i < _pool.Count; i++) {
			var aObject = _pool [i];
			if (!aObject.activeInHierarchy)
				return aObject;
		}

		if (AutoExpandQuantity == 0)
			return null;

		for (var i = 0; i < AutoExpandQuantity; i++) {
			AddNewObject ();
		}

		return _pool [_pool.Count - 1];
	}

	public void Drain() {
		var l = _pool.Count;
		for (var i = 0; i < l; i++) {
			GameObject.Destroy (_pool [i]);
		}
		_pool.Clear ();
	}

	public void Reclaim() {
		var l = _pool.Count;
		for (var i = 0; i < l; i++) {
			_pool [i].SetActive(false);
		}
	}

}

[Serializable]
public class ListOfPools {
	public List<GameObjectPool> Pools;

	public void Fill() {
		for (var i = 0; i < Pools.Count; i++) {
			Pools [i].Fill ();
		}
	}

	public void Drain() {
		for (var i = 0; i < Pools.Count; i++) {
			Pools [i].Drain ();
		}
	}

	public void Reclaim() {
		for (var i = 0; i < Pools.Count; i++) {
			Pools [i].Reclaim ();
		}
	}

	public int Count {
		get {
			return Pools.Count;
		}
	}

	public GameObjectPool this[int index]
	{  
		get {
			return Pools[index];
		}
		set {
			Pools[index] = value;
		}
	}

}

[Serializable]
public class DoubleListOfPools {
	public List<ListOfPools> Lists;

	public void Fill() {
		for (var i = 0; i < Lists.Count; i++) {
			Lists [i].Fill ();
		}
	}

	public void Drain() {
		for (var i = 0; i < Lists.Count; i++) {
			Lists [i].Drain ();
		}
	}

	public void Reclaim() {
		for (var i = 0; i < Lists.Count; i++) {
			Lists [i].Reclaim ();
		}
	}

	public int Count {
		get {
			return Lists.Count;
		}
	}

	public ListOfPools this[int index]
	{  
		get {
			return Lists[index];
		}
		set {
			Lists[index] = value;
		}
	}

}
