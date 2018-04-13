﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaucerBehavior : Waiter {

	const string CRASH_SFX = "ship_crash";
	const float TARGET_ANGLE_FIX = -90f;

	[Header("References")]
	public Rigidbody2D TargetBody;
	[Space(20)]

	[Header("General parameters")]
	public int Level = 0;
	public float PreferedScreenPercentage = 0.9f;
	public float MinSqrMagnitudeToMove = 1f;
	public float MinImpulseForce = 1f;
	public float MaxImpulseForce = 4f;
	public float MinWaitToShootTime = 2f;
	public float MaxWaitToShootTime = 10f;
	[Space(20)]

	[Header("Targeting")]
	public GameObject LaserTarget;
	public float LaserTargetScreenPercentage = 1.1f;
	public float LaserTargetMaxOffset = 15f;
	public float LaserTargetRate = 0.95f;
	[Space(20)]

	[Header("Events")]
	public SaucerEvent OnStruck;
	public SaucerEvent OnDie;
	public SaucerEvent OnGone;
	[Space(20)]

	[Header("Object Pools")]
	public GameObjectPool LaserPool;
	public GameObjectPool LaserCrashPool;
	public GameObjectPool CrashPool;

	bool _isAlive = false;
	Camera _camera;

	void Awake() {

		AudioHandler.Load (CRASH_SFX);

		if (OnStruck == null)
			OnStruck = new SaucerEvent ();
		if (OnDie == null)
			OnDie = new SaucerEvent ();
		if (OnGone == null)
			OnGone = new SaucerEvent ();

		LaserPool.Fill ();
		LaserCrashPool.Fill ();
		CrashPool.Fill ();

		_camera = Camera.main;
		
	}

	void OnEnable() {
		_isAlive = true;
		StartCoroutine (Move ());
		StartCoroutine (ShootLaser ());
	}

	void Update() {

		if (LaserTarget != null) {

			var targetRegion = UnityExtensions.CreateRectFromCamera(_camera, LaserTargetScreenPercentage);

			if (targetRegion.Contains(LaserTarget.transform.position)) {
				var direction = LaserTarget.transform.position - transform.position; 
				if (direction != Vector3.zero) {

					var offsetAngle = Mathf.Sin(Time.timeSinceLevelLoad) * LaserTargetMaxOffset;
					var targetAngle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;

					var fixedTargetAngle = UnityExtensions.ClampAngle(targetAngle + TARGET_ANGLE_FIX);
					var currentAngle = UnityExtensions.ClampAngle(transform.rotation.eulerAngles.z);
					var angleDifference = UnityExtensions.ClampAngle(fixedTargetAngle - currentAngle + offsetAngle);
					
					var finalAngle = currentAngle + angleDifference * Time.deltaTime * 5f * LaserTargetRate;

					transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);
				}
			}
			
		}

	}

	IEnumerator Move() {
		while (this._isAlive) {

			yield return Wait(0.5f);
		
			if (TargetBody.velocity.sqrMagnitude >= MinSqrMagnitudeToMove)
				continue;

			var preferedRegion = UnityExtensions.CreateRectFromCamera(_camera, PreferedScreenPercentage);

			Vector2 moveDirection;

			if (preferedRegion.Contains(transform.position)) {
				moveDirection = Random.insideUnitCircle;
			} else {
				var targetPosition = RandomExtensions.PointInsideRect(preferedRegion);
				moveDirection = targetPosition - new Vector2(transform.position.x, transform.position.y);
			}
			
			var impulse = Random.Range (MinImpulseForce, MaxImpulseForce);
			this.TargetBody.AddForce (moveDirection.normalized * impulse, ForceMode2D.Impulse);

		}
	}

	IEnumerator ShootLaser() {
		while (this._isAlive) {

			var waitTime = Random.Range (MinWaitToShootTime, MaxWaitToShootTime);
			yield return Wait(waitTime);

			if (!this._isAlive)
				yield break;

			var impulse = Random.Range (MinImpulseForce, MaxImpulseForce);

			this.TargetBody.AddForce (Random.insideUnitCircle.normalized * impulse, ForceMode2D.Impulse);

			var laser = LaserPool.GetObject ();
			if (laser == null)
				yield break;

			laser.transform.rotation = transform.rotation;
			if (LaserTarget == null)
				laser.transform.Rotate (0, 0, Random.Range (0, 360));

			laser.transform.position = transform.position + laser.transform.up * 0.55f;
			laser.SetActive (true);

		}
	}

	void OnTriggerEnter2D(Collider2D collider) {
		var cache = SaucerHit.Cache;
		var key = collider.gameObject;
		if (cache.ContainsKey(key))
			cache [key].Hit (gameObject, collider);
	}

	public void Struck() {
		OnStruck.Invoke (gameObject, this);
		Die ();
	}

	public void Die() {
		Gone ();
		OnDie.Invoke (gameObject, this);
	}

	public void Gone() {

		this._isAlive = false;

		AudioHandler.Play (CRASH_SFX, 0.6f);

		var crashParticle = CrashPool.GetObject ();
		if (crashParticle != null) {
			crashParticle.transform.position = transform.position;
			crashParticle.SetActive (true);
		}

		StopAllCoroutines ();
		
		OnGone.Invoke (gameObject, this);

	}

}
