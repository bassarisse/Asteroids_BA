using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectPool {

	List<GameObject> _pool;
	GameObject _baseObject;
	int _autoExpandQuantity;
	Action<GameObject> _onCreate = null;

	public ObjectPool (GameObject baseObject, int initialSize, int autoExpandQuantity = 0, Action<GameObject> onCreate = null) {
		_pool = new List<GameObject> ();
		_baseObject = baseObject;
		_autoExpandQuantity = autoExpandQuantity;
		_onCreate = onCreate;

		for (var i = 0; i < initialSize; i++) {
			_pool.Add (CreateObject ());
		}

	}

	GameObject CreateObject(GameObject baseObject = null) {
		
		var prefab = baseObject != null ? baseObject : _baseObject;
		if (prefab == null)
			return null;
		
		var newObject = GameObject.Instantiate(prefab);
		newObject.SetActive (false);
		if (_onCreate != null)
			_onCreate (newObject);
		
		return newObject;
	}

	public GameObject GetObject() {
		
		for (var i = 0; i < _pool.Count; i++) {
			var aObject = _pool [i];
			if (!aObject.activeInHierarchy)
				return aObject;
		}

		if (_autoExpandQuantity == 0)
			return null;
		
		for (var i = 0; i < _autoExpandQuantity; i++) {
			_pool.Add (CreateObject ());
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
