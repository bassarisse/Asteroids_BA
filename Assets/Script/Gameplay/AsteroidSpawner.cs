using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : Waiter {
	
	[Header("General Settings")]
	public bool AutoDeploy = false;
	public float SpawnOffset = 2f;
	public float DirectionOffset = 45f;
	[Space(20)]

	[Header("Difficulty Settings")]
	public int MinAsteroids = 3;
	public int MaxAsteroids = 8;
	public float StageToAsteroidsAddFactor = 0.5f;
	[Space(20)]

	public DoubleListOfPools AsteroidPools;
	public GameObjectPool AsteroidCrashPool;
	public GameObjectEvent OnAsteroidStruck;

	int _stage = 1;
	int _asteroidCount = 0;

	void Awake() {
		
		if (OnAsteroidStruck == null)
			OnAsteroidStruck = new GameObjectEvent ();

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
			asteroidBehavior.OnStruck.AddListener (StruckAsteroid);
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

		if (!gameObject.activeInHierarchy)
			return;

		var camera = Camera.main;
		if (camera == null)
			return;

		var area = camera.rect;
		
		var posMin = camera.ViewportToWorldPoint (new Vector3 (area.xMin, area.yMin, 0));
		var posMax = camera.ViewportToWorldPoint (new Vector3 (area.xMax, area.yMax, 0));

		var qtyToDeploy = Mathf.Min(MinAsteroids + Mathf.FloorToInt (_stage * StageToAsteroidsAddFactor), MaxAsteroids);

		for (var i = 0; i < qtyToDeploy; i++) {
			var offset = 1f + Random.Range (0, SpawnOffset);
			DeployAsteroid (posMin, posMax, 0, offset);
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

	void StruckAsteroid(GameObject gameObject, AsteroidBehavior asteroidBehavior) {
		OnAsteroidStruck.Invoke (gameObject);
	}

	void DivideAsteroid(GameObject gameObject, AsteroidBehavior asteroidBehavior) {

		for (var i = 0; i < 2; i++) {
			DeployAsteroid (gameObject.transform.position, gameObject.transform.position, asteroidBehavior.Level + 1, 0f);
		}

		if (_asteroidCount == 0) {
			_stage += 1;
			DeployAsteroids ();
		}
	}

	void ReturnAsteroid(GameObject gameObject, AsteroidBehavior asteroidBehavior) {
		gameObject.SetActive (false);
		_asteroidCount -= 1;
	}

	public void ResetAsteroids() {
		AsteroidPools.Reclaim ();
		_asteroidCount = 0;
		DeployAsteroids ();
	}

}
