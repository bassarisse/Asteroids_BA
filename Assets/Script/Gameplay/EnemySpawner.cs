using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : Waiter {

	public float SpawnOffset = 2f;
	public bool AutoDeploy = false;
	public float MinSaucerWaitTime = 3f;
	public float MaxSaucerWaitTime = 8f;
	public float MinEnemyShipWaitTime = 5f;
	public float MaxEnemyShipWaitTime = 12f;
	public GameObject LaserTarget;
	public GameObjectPool SaucerPool;
	public GameObjectPool EnemyShipPool;
	public GameObjectPool EnemyLaserPool;
	public GameObjectPool EnemyLaserCrashPool;
	public GameObjectPool EnemyCrashPool;
	public IntEvent OnScore;

	int _enemyCount = 0;

	void Awake() {

		if (OnScore == null)
			OnScore = new IntEvent ();

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

	public void ConfigEnemy(GameObject newObject) {
		var saucerBehavior = newObject.GetComponent<SaucerBehavior> ();
		if (saucerBehavior != null) {
			if (saucerBehavior.Level > 1) {
				saucerBehavior.LaserTarget = LaserTarget;
			}
			saucerBehavior.LaserPool = EnemyLaserPool;
			saucerBehavior.LaserCrashPool = EnemyLaserCrashPool;
			saucerBehavior.CrashPool = EnemyCrashPool;
			saucerBehavior.OnStruck.AddListener (ScoreFromEnemy);
			saucerBehavior.OnDie.AddListener (ScheduleNewEnemy);
			saucerBehavior.OnGone.AddListener (ReturnEnemy);
		}
	}

	public void ConfigEnemyLaser(GameObject newObject) {
		var enemyLaserBehavior = newObject.GetComponent<EnemyLaserBehavior> ();
		if (enemyLaserBehavior != null) {
			enemyLaserBehavior.CrashPool = EnemyLaserCrashPool;
		}
	}

	void DeployEnemies(params int[] enemyLevels) {

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
		yield return Wait(Random.Range(MinEnemyShipWaitTime, MaxEnemyShipWaitTime));
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

		_enemyCount += 1;

	}

	void ScoreFromEnemy(GameObject gameObject, SaucerBehavior saucerBehavior) {
		OnScore.Invoke (saucerBehavior.Score);
	}

	void ScheduleNewEnemy(GameObject gameObject, SaucerBehavior saucerBehavior) {
		DeployEnemies (saucerBehavior.Level);
	}

	void ReturnEnemy(GameObject gameObject, SaucerBehavior saucerBehavior) {
		gameObject.SetActive (false);
		_enemyCount -= 1;
	}

	public void ResetEnemies() {
		SaucerPool.Reclaim ();
		EnemyShipPool.Reclaim ();
		_enemyCount = 0;
		DeployEnemies (1, 2);
	}

}
