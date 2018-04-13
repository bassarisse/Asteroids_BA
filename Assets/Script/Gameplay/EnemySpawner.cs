using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : Waiter {

	[Header("General Settings")]
	public bool AutoDeploy = false;
	public float SpawnOffset = 2f;
	public GameObject DefaultEnemyTarget;
	[Space(20)]

	[Header("Difficulty Settings")]
	public float ScoreToDifficultyFactor = 0.001f;
	public float MinDifficultyLevel = 1f;
	public float MaxDifficultyLevel = 15f;
	[Space(20)]

	[Header("Events")]
	public GameObjectEvent OnEnemyStruck;
	[Space(20)]

	[Header("Object Pools")]
	public GameObjectPool EnemyLaserPool;
	public GameObjectPool EnemyLaserCrashPool;
	public GameObjectPool EnemyCrashPool;
	[Space(20)]

	public List<EnemyData> Enemies;

	float _difficultyLevel = 0;

	void Awake() {

		if (OnEnemyStruck == null)
			OnEnemyStruck = new GameObjectEvent ();

		foreach	(var enemyData in Enemies) {
			enemyData.Pool.Fill();
		}

		EnemyLaserPool.Fill ();
		EnemyLaserCrashPool.Fill ();
		EnemyCrashPool.Fill ();

	}

	void Start () {
		if (AutoDeploy)
			DeployEnemies(Enemies.ToArray());
	}

	public void IncreaseDifficultyFromScore(int score) {
		_difficultyLevel = Mathf.Min(MaxDifficultyLevel, score * ScoreToDifficultyFactor);
		_difficultyLevel = Mathf.Max(MinDifficultyLevel, _difficultyLevel);
	}

	public void ConfigEnemy(GameObject newObject) {
		var enemyBehavior = newObject.GetComponent<EnemyBehavior> ();
		if (enemyBehavior != null) {
			enemyBehavior.LaserTarget = enemyBehavior.WantsTarget ? DefaultEnemyTarget : null;
			enemyBehavior.LaserPool = EnemyLaserPool;
			enemyBehavior.LaserCrashPool = EnemyLaserCrashPool;
			enemyBehavior.CrashPool = EnemyCrashPool;
			enemyBehavior.OnStruck.AddListener (StruckEnemy);
			enemyBehavior.OnDie.AddListener (ScheduleNewEnemy);
			enemyBehavior.OnGone.AddListener (ReturnEnemy);
		}
	}

	public void ConfigEnemyLaser(GameObject newObject) {
		var enemyLaserBehavior = newObject.GetComponent<LaserBehavior> ();
		if (enemyLaserBehavior != null) {
			enemyLaserBehavior.CrashPool = EnemyLaserCrashPool;
		}
	}

	void DeployEnemies(params EnemyData[] enemyDataList) {

		if (!gameObject.activeInHierarchy)
			return;

		var camera = Camera.main;
		if (camera == null)
			return;

		var area = camera.rect;

		var posMin = camera.ViewportToWorldPoint (new Vector3 (area.xMin, area.yMin, 0));
		var posMax = camera.ViewportToWorldPoint (new Vector3 (area.xMax, area.yMax, 0));

		for (var i = 0; i < enemyDataList.Length; i++) {
			var enemyData = enemyDataList [i];
			var offset = 1f + Random.Range (0, SpawnOffset);

			StartCoroutine (ScheduleEnemyDeploy(enemyData, posMin, posMax, offset));
		}

	}

	IEnumerator ScheduleEnemyDeploy(EnemyData enemyData, Vector3 posMin, Vector3 posMax, float offset) {

		float minTime = enemyData.MinSpawnWaitTime;
		float maxTime = enemyData.MaxSpawnWaitTime;

		if (enemyData.AffectedByDifficultyLevel) {
			minTime /= _difficultyLevel;
			maxTime /= _difficultyLevel;
		}
		
		yield return Wait(Random.Range(minTime, maxTime));
		DeployEnemy (enemyData.Pool.GetObject (), posMin, posMax, offset);
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

	void StruckEnemy(GameObject gameObject, EnemyBehavior enemyBehavior) {
		OnEnemyStruck.Invoke (gameObject);
	}

	void ScheduleNewEnemy(GameObject gameObject, EnemyBehavior enemyBehavior) {
		foreach (var enemyData in Enemies)
		{
			if (enemyData.Pool.Contains(gameObject)) {
				DeployEnemies(enemyData);
				return;
			}
		}
	}

	void ReturnEnemy(GameObject gameObject, EnemyBehavior enemyBehavior) {
		gameObject.SetActive (false);
	}

	public void ResetEnemies() {
		StopAllCoroutines ();

		foreach (var enemyData in Enemies)
		{
			enemyData.Pool.Reclaim ();
		}

		DeployEnemies (Enemies.ToArray());
	}

}

[System.Serializable]
public class EnemyData : System.Object {

	public float MinSpawnWaitTime = 3f;
	public float MaxSpawnWaitTime = 8f;
	public bool AffectedByDifficultyLevel = false;
	public GameObjectPool Pool;

}
