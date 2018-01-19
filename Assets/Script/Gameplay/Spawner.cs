using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
	
	public float SpawnOffset = 2f;
	public float DirectionOffset = 45f;
	public int MaxAsteroids = 10;
	public int Stage = 1;
	public bool AutoDeploy = false;
	public DoubleListOfPools AsteroidPools;
	public GameObjectPool AsteroidCrashPool;
	public IntEvent OnScore;

	int _asteroidCount = 0;

	void Awake() {
		
		if (OnScore == null)
			OnScore = new IntEvent ();

		AsteroidPools.Fill ();
		AsteroidCrashPool.Fill ();

	}

	void Start () {
		if (AutoDeploy)
			DeployAsteroids();
	}

	public void ConfigAsteroid(GameObject newObject) {
		var asteroidBehavior = newObject.GetComponent<AsteroidBehavior> ();
		if (asteroidBehavior != null) {
			asteroidBehavior.CrashPool = AsteroidCrashPool;
			asteroidBehavior.OnStruck.AddListener (ScoreFromAsteroid);
			asteroidBehavior.OnDie.AddListener (DivideAsteroid);
			asteroidBehavior.OnGone.AddListener (ReturnAsteroid);
		}
	}

	GameObject GetAsteroid(int level) {

		var maxLevel = AsteroidPools.Count - 1;
		if (level > maxLevel)
			return null;

		var listsOfPools = AsteroidPools [level];
		var typeIndex = Random.Range (0, listsOfPools.Count - 1);

		var pool = listsOfPools[typeIndex];

		return pool.GetObject ();
	}

	void DeployAsteroids() {

		var camera = Camera.main;
		if (camera == null)
			return;

		var area = camera.rect;
		
		var posMin = camera.ViewportToWorldPoint (new Vector3 (area.xMin, area.yMin, 0));
		var posMax = camera.ViewportToWorldPoint (new Vector3 (area.xMax, area.yMax, 0));

		var qtyToDeploy = Mathf.Min(4 + Mathf.FloorToInt (Stage / 2), MaxAsteroids);

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

	void ScoreFromAsteroid(GameObject gameObject, AsteroidBehavior asteroidBehavior) {
		OnScore.Invoke (asteroidBehavior.Score);
	}

	void DivideAsteroid(GameObject gameObject, AsteroidBehavior asteroidBehavior) {

		for (var i = 0; i < 2; i++) {
			DeployAsteroid (gameObject.transform.position, gameObject.transform.position, asteroidBehavior.Level + 1, 0f);
		}

		if (_asteroidCount == 0) {
			Stage += 1;
			DeployAsteroids ();
		}
	}

	void ReturnAsteroid(GameObject gameObject, AsteroidBehavior asteroidBehavior) {
		gameObject.SetActive (false);
		_asteroidCount -= 1;
	}

	public void ResetAsteroids() {
		for (var l = 0; l < AsteroidPools.Count; l++) {
			var poolsPerLevel = AsteroidPools [l];
			for (var t = 0; t < poolsPerLevel.Count; t++) {
				var pool = poolsPerLevel [t];
				pool.Reclaim ();
			}
		}
		_asteroidCount = 0;
		DeployAsteroids ();
	}

}
