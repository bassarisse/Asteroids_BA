﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaucerBehavior : Waiter {

	const string CRASH_SFX = "ship_crash";

	public Rigidbody2D TargetBody;
	public float PreferedScreenPercentage = 0.9f;
	public float MinSqrMagnitudeToMove = 1f;
	public int Level = 0;
	public float MinImpulseForce = 1f;
	public float MaxImpulseForce = 4f;
	public float MinWaitToShootTime = 2f;
	public float MaxWaitToShootTime = 10f;
	public GameObject LaserTarget;
	public GameObjectPool LaserPool;
	public GameObjectPool LaserCrashPool;
	public GameObjectPool CrashPool;

	public SaucerEvent OnStruck;
	public SaucerEvent OnDie;
	public SaucerEvent OnGone;

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
			var moveDirection = transform.position - LaserTarget.transform.position; 
			if (moveDirection != Vector3.zero) {
				var angle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
				transform.rotation = Quaternion.AngleAxis (angle, Vector3.forward);
				transform.Rotate (0, 0, 90);
			}
		}

	}

	IEnumerator Move() {
		while (this._isAlive) {

			yield return Wait(0.5f);
		
			if (TargetBody.velocity.sqrMagnitude >= MinSqrMagnitudeToMove)
				continue;

			var center = _camera.transform.position;
			var height = _camera.orthographicSize * 2f * PreferedScreenPercentage;
			var width = height * _camera.aspect;

			var preferedRegion = new Rect(center.x - width / 2f, center.y - height / 2f, width, height);

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
