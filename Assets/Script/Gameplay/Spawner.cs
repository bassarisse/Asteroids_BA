using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ListOfObjects {
	public List<GameObject> Objects;
}

public class Spawner : MonoBehaviour {
	
	public float SpawnOffset = 2f;
	public float DirectionOffset = 45f;
	public List<ListOfObjects> AsteroidObjects;

	List<List<List<GameObject>>> _asteroidPools;
	int _stage;

	// Use this for initialization
	void Start () {

		_stage = 1;

		CreateObjectPools ();
		DeployAsteroids ();
		
	}

	void DeployAsteroids() {

		var camera = Camera.main;
		if (camera == null)
			return;

		var area = camera.rect;
		
		var posMin = camera.ViewportToWorldPoint (new Vector3 (area.xMin, area.yMin, 0));
		var posMax = camera.ViewportToWorldPoint (new Vector3 (area.xMax, area.yMax, 0));

		for (var i = 0; i < 5; i++) {
			DeployAsteroid (posMin, posMax);
		}

	}

	void DeployAsteroid(Vector3 posMin, Vector3 posMax) {

		var deployOnSides = RandomExtensions.Bool ();
		var deployOnLeft = RandomExtensions.Bool ();

		var ax1Min = deployOnSides ? posMin.x : posMin.y;
		var ax1Max = deployOnSides ? posMax.x : posMax.y;
		var ax2Min = deployOnSides ? posMin.y : posMin.x;
		var ax2Max = deployOnSides ? posMax.y : posMax.x;

		var offset = 1f + Random.Range (0, SpawnOffset);
		var ax1 = deployOnLeft ? ax1Min - offset : ax1Max + offset;
		var quarter = (ax2Max - ax2Min) / 4f;
		var ax2 = ax2Min + quarter + quarter * 2f * Random.value;

		var position = new Vector3(deployOnSides ? ax1 : ax2, deployOnSides ? ax2 : ax1, 0);
		var asteroid = GetAsteroid (0);

		Vector3 direction;
		if (deployOnSides) {
			direction = deployOnLeft ? Vector3.right : Vector3.left;
		} else {
			direction = deployOnLeft ? Vector3.up : Vector3.down;
		}

		asteroid.transform.position = position;
		asteroid.transform.rotation = Quaternion.FromToRotation (Vector3.up, direction);
		asteroid.transform.Rotate (0, 0, Random.Range (-DirectionOffset, DirectionOffset));
		asteroid.SetActive (true);

	}

	void CreateObjectPools() {

		_asteroidPools = new List<List<List<GameObject>>> ();

		for (var a = 0; a < AsteroidObjects.Count; a++) {
			var levelTypes = AsteroidObjects [a].Objects;
			var newTypePool = new List<List<GameObject>> ();

			for (var l = 0; l < levelTypes.Count; l++) {
				var prefab = levelTypes [l];
				var newPool = new List<GameObject> ();

				for (int i = 0; i < 3; i++) {
					newPool.Add (CreateObject (prefab));
				}

				newTypePool.Add (newPool);
			}

			_asteroidPools.Add (newTypePool);
		}
	}

	GameObject CreateObject(GameObject newObject) {
		var bullet = Instantiate (newObject);
		bullet.SetActive (false);
		return bullet;
	}

	GameObject GetAsteroid(int level) {
		var typeIndex = Random.Range (0, AsteroidObjects.Count - 1);
		var pool = _asteroidPools[level][typeIndex];
		for (var i = 0; i < pool.Count; i++) {
			var newObject = pool [i];
			if (!newObject.activeInHierarchy)
				return newObject;
		}
		for (var i = 0; i < 2; i++) {
			pool.Add (CreateObject (AsteroidObjects[level].Objects[typeIndex]));
		}
		return pool [pool.Count - 1];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
