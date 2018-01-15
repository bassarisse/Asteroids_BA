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
	public IntEvent OnScore;

	List<List<ObjectPool>> _asteroidPools;
	int _stage;
	int _asteroidCount = 0;

	void Awake() {
		if (OnScore == null)
			OnScore = new IntEvent ();
	}

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

		var qtyToDeploy = 4 + Mathf.Floor (_stage / 2);

		for (var i = 0; i < qtyToDeploy; i++) {
			DeployAsteroid (posMin, posMax, 0, 1f + Random.Range (0, SpawnOffset));
		}

	}

	void DeployAsteroid(Vector3 posMin, Vector3 posMax, int level, float offset) {
		
		var asteroid = GetAsteroid (level);
		if (asteroid == null)
			return;

		var deployOnSides = RandomExtensions.Bool ();
		var deployOnLeft = RandomExtensions.Bool ();

		var ax1Min = deployOnSides ? posMin.x : posMin.y;
		var ax1Max = deployOnSides ? posMax.x : posMax.y;
		var ax2Min = deployOnSides ? posMin.y : posMin.x;
		var ax2Max = deployOnSides ? posMax.y : posMax.x;

		var ax1 = deployOnLeft ? ax1Min - offset : ax1Max + offset;
		var quarter = (ax2Max - ax2Min) / 4f;
		var ax2 = ax2Min + quarter + quarter * 2f * Random.value;

		var position = new Vector3(deployOnSides ? ax1 : ax2, deployOnSides ? ax2 : ax1, 0);

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

		_asteroidCount += 1;

	}

	void CreateObjectPools() {

		_asteroidPools = new List<List<ObjectPool>> ();

		for (var a = 0; a < AsteroidObjects.Count; a++) {
			var levelTypes = AsteroidObjects [a].Objects;
			var newTypePool = new List<ObjectPool> ();

			for (var l = 0; l < levelTypes.Count; l++) {
				var prefab = levelTypes [l];
				newTypePool.Add (new ObjectPool (prefab, 3, 2, ConfigAsteroid));
			}

			_asteroidPools.Add (newTypePool);
		}
	}

	void ConfigAsteroid(GameObject newObject) {
		var asteroidBehavior = newObject.GetComponent<AsteroidBehavior> ();
		if (asteroidBehavior != null) {
			asteroidBehavior.OnStruck.AddListener (ScoreFromAsteroid);
			asteroidBehavior.OnDie.AddListener (ReturnAsteroid);
		}
	}

	GameObject GetAsteroid(int level) {
		
		var maxLevel = AsteroidObjects.Count - 1;
		if (level > maxLevel)
			return null;

		var objects = AsteroidObjects [level];
		var typeIndex = Random.Range (0, objects.Objects.Count - 1);

		var pool = _asteroidPools[level][typeIndex];

		return pool.GetObject ();
	}

	void ScoreFromAsteroid(GameObject gameObject, AsteroidBehavior asteroidBehavior) {
		OnScore.Invoke (asteroidBehavior.Score);
	}

	void ReturnAsteroid(GameObject gameObject, AsteroidBehavior asteroidBehavior) {
		gameObject.SetActive (false);
		_asteroidCount -= 1;

		for (var i = 0; i < 2; i++) {
			DeployAsteroid (gameObject.transform.position, gameObject.transform.position, asteroidBehavior.Level + 1, 0f);
		}

		if (_asteroidCount == 0) {
			_stage += 1;
			DeployAsteroids ();
		}
	}

	void Update () {
		
	}

}
