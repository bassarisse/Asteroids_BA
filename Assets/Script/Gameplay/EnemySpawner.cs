using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : Waiter {

	[Header("General Settings")]
	public bool AutoDeploy = false;
	public float SpawnOffset = 2f;
	public GameObject LaserTarget;
	[Space(20)]

	[Header("Difficulty Settings")]
	public float ScoreToDifficultyFactor = 0.001f;
	public float MinDifficultyLevel = 1f;
	public float MaxDifficultyLevel = 15f;
	public float MinSaucerWaitTime = 3f;
	public float MaxSaucerWaitTime = 8f;
	public float MinEnemyShipWaitTime = 5f;
	public float MaxEnemyShipWaitTime = 12f;
	[Space(20)]

	[Header("Object Pools")]
	public GameObjectPool SaucerPool;
	public GameObjectPool EnemyShipPool;
	public GameObjectPool EnemyLaserPool;
	public GameObjectPool EnemyLaserCrashPool;
	public GameObjectPool EnemyCrashPool;
	public GameObjectEvent OnEnemyStruck;

	float _difficultyLevel = 0;

	void Awake() {

		if (OnEnemyStruck == null)
			OnEnemyStruck = new GameObjectEvent ();

		SaucerPool.Fill ();
		EnemyShipPool.Fill ();
		EnemyLaserPool.Fill ();
		EnemyLaserCrashPool.Fill ();
		EnemyCrashPool.Fill ();

	}

	void Start () {
		if (AutoDeploy)
			DeployEnemies(1, 2);
	}

	public void IncreaseDifficultyFromScore(int score) {
		_difficultyLevel = Mathf.Min(MaxDifficultyLevel, score * ScoreToDifficultyFactor);
		_difficultyLevel = Mathf.Max(MinDifficultyLevel, _difficultyLevel);
	}

	public void ConfigEnemy(GameObject newObject) {
		var saucerBehavior = newObject.GetComponent<SaucerBehavior> ();
		if (saucerBehavior != null) {
			saucerBehavior.LaserTarget = saucerBehavior.Level > 1 ? LaserTarget : null;
			saucerBehavior.LaserPool = EnemyLaserPool;
			saucerBehavior.LaserCrashPool = EnemyLaserCrashPool;
			saucerBehavior.CrashPool = EnemyCrashPool;
			saucerBehavior.OnStruck.AddListener (StruckEnemy);
			saucerBehavior.OnDie.AddListener (ScheduleNewEnemy);
			saucerBehavior.OnGone.AddListener (ReturnEnemy);
		}
	}

	public void ConfigEnemyLaser(GameObject newObject) {
		var enemyLaserBehavior = newObject.GetComponent<LaserBehavior> ();
		if (enemyLaserBehavior != null) {
			enemyLaserBehavior.CrashPool = EnemyLaserCrashPool;
		}
	}

	void DeployEnemies(params int[] enemyLevels) {

		if (!gameObject.activeInHierarchy)
			return;

		var camera = Camera.main;
		if (camera == null)
			return;

		var area = camera.rect;

		var posMin = camera.ViewportToWorldPoint (new Vector3 (area.xMin, area.yMin, 0));
		var posMax = camera.ViewportToWorldPoint (new Vector3 (area.xMax, area.yMax, 0));

		for (var i = 0; i < enemyLevels.Length; i++) {
			var level = enemyLevels [i];
			var offset = 1f + Random.Range (0, SpawnOffset);

			if (level == 1)
				StartCoroutine (DeploySaucer (posMin, posMax, offset));
			else if (level == 2)
				StartCoroutine (DeployEnemyShip (posMin, posMax, offset));
			
		}

	}

	IEnumerator DeploySaucer(Vector3 posMin, Vector3 posMax, float offset) {
		yield return Wait(Random.Range(MinSaucerWaitTime, MinSaucerWaitTime));
		DeployEnemy (SaucerPool.GetObject (), posMin, posMax, offset);
	}

	IEnumerator DeployEnemyShip(Vector3 posMin, Vector3 posMax, float offset) {
		yield return Wait(Random.Range(MinEnemyShipWaitTime / _difficultyLevel, MaxEnemyShipWaitTime / _difficultyLevel));
		DeployEnemy (EnemyShipPool.GetObject (), posMin, posMax, offset);
	}

	void DeployEnemy(GameObject enemy, Vector3 posMin, Vector3 posMax, float offset) {

		if (enemy == null)
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

		enemy.transform.position = position;
		enemy.SetActive (true);

	}

	void StruckEnemy(GameObject gameObject, SaucerBehavior saucerBehavior) {
		OnEnemyStruck.Invoke (gameObject);
	}

	void ScheduleNewEnemy(GameObject gameObject, SaucerBehavior saucerBehavior) {
		DeployEnemies (saucerBehavior.Level);
	}

	void ReturnEnemy(GameObject gameObject, SaucerBehavior saucerBehavior) {
		gameObject.SetActive (false);
	}

	public void ResetEnemies() {
		StopAllCoroutines ();
		SaucerPool.Reclaim ();
		EnemyShipPool.Reclaim ();
		DeployEnemies (1, 2);
	}

}
